using ClientCore.Signaling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace PeerCC.Signaling
{
    /// <summary>
    /// HttpSignaler instance is used to fire connection events.
    /// </summary>
    public class HttpSignaler : HttpSignalerEvents, ISignaler
    {
        #region Signaling server config
        private static string _url = "http://peercc-server.ortclib.org";
        private static int _port = 8888;
        #endregion

        private static HttpSignaler instance = null;
        private static readonly object InstanceLock = new object();

        public static HttpSignaler Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new HttpSignaler();

                    return instance;
                }
            }
        }

        private readonly HttpClient _httpClient = new HttpClient();
        private State _state;
        private Uri _baseHttpAddress;
        private int _myId;
        private string _clientName;
        public ObservableCollection<Peer> _peers = new ObservableCollection<Peer>();
        private ManualResetEvent _sendEvent = new ManualResetEvent(false);
        private ConcurrentQueue<Tuple<int, string>> _sendMessageQueue = new ConcurrentQueue<Tuple<int, string>>();
        private Thread _sendThread;

        public HttpSignaler()
        {
            _state = State.NotConnected;
            _myId = -1;
            _clientName = LocalPeer.Name;
            _baseHttpAddress = new Uri(_url + ":" + _port);
        }

        /// <summary>
        /// Checks if connected to the server.
        /// </summary>
        /// <returns>True if connected to the server.</returns>
        public bool IsConnected()
        {
            return _myId != -1;
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        public async Task<bool> Connect()
        {
            try
            {
                if (_state != State.NotConnected)
                {
                    OnServerConnectionFailure();
                    return true;
                }
                _state = State.SigningIn;
                await SendSignInRequestAsync();
                if (_state == State.Connected)
                {
                    if (_sendThread == null)
                    {
                        // Upon connection succeeding and no sender thread
                        // being present, spawn a thread dedicated to
                        // delivering messages in order the messages were
                        // created. The thread waits for a message to be
                        // queued, and uses a thread safe queue to process
                        // any and all outstanding messages to be sent to
                        // a server until the queue is exhausted where the
                        // thread goes back to sleep waiting for the next
                        // message to be queued.
                        _sendThread = new Thread(() =>
                        {
                            while (true)
                            {
                                _sendEvent.WaitOne();
                                _sendEvent.Reset();
                                Tuple<int, string> peerMessageTuple;
                                while (_sendMessageQueue.TryDequeue(out peerMessageTuple))
                                {
                                    if (-1 == peerMessageTuple.Item1) break;    // quit thread
                                    SendToPeerAsync(peerMessageTuple.Item1, peerMessageTuple.Item2).Wait();
                                }
                                Thread.Yield();
                            }
                        });

                        _sendThread.Start();
                    }

                    // The connect routine is called from the GUI thread. The
                    // SendWaitRequestAsync() loops indefinitely performing
                    // HTTP requests to fetch data from the signaling server.
                    // Spawn a separate thread to prevent this forever loop
                    // from blocking the GUI thread from processing other
                    // events.
                    Thread thread = new Thread(() =>
                    {
                        // The .SendWaitRequestAsync() is performed in a loop
                        // indefinitely until the connection is closed. The
                        // thread is now allowed to complete by virtue of the
                        // .Wait() causing the send routine to quit before
                        // the end of the thread scope completes.
                        SendWaitRequestAsync().Wait();
                    });

                    thread.Start();
                    return true;
                }

                _state = State.NotConnected;
                OnServerConnectionFailure();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] Signaling: Failed to connect to server: " + ex.Message);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Sends "sign_in" request to the server and waits for response.
        /// </summary>
        /// <returns>False if there is a failure, otherwise returns true.</returns>
        private async Task<bool> SendSignInRequestAsync()
        {
            try
            {
                string request = string.Format("sign_in?" + _clientName);

                // Send the request, await response
                HttpResponseMessage response = await _httpClient.GetAsync(_baseHttpAddress + request);
                HttpStatusCode status_code = response.StatusCode;

                string result = await response.Content.ReadAsStringAsync();
                if (result == null)
                    return false;
                int content_length = result.Length;

                string peer_name;
                int peer_id, peer_connected;
                if (!ParseServerResponse(result, status_code,
                    out peer_name, out peer_id, out peer_connected))
                    return false;

                if (_myId == -1)
                {
                    Debug.Assert(_state == State.SigningIn);
                    _myId = peer_id;
                    Debug.Assert(_myId != -1);

                    if (content_length > 0)
                    {
                        if (!ParseServerResponseAddPeersToList(result, status_code))
                            return false;

                        OnSignedIn();
                    }
                }
                else if (_state == State.SigningOut)
                {
                    Close();
                    OnDisconnected();
                }
                else if (_state == State.SigningOutWaiting)
                {
                    await SignOut();
                }
                if (_state == State.SigningIn)
                {
                    _state = State.Connected;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] Signaling SendSignInRequestAsync: Failed to connect to server: " + ex.Message);
                await SignOut();
                return false;
            }
        }

        public override void SendToPeer(int peer_id, string message)
        {
            // A message is queued to deliver to the server in order the
            // messages are created. This prevents the server from
            // accidentally receiving a message sent later because an
            // earlier HTTP request was delayed before the next HTTP
            // request was able to succeed.
            _sendMessageQueue.Enqueue(new Tuple<int, string>(peer_id, message));
            _sendEvent.Set();
        }

        private async Task<bool> SendToPeerAsync(int peer_id, string message)
        {
            try
            {
                if (_state != State.Connected)
                    return false;

                Debug.Assert(IsConnected());

                if (!IsConnected() || peer_id == -1)
                    return false;

                string request =
                    string.Format(
                        "message?peer_id={0}&to={1} HTTP/1.0",
                        _myId, peer_id);

                var content = new StringContent(message, System.Text.Encoding.UTF8, "application/json");

                Debug.WriteLine("Sending to remote peer: " + request + " " + message);

                // Send request, await response
                HttpResponseMessage response = await _httpClient.PostAsync(
                    _baseHttpAddress + request, content);

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] Signaling SendToPeer: " + ex.Message);
            }
            return true;
        }

        /// <summary>
        /// Disconnects the user from the server.
        /// </summary>
        /// <returns>True if the user is disconnected from the server.</returns>
        public async Task<bool> SignOut()
        {
            if (_state == State.NotConnected || _state == State.SigningOut)
                return true;
            _state = State.SigningOut;

            if (_myId != -1)
            {
                string request = string.Format("sign_out?peer_id={0}", _myId);

                // Send request, await response
                HttpResponseMessage response = await _httpClient.GetAsync(
                    _baseHttpAddress + request);
            }
            else
                // Can occur if the app is closed before we finish connecting
                return true;

            _peers.Clear();
            _myId = -1;
            _state = State.NotConnected;

            // By putting an invalid peer ID and null message into the queue,
            // the send message thread is signaled to quit.
            _sendMessageQueue.Enqueue(new Tuple<int, string>(-1, null));
            _sendEvent.Set();

            // The spawned sender thread is no longer usable as it will quit.
            _sendThread = null;

            return true;
        }

        public void Close()
        {
            _peers.Clear();
            _state = State.NotConnected;
        }

        /// <summary>
        /// Add connected peer to the list, remove disconnected peer from the list
        /// </summary>
        private void AddOrRemovePeerFromList(string peer_name, int peer_id, int peer_connected)
        {
            var peer = new Peer(peer_id, peer_name);

            if (peer_connected == 1)
            {
                if (peer_name != LocalPeer.Name)
                    AddPeerToList(peer);

                OnPeerConnected(peer);
            }
            else if (peer_connected == 0)
            {
                RemovePeerFromList(peer_id);

                OnPeerDisconnected(peer);
            }
        }

        private int ParseHeaderGetPragma(HttpResponseHeaders header)
        {
            string strHeader = header.ToString();
            string[] separatingChars = { "Pragma: " };
            string[] words = strHeader.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

            string pr = words[1];
            int separator = pr.IndexOf("\n");
            string strPragma = pr.Substring(0, separator + 1);

            return strPragma.ParseLeadingInt();
        }

        private bool ParseServerResponse(string content, HttpStatusCode status_code,
            out string peer_name, out int peer_id, out int peer_connected)
        {
            peer_name = "";
            peer_id = -1;
            peer_connected = 0;
            try
            {
                if (status_code != HttpStatusCode.OK)
                {
                    if (status_code == HttpStatusCode.InternalServerError)
                    {
                        Debug.WriteLine("[Error] Signaling ParseServerResponse: " + status_code);
                        OnPeerDisconnected(new Peer(0, string.Empty));
                        return false;
                    }
                    Close();
                    _myId = -1;
                    return false;
                }

                string[] separatingChars = { "," };
                string[] words = content.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                peer_name = words[0];
                peer_id = words[1].ParseLeadingInt();
                peer_connected = words[2].ParseLeadingInt();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] Failed to parse server response (ex=" + ex.Message +
                    ")! Content(" + content.Length + ")=<" + content + ">");
                return false;
            }
        }

        private bool ParseServerResponseAddPeersToList(string content, HttpStatusCode status_code)
        {
            try
            {
                if (status_code != HttpStatusCode.OK)
                {
                    if (status_code == HttpStatusCode.InternalServerError)
                    {
                        Debug.WriteLine("[Error] Signaling ParseServerResponseSignIn: " + status_code);
                        OnPeerDisconnected(new Peer(0, string.Empty));
                        return false;
                    }
                    Close();
                    _myId = -1;
                    return false;
                }
                string[] separatingCharacter = { "\n" };
                string[] stringPeer = content.Split(separatingCharacter, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in stringPeer)
                {
                    string[] separatingChars = { "," };
                    string[] words = s.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                    string peer_name = words[0];
                    int peer_id = words[1].ParseLeadingInt();

                    Peer connectedPeer = new Peer(peer_id, peer_name);

                    if (peer_name != LocalPeer.Name)
                        AddPeerToList(connectedPeer);

                    OnPeerConnected(new Peer(peer_id, peer_name));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] Failed to parse server response (ex=" + ex.Message +
                    ")! Content(" + content.Length + ")=<" + content + ">");
                return false;
            }
            return true;
        }

        public void AddPeerToList(Peer peer)
        {
            _peers.Add(peer);
        }

        public void RemovePeerFromList(int peerId)
        {
            _peers.Remove(p => p.Id == peerId);
        }

        /// <summary>
        /// Send a message to a peer.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Returns when message is delivered or an
        /// exception if the message could not be delivered.
        public async Task SentToPeerAsync(Message message)
        {
            try
            {
                if (_state != State.Connected)
                    return;

                Debug.Assert(IsConnected());

                if (!IsConnected() || message.PeerId == "")
                    return;

                string request =
                    string.Format(
                        "message?peer_id={0}&to={1}" +
                        "Content-Length: {2}\r\n" +
                        "Content-Type: text/plain\r\n" +
                        "\r\n" +
                        "{3}",
                    _myId, message.PeerId, message.Content.Length, message.Content);

                var content = new StringContent(message.Content, System.Text.Encoding.UTF8, "application/json");

                Debug.WriteLine("Sending to remote peer id: " + message.PeerId);
                Debug.WriteLine("Message id: " + message.Id + ", content: " + message.Content);
                Debug.WriteLine("Formated request: " + request);

                // Send request, await response
                HttpResponseMessage response = await _httpClient.PostAsync(
                    _baseHttpAddress + request, content);

                if (response.StatusCode != HttpStatusCode.OK)
                    return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] Signaling SendToPeer: " + ex.Message);
            }
        }

        private List<Message> messagesList = new List<Message>();
        private object _locker = new object();

        /// <summary>
        /// Long lasting loop to get server responses 
        /// and to get notified about connected/disconnected peers and messages.
        /// </summary>
        private async Task SendWaitRequestAsync()
        {
            int messageId = 0;
            while (_state != State.NotConnected)
            {
                try
                {
                    string request = string.Format("wait?peer_id=" + _myId);

                    // Send the request, await response
                    HttpResponseMessage response =
                        await _httpClient.GetAsync(_baseHttpAddress + request,
                        HttpCompletionOption.ResponseContentRead);
                    HttpResponseHeaders header = response.Headers;
                    HttpStatusCode status_code = response.StatusCode;
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        Debug.WriteLine("Internal server error, StatusCode: 500");
                        return;
                    }

                    int peerId = ParseHeaderGetPragma(header);

                    string result;
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        if (_myId == peerId)
                        {
                            string peer_name;
                            int peer_id, peer_connected;
                            if (!ParseServerResponse(result, status_code,
                                out peer_name, out peer_id, out peer_connected))
                                continue;

                            AddOrRemovePeerFromList(peer_name, peer_id, peer_connected);
                        }
                        else
                        {
                            if (response.ToString().Contains("BYE"))
                                OnPeerHangup(new Peer(peerId, string.Empty));
                            else
                            {
                                messageId++;

                                var message = new Message();
                                message.Id = messageId.ToString();
                                message.PeerId = peerId.ToString();
                                message.Content = result;

                                Debug.WriteLine("OnMessageFromPeer! " +
                                    "message id: " + message.Id +
                                    " , peer id: " + message.PeerId +
                                    " , message content: " + message.Content);

                                lock (_locker)
                                {
                                    messagesList.Add(message);
                                }

                                OnMessageFromPeer(message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[Error] Signaling SendWaitRequestAsync, Message: " + ex.Message);
                }

                // If the client or server HTTP is in a messed up state and
                // returns bogus information or exceptions immediately,
                // prevent the CPU from becoming pinned by yielding the CPU
                // to other tasks. This will not solve the issue but this
                // will prevent CPU spiking to 100%.
                await Task.Yield();
            }
        }

        /// <summary>
        /// Wait for messages to arrive from peers. This method will not
        /// return until there is at least one message available.
        /// </summary>
        /// <returns>Returns a list messages sent from peers or
        /// returns an exception the ability to receive messages
        /// failed.</returns>
        public async Task<IList<Message>> WaitForMessagesAsync()
        {
            return await Task.Run(() => GetMessagesList());
        }

        private IList<Message> GetMessagesList()
        {
            IList<Message> list = new List<Message>();

            lock (_locker)
            {
                messagesList.ForEach(m => { list.Add(m); });
            }

            return list;
        }

        private static readonly string _localPeerRandom = new Func<string>(() =>
        {
            Random random = new Random();   // WARNING: NOT cryptographically strong!
            const string chars = "abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int length = 5;
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        })();

        static readonly Peer _localPeer = new Func<Peer>(() =>
        {
            string host = IPGlobalProperties.GetIPGlobalProperties().HostName.ToLower();
            string hostname = host != null ? host : "<unknown host>";

            // A random string is added to the peer name to easily filter
            // our local peer by name when the server re-announces the
            // local peer to itself. Thus two peers with the same hostname
            // will never be the same and running the application again
            // causes a slightly different peer name on the peer list to
            // distinguish a new peer from an old zombie peer still not
            // yet purged from the server.
            string peerName = hostname + "-" + _localPeerRandom + "-data";

            return new Peer(-1, peerName);
        })();

        public Peer LocalPeer
        {
            // return a clone of the static local peer contents to ensure original Peer values cannot be modified
            get { return new Peer(_localPeer); }
        }
    }
}
