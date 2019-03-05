﻿using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Client_UWP.Pages.SettingsConnection
{
    public class SettingsConnectionPageViewModel : ViewModelBase
    {
        public SettingsConnectionPageViewModel()
        {
            try
            {
                if (SettingsController.Instance.localSettings.Values["IceServersList"] != null)
                {
                    if (!(XmlSerialization<ObservableCollection<IceServer>>
                .Deserialize((string)SettingsController.Instance.localSettings.Values["IceServersList"])).Any())
                    {
                        AddDefaultIceServers(IceServersList);

                        ObservableCollection<IceServer> list =
                            XmlSerialization<ObservableCollection<IceServer>>
                            .Deserialize((string)SettingsController.Instance.localSettings.Values["IceServersList"]);

                        foreach (IceServer ice in list)
                            IceServersList.Add(ice);
                    }
                    else
                    {
                        ObservableCollection<IceServer> list =
                            XmlSerialization<ObservableCollection<IceServer>>
                            .Deserialize((string)SettingsController.Instance.localSettings.Values["IceServersList"]);

                        foreach (IceServer ice in list)
                            IceServersList.Add(ice);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] SettingsConnectionPageViewModel message: " + ex.Message);
            }
        }

        public ObservableCollection<IceServer> IceServersList { get; set; } = new ObservableCollection<IceServer>();

        public static ObservableCollection<IceServer> AddDefaultIceServers(ObservableCollection<IceServer> IceServersList)
        {
            List<IceServer> list = DefaultSettings.IceServersList;

            foreach (IceServer ice in list)
                IceServersList.Add(ice);

            return IceServersList;
        }
    }
}
