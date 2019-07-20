using Client_UWP.Models;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace Client_UWP.Pages
{
    public class LocalSettings
    {
        public ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public void SerializeIceServersList(ObservableCollection<IceServerModel> iceServersList) =>
            localSettings.Values["IceServersList"] =
                XmlSerialization<ObservableCollection<IceServerModel>>.Serialize(iceServersList);

        public ObservableCollection<IceServerModel> DeserializeIceServersList() =>
            XmlSerialization<ObservableCollection<IceServerModel>>.Deserialize((string)localSettings.Values["IceServersList"]);

        public void SerializeAccountsList(ObservableCollection<AccountModel> accountsList) =>
            localSettings.Values["AccountsList"] =
                    XmlSerialization<ObservableCollection<AccountModel>>.Serialize(accountsList);

        public ObservableCollection<AccountModel> DeserializeAccountsList() =>
            XmlSerialization<ObservableCollection<AccountModel>>.Deserialize((string)localSettings.Values["AccountsList"]);

        public void SerializeSelectedAccount(AccountModel account) =>
            localSettings.Values["SelectedAccount"] = XmlSerialization<AccountModel>.Serialize(account);

        public AccountModel DeserializeSelectedAccount() =>
            XmlSerialization<AccountModel>.Deserialize((string)localSettings.Values["SelectedAccount"]);

        public void SerializeVideoCodecsNameList(ObservableCollection<string> videoCodecsNameList) =>
            localSettings.Values["VideoCodecsNameList"] = XmlSerialization<ObservableCollection<string>>.Serialize(videoCodecsNameList);

        public void SerializeAudioCodecsNameList(ObservableCollection<string> audioCodecsNameList) =>
            localSettings.Values["AudioCodecsNameList"] = XmlSerialization<ObservableCollection<string>>.Serialize(audioCodecsNameList);

        public ObservableCollection<string> DeserializeVideoCodecsNameList() =>
            XmlSerialization<ObservableCollection<string>>.Deserialize((string)localSettings.Values["VideoCodecsNameList"]);

        public ObservableCollection<string> DeserializeAudioCodecsNameList() =>
            XmlSerialization<ObservableCollection<string>>.Deserialize((string)localSettings.Values["AudioCodecsNameList"]);

        public object GetSelectedVideoCodecName => localSettings.Values["SelectedVideoCodecName"];
        public void SetSelectedVideoCodecName(string selectedVideoCodecName) => localSettings.Values["SelectedVideoCodecName"] = selectedVideoCodecName;

        public object GetSelectedAudioCodecName => localSettings.Values["SelectedAudioCodecName"];
        public void SetSelectedAudioCodecName(string selectedAudioCodecName) => localSettings.Values["SelectedAudioCodecName"] = selectedAudioCodecName;

        public object GetSelectedSpeakerName => localSettings.Values["SelectedSpeakerName"];
        public void SetSelectedSpeakerName(string selectedSpeakerName) => localSettings.Values["SelectedSpeakerName"] = selectedSpeakerName;

        public object GetSelectedMicrophoneName => localSettings.Values["SelectedMicrophoneName"];
        public void SetSelectedMicrophoneName(string selectedMicrophoneName) => localSettings.Values["SelectedMicrophoneName"] = selectedMicrophoneName;

        public object GetSelectedCameraName => localSettings.Values["SelectedCameraName"];
        public object SetSelectedCameraName(string selectedCameraName) => localSettings.Values["SelectedCameraName"] = selectedCameraName;

        public object GetSelectedResolutionString => localSettings.Values["SelectedResolution"];
        public object SetSelectedResolutionString(string selectedResolution) => localSettings.Values["SelectedResolution"] = selectedResolution;

        public object GetSelectedFrameRateString => localSettings.Values["SelectedFrameRate"];
        public object SetSelectedFrameRateString(string selectedFrameRate) => localSettings.Values["SelectedFrameRate"] = selectedFrameRate;
    }
}
