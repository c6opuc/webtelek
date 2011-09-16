using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;
//using MediaPortal.GUI.TV;
//using MediaPortal.TV.Recording;
using MediaPortal.Configuration;
using Microsoft.Win32;


namespace MediaPortal.GUI.WebTelek
{

    public class WebTelek : GUIWindow
    {
        
        #region SkinControlAttributes
        [SkinControlAttribute(4)]
        protected GUIButtonControl btnSelection = null;
        [SkinControlAttribute(7)]
        protected GUIButtonControl btnFavorites = null;
        [SkinControlAttribute(17)]
        protected GUIButtonControl btnSearch = null;
        [SkinControlAttribute(9)]
        protected GUIButtonControl btnArchive = null;
        [SkinControlAttribute(10)]
        protected GUIListControl listView = null;
        [SkinControlAttribute(6)]
        protected GUIImage ChannelLogo = null;
        [SkinControlAttribute(8)]
        protected GUIButtonControl btnOthers = null;
        [SkinControlAttribute(3)]
        protected GUIButtonControl btnKinozal = null;
        [SkinControlAttribute(12)]
        protected GUITextScrollUpControl textKinozal = null;
        [SkinControlAttribute(13)]
        protected GUIImage imageKinozal = null;
        [SkinControlAttribute(14)]
        protected GUIListControl listKinozal = null;
        #endregion
        #region Variables
                
        public static string VERSION = "1.0.0.1";
        public static int PluginID  = 7926;
        public static int TVGuideID = 7927;
        public static int TVProgramID = 7928;
        public static int VirtualKeyboardID = 7929;

        public static bool isPlayNextActive = false;

        GUIDialogMenu optionchooser = null;
        GUIDialogMenu chooser = null;
        private string airzone = "";

        private string SelectedCategoryNr = string.Empty;

        private string SelectedItem = string.Empty;
        private string SelectedLanguageLink = string.Empty;
        private string ChoosenList = string.Empty;

        private StringCollection FavURLs = new StringCollection();
        
        private StringCollection FCategories = new StringCollection();
        private StringCollection FCountries = new StringCollection();
        private StringCollection FNames = new StringCollection();
        private StringCollection FUrls = new StringCollection();
        private List<string> _searchNames = new List<string>();
        private StringCollection FDescriptions = new StringCollection();
        private StringCollection PlayNextUrls = new StringCollection();
        private StringCollection PlayNextNames = new StringCollection();
        private int PlayNextIndex = 0;

        private StringCollection Categories = new StringCollection();
        private StringCollection Countries = new StringCollection();
        private StringCollection Languages = new StringCollection();
        private StringCollection LanguagesLink = new StringCollection();
        
        private StringCollection DataCountries = new StringCollection();
        private StringCollection DataCategories = new StringCollection();
        private StringCollection DataChannelName = new StringCollection();
        private StringCollection DataUrls = new StringCollection();
        public StringCollection DataDescriptions = new StringCollection();
        
        private WebTelekHTTPClient webdata = null;
        private WebTelekLiveXML xml = null;
        private WebTelekArchiveXML archive = null;
        private string archivexml = "";
        private StringCollection[] archiveMenuPath = new StringCollection[2];
        private const string ARCHIVECHANNELSBYDATE = "ArchiveChannelsByDate";
        private const string ARCHIVEDATESBYCHANNEL = "ArchiveDatesByChannel";
        private const string ARCHIVECHANNELS = "ArchiveChannels";
        private const string ARCHIVEDATES = "ArchiveDates";
        private const string ARCHIVESHOWS = "ArchiveShows";

        private const string KINOZAL = "Kinozal";

        private string LastChoosen = string.Empty;
        private int LastSelectedItem = -1;
        private int LastSelectedChannel = -1;


        private string currplay = "";
        private DateTime lastrefresh = DateTime.Today;
        private string kinozalurl = "";
        private StringCollection[] records = new StringCollection[2];
        private StringCollection[] recorditems = new StringCollection[8];
        private int kinozalItemIndex;
        //private string SelectedChannelLogo;
        private string curr_play_url = "";
        private Timer _timer = new Timer();

        private string arcDateSelector = String.Empty;
        private string arcChannelSelector = String.Empty;
        private string arcGenreSelector = String.Empty;
        private string channel = String.Empty;
        private string genre = String.Empty;


        [DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);
        [DllImport("kernel32.dll")]
        static extern ErrorModes GetErrorMode();

        List<int> Navigation = new List<int>();
        private TypeOfList _currentTypeOfList;
        private string _lastSearchTitle;
        private Boolean preload;
        private Boolean stdplayer;
        public static Double opacity;
        
        public enum TypeOfList
        {
            NONE,
            SavedSearchList,
            ResultsOfSavedSearchSelection
        }

        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }

        private enum NaviPlace : int
        {
            FAVORITES = 0,
            COUNTRIES = 1,
            CATEGORIES = 2,
            SHOWLIST = 3,
            ARCHIVELIST = 4,
            KINOZALLIST = 5,
            KINOZALRECORD = 6
        }

        #endregion

        public override int GetID
        {
            get
            {
                return PluginID;
            }
            set
            {
                base.GetID = value;
            }
        }

        public override bool Init()
        {
            SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS);
            archiveMenuPath[0] = new StringCollection();
            archiveMenuPath[1] = new StringCollection();

            g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
            g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "rumote_profile.xml"), false))
            {
                preload = Boolean.Parse(Convert.ToString(xmlreader.GetValueAsString("Account", "preload", "false")));
                stdplayer = Boolean.Parse(Convert.ToString(xmlreader.GetValueAsString("Account", "stdplayer", "false")));
                opacity = Double.Parse(Convert.ToString(xmlreader.GetValueAsString("Account", "opacity", "1")));
            }

            if (! stdplayer) g_Player.Factory = new WebTelekPlayerFactory();

            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory(dir+@"\rumote\");

            return Load(GUIGraphicsContext.Skin + @"\rumote.xml");
        }


        void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
        {
            Log.Debug("WebTelekPlugin: recieved PlayBackStopped event");
           
        }

        void OnPlayBackEnded(g_Player.MediaType type, string filename)
        {
            Log.Debug("WebTelekPlugin: recieved PlayBackEnded event");
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();

            GUIPropertyManager.SetProperty("#favorites", "Любимые");
            GUIPropertyManager.SetProperty("#select", "Выбор каналов");
            GUIPropertyManager.SetProperty("#archive", "Архив передач");
            GUIPropertyManager.SetProperty("#kinozal", "Кинозал");
            GUIPropertyManager.SetProperty("#others", "Дополнительно");
            btnFavorites.Label = "Любимые";
                 

            OSDVolume.Stop();
            OSDInfo.Stop();
            OSDProgressBar.Stop();
            //OSDNotify.Stop();
            
            OSDVolume.Start();
            OSDInfo.Start();
            OSDProgressBar.Start();
            //OSDNotify.Start();

            if (Navigation.Count > 0)
            {
                Log.Debug("OnPageLoad: " + Navigation[0].ToString());
            }
            else
            {
                Log.Debug("OnPageLoad: empty");
            }

            
            GUIPropertyManager.SetProperty("#Header",       " ");
            GUIPropertyManager.SetProperty("#ChannelInfo",  " ");


            listView.Focus = true;
            btnArchive.Focus = false;
            btnSelection.Focus = false;
            btnFavorites.Focus = false;
            btnOthers.Focus = false;

            textKinozal.IsVisible = false;
            listKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;

            LoadFavoritesAndSearches();
            if (currplay != "") GUIPropertyManager.SetProperty("#Play.Current.Title", currplay);
            GetChannelData(false);

            if (airzone == "" ) airzone = webdata.region;

            if ((_currentTypeOfList == TypeOfList.SavedSearchList || _currentTypeOfList == TypeOfList.ResultsOfSavedSearchSelection)  &&  !string.IsNullOrEmpty (_lastSearchTitle) )
            {
                ShowResultOfSearch(_lastSearchTitle); return;
            }
            if (LastChoosen != "")
            {
                if (LastChoosen == "ShowRecord")
                {
                    ShowRecord();
                    return;
                }
                
                if (LastChoosen == "Countries")
                {
                    SelectedItem = Countries[LastSelectedItem];
                    ChoosenList = LastChoosen;
                    GUIPropertyManager.SetProperty("#Header", SelectedItem);
                    ShowChannels(ChoosenList, SelectedItem, LastSelectedChannel);
                }
                if (LastChoosen == "Categories")
                {
                    SelectedItem = Categories[LastSelectedItem];
                    ChoosenList = LastChoosen;
                    GUIPropertyManager.SetProperty("#Header", SelectedItem);
                    ShowChannels(ChoosenList, SelectedItem, LastSelectedChannel);
                }
                if (LastChoosen == "Favorites")
                {
                    ChoosenList = LastChoosen;
                    GUIPropertyManager.SetProperty("#Header", btnFavorites.Label);
                    ShowChannels("Favorites", "", LastSelectedChannel);
                }
                
                if (LastChoosen == ARCHIVESHOWS)
                {
                    ShowArchiveShow(channel, arcDateSelector, genre);
                    ChoosenList = LastChoosen;
                    return;
                }
            }
            else
            {
                ChoosenList = LastChoosen;
                GUIPropertyManager.SetProperty("#Header", btnFavorites.Label);
                ShowChannels("Favorites", "", -1);
            }
        }

        private void GetInfoMessage()
        {
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "rumote_profile.xml"), false))
            {
                if (Convert.ToString(xmlreader.GetValueAsString("Account", "versioncheck", "true")) == "true")
                {
                    string infomessage = webdata.getHTTPData("http://www.rumote.com/export/maintenance-updates.php?ver=" + VERSION);
                    if (infomessage != "OK")
                    {
                        GUIDialogNotify info = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
                        info.Reset();
                        info.IsOverlayAllowed = true;
                        info.SetHeading("Новости");
                        info.SetText(infomessage);
                        info.DoModal(GetID);

                    }
                }
            }
        }

        private void LoadFavoritesAndSearches()
        {
            FavURLs.Clear();
            //string dir = Directory.GetCurrentDirectory();
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "rumote_favorites.xml"), false))
            {
                for (int i = 0; i <= 1500; i++)
                {
                    string temp = Convert.ToString(xmlreader.GetValueAsString("Favorites", i.ToString(), "")).Trim();
                    if (temp != "") FavURLs.Add(temp);
                }
            }
            _searchNames.Clear();
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "rumote_searches.xml"), false))
            {
                for (int i = 0; i <= 1500; i++)
                {
                    string temp = Convert.ToString(xmlreader.GetValueAsString("Searches", i.ToString(), "")).Trim();
                    if (temp != "") _searchNames.Add (temp);
                }
            }

        }

        private void SaveFavorites()
        {
            //string dir = Directory.GetCurrentDirectory();
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            File.Delete(Config.GetFile(Config.Dir.Config, "rumote_favorites.xml"));
            XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "rumote_favorites.xml"), null);
            writer.WriteStartDocument();
            writer.Formatting = Formatting.Indented;
            writer.WriteStartElement("profile");
            writer.WriteStartElement("section");
            writer.WriteAttributeString("name", "Favorites");
            for (int i = 0; i < FavURLs.Count; i++)
            {
                writer.WriteStartElement("entry");
                writer.WriteAttributeString("name", i.ToString());
                writer.WriteString(FavURLs[i].Trim());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();

        }

        private void SaveSearches()
        {
            //string dir = Directory.GetCurrentDirectory();
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            File.Delete(Config.GetFile(Config.Dir.Config, "rumote_searches.xml"));
            XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "rumote_searches.xml"), null);
            writer.WriteStartDocument();
            writer.Formatting = Formatting.Indented;
            writer.WriteStartElement("profile");
            writer.WriteStartElement("section");
            writer.WriteAttributeString("name", "Searches");
            for (int i = 0; i < _searchNames.Count; i++)
            {
                writer.WriteStartElement("entry");
                writer.WriteAttributeString("name", i.ToString());
                writer.WriteString(_searchNames[i].Trim());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();

        }

        public void GetChannelData(Boolean refresh)
        {
            Countries.Clear();
            Categories.Clear();
            DataCountries.Clear();
            DataCategories.Clear();
            DataChannelName.Clear();
            DataUrls.Clear();
            DataDescriptions.Clear();

            if (lastrefresh.DayOfYear != DateTime.Today.DayOfYear)
            {
                refresh = true;
                lastrefresh = DateTime.Today;
            }
            if (refresh)
            {
                webdata = new WebTelekHTTPClient();
                if (airzone != "") webdata.region = airzone;
                xml = new WebTelekLiveXML(new MemoryStream(UTF8Encoding.Default.GetBytes(webdata.getData())));
                archive = new WebTelekArchiveXML(
                    webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=days&version=2.0"),
                    webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=channels&version=2.0"),
                    webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=genres&version=2.0")
                );
                // webdata.getEPG(true);
            }
            else
            {
                if (webdata == null)
                {
                    webdata = new WebTelekHTTPClient();
                    xml = new WebTelekLiveXML(new MemoryStream(UTF8Encoding.Default.GetBytes(webdata.getData())));
                    archive = new WebTelekArchiveXML(
                        webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=days&version=2.0"),
                        webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=channels&version=2.0"),
                        webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=genres&version=2.0")
                    );
                    //webdata.getEPG();
                    GetInfoMessage();
                }
                if (xml == null) xml = new WebTelekLiveXML(new MemoryStream(UTF8Encoding.Default.GetBytes(webdata.getData())));
                if (archive == null) archive = new WebTelekArchiveXML(
                                        webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=days&version=2.0"),
                                        webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=channels&version=2.0"),
                                        webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=genres&version=2.0")
                                     );

            }

            DataCountries = xml.getCountries();
            DataCategories = xml.getCategories();
            DataChannelName = xml.getChannelsNames();
            DataUrls = xml.getURLs();
            DataDescriptions = xml.getSchedules();
            foreach (string custom in xml.getCustom("country"))
            {
                DataCountries.Add(custom);
            }
            foreach (string custom in xml.getCustom("category"))
            {
                DataCategories.Add(custom);
            }
            foreach (string custom in xml.getCustom("name"))
            {
                DataChannelName.Add(custom);
            }
            foreach (string custom in xml.getCustom("url"))
            {
                DataUrls.Add(custom);
            }
            foreach (string custom in xml.getCustom("description"))
            {
                DataDescriptions.Add(custom);
            } 
           
            Countries.Add("Все");
            foreach (string country in DataCountries)
            {
                int i = 0;
                i = Countries.IndexOf(country);
                if (i == -1)
                {
                    Countries.Add(country);
                }
            }

            Categories.Add("Все");
            foreach (string category in DataCategories)
            {
                int i = 0;
                i = Categories.IndexOf(category);
                if (i == -1)
                {
                    Categories.Add(category);
                }
            }
        }

        private void chooseTZ()
        {
            GUIDialogMenu airzonechooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            airzonechooser.Reset();
            airzonechooser.SetHeading("Выбрать зону вещания");

            if (airzone == "msk")
            {
                airzonechooser.Add("Выбрана зона вещания MSK *");
            }
            else
            {
                airzonechooser.Add("Выбрать зону вещания MSK");
            }
            if (airzone == "est")
            {
                airzonechooser.Add("Выбрана зона вещания EST *");
            }
            else
            {
                airzonechooser.Add("Выбрать зону вещания EST");
            }
            if (airzone == "pst")
            {
                airzonechooser.Add("Выбрана зона вещания PST *");
            }
            else
            {
                airzonechooser.Add("Выбрать зону вещания PST");
            }
            if (airzone == "aest")
            {
                airzonechooser.Add("Выбрана зона вещания AEST *");
            }
            else
            {
                airzonechooser.Add("Выбрать зону вещания AEST");
            }

            airzonechooser.DoModal(GetID);

            MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"), false);
            switch (airzonechooser.SelectedId)
            {
                case 1:
                    File.Delete(Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", "")) + @"\epglastdate.dat");
                    airzone = "msk";
                    GetChannelData(true);
                    break;
                case 2:
                    File.Delete(Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", "")) + @"\epglastdate.dat");
                    airzone = "est";
                    GetChannelData(true);
                    break;
                case 3:
                    File.Delete(Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", "")) + @"\epglastdate.dat");
                    airzone = "pst";
                    GetChannelData(true);
                    break;
                case 4:
                    File.Delete(Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", "")) + @"\epglastdate.dat");
                    airzone = "aest";
                    GetChannelData(true);
                    break;
                default:
                    break;
            }
        }

        private void chooseNET()
        {
            GUIDialogMenu netchooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            netchooser.Reset();
            netchooser.SetHeading("Настройки сети");

            RegistryKey key;

            key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\MediaPlayer\\Preferences", true);

            if (key.GetValue("UseUDP") == null) key.SetValue("UseUDP", 1);
            if (key.GetValue("UseTCP") == null) key.SetValue("UseTCP", 1);
            if (key.GetValue("UseHTTP") == null) key.SetValue("UseHTTP", 1);

            if ((int)key.GetValue("UseUDP") == 1)
            {
                netchooser.Add("UDP : ВКЛ");
            }
            else
            {
                netchooser.Add("UDP : ВЫКЛ");
            }
            if ((int)key.GetValue("UseTCP") == 1 || key.GetValue("UseTCP") == null)
            {
                netchooser.Add("TCP : ВКЛ");
            }
            else
            {
                netchooser.Add("TCP : ВЫКЛ");
            }
            if ((int)key.GetValue("UseHTTP") == 1 || key.GetValue("UseHTTP") == null)
            {
                netchooser.Add("HTTP : ВКЛ");
            }
            else
            {
                netchooser.Add("HTTP : ВЫКЛ");
            }

            netchooser.Add("Сохранить");

            netchooser.DoModal(GetID);

            MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"), false);
            switch (netchooser.SelectedId)
            {
                case 1:
                    if ((int)key.GetValue("UseUDP") == 1) 
                    {
                        key.SetValue("UseUDP", 0);
                    }
                    else 
                    {
                        key.SetValue("UseUDP", 1);
                    }
                    chooseNET();
                    break;
                case 2:
                    if ((int)key.GetValue("UseTCP") == 1)
                    {
                        key.SetValue("UseTCP", 0);
                    }
                    else
                    {
                        key.SetValue("UseTCP", 1);
                    }
                    chooseNET();
                    break;
                case 3:
                    if ((int)key.GetValue("UseHTTP") == 1)
                    {
                        key.SetValue("UseHTTP", 0);
                    }
                    else
                    {
                        key.SetValue("UseHTTP", 1);
                    }
                    chooseNET();
                    break;
                case 4:
                    g_Player.Stop();
                    g_Player.Release();
                    GC.Collect(); 
                    g_Player.Init();
                    key.Close();
                    break;
                default:
                    break;
            }


        }

        private void chooseOthers()
        {
            optionchooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            optionchooser.Reset();
            optionchooser.SetHeading("Дополнительно");
            optionchooser.Add("Обновить соединение");
            optionchooser.Add("Изменить зону вещания");
            optionchooser.Add("Настройки сети");
            //optionchooser.Add("Программа передач (EPG)");

            optionchooser.DoModal(GetID);
            
            switch (optionchooser.SelectedId)
            {
                case 1:
                    GetChannelData(true);
                    break;
                case 2:
                    chooseTZ();
                    break;
                case 3:
                    chooseNET();
                    break;
                case 4:
                    //MediaPortal.GUI.TV.TVGuideBase.wp = this;
                    //GUIWindowManager.ActivateWindow(WebTelek.TVGuideID);
                    break;
                default:
                    break;
            }
        }

        private void ShowChannels(string choise, string SelectedChoise, int SelectedChoiseIndex)
        {

            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;

            LastChoosen = ChoosenList;
            ChoosenList = "Channels";
            listView.Clear();

            FCountries.Clear();
            FCategories.Clear();
            FNames.Clear();
            FUrls.Clear();
            FDescriptions.Clear();

            GUIListItem item1 = new GUIListItem();
            item1.Label = "..";
            item1.IsFolder = true;
            MediaPortal.Util.Utils.SetDefaultIcons(item1);
            listView.Add(item1);

            int i = 0;
            int j = 0;
            String[] temp;

            if (choise == "Favorites")
            {
                Navigation.Insert(0, (int)NaviPlace.FAVORITES);
                listView.RemoveItem(0);
                GUIPropertyManager.SetProperty("#Header", btnFavorites.Label);
                foreach (string favorite in FavURLs)
                {
                    for (int z=0;z < DataUrls.Count; z++)
                    {
                        if (favorite == DataUrls[z])
                        {
                            FCountries.Add(DataCountries[z]);
                            FCategories.Add(DataCategories[z]);
                            FNames.Add(DataChannelName[z]);
                            FUrls.Add(DataUrls[z]);
                            FDescriptions.Add(DataDescriptions[z]);

                            GUIListItem item = new GUIListItem();

                            temp = DataDescriptions[z].Split('\n');
                            item.Label = DataChannelName[z] + ": " + temp[0];

                            item.IconImage = "defaultVideo.png";
                            listView.Add(item);
                        }
                    }
                }
                ChoosenList = "Favorites";
                LastChoosen = "Favorites";
            }
            if (choise == "Categories")
            {
                Navigation.Insert(0, (int)NaviPlace.SHOWLIST);
                while (j < DataCategories.Count)
                {
                    if (SelectedChoise == DataCategories[j] || SelectedChoise == "Все")
                    {
                        FCountries.Add(DataCountries[j]);
                        FCategories.Add(DataCategories[j]);
                        FNames.Add(DataChannelName[j]);
                        FUrls.Add(DataUrls[j]);
                        FDescriptions.Add(DataDescriptions[j]);

                        GUIListItem item = new GUIListItem();

                        temp = FDescriptions[i].Split('\n');
                        item.Label = FNames[i] + ": " + temp[0];

                        item.IconImage = "defaultVideo.png";
                        listView.Add(item);
                        i++;
                    }
                    j++;
                }
            }

            if (choise == "Countries")
            {
                Navigation.Insert(0, (int)NaviPlace.SHOWLIST);
                while (j < DataCountries.Count)
                {
                    if (SelectedChoise == DataCountries[j] || SelectedChoise == "Все")
                    {
                        FCountries.Add(DataCountries[j]);
                        FCategories.Add(DataCategories[j]);
                        FNames.Add(DataChannelName[j]);
                        FUrls.Add(DataUrls[j]);
                        FDescriptions.Add(DataDescriptions[j]);

                        GUIListItem item = new GUIListItem();
                        
                        temp = FDescriptions[i].Split('\n');
                        item.Label = FNames[i] + ": " + temp[0];
                        item.IconImage = "defaultVideo.png";
                        listView.Add(item);

                        i++;
                    }
                    j++;
                }
            }

            if (SelectedChoiseIndex > -1)
            {
                if (choise == "Favorites")
                {
                    listView.SelectedListItemIndex = SelectedChoiseIndex;
                }
                else
                {
                    listView.SelectedListItemIndex = SelectedChoiseIndex + 1;
                }
            }
        }


        private void ShowArchiveShow(string channel, string date, string genre)
        {
            ShowArchiveShow(channel, date, genre, "");
        }

        private void ShowArchiveShow(string query)
        {
            ShowArchiveShow("", "", "", query);
        }

        private void ShowArchiveShow(string channel, string date, string genre, string query)
        {
            Navigation.Insert(0, (int)NaviPlace.ARCHIVELIST);
            GUIPropertyManager.SetProperty("#Header", "Список выбранных программ");
            LastChoosen = ChoosenList;
            ChoosenList = ARCHIVESHOWS;

            query = System.Web.HttpUtility.UrlEncode(query, Encoding.GetEncoding("windows-1251"));
            //Log.Info("WebTelek Search URL: http://www.rumote.com/export/archive.php?action=listings&version=2.0&ch=" + channel + "&day=" + date + "&lg=" + genre + "&q=" + query);
            archivexml = webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=listings&version=2.0&ch=" + channel + "&day=" + date + "&lg=" + genre + "&q=" + query);

            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;

            listView.Clear();
            int j = 0;
            while (j < archive.getShows(archivexml)[0].Count)
            {
                GUIListItem item = new GUIListItem();
                item.Label = archive.getShows(archivexml)[2][j];
                item.IsFolder = false;
                getChannelLogo(archive.getShows(archivexml)[1][j]);
                item.IconImage = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\rumote\\" + archive.getShows(archivexml)[1][j] + ".jpg";
                listView.Add(item);
                j++;
            }
        }
        private void ShowCategories()
        {
            Navigation.Insert(0, (int)NaviPlace.CATEGORIES);
            GUIPropertyManager.SetProperty("#Header", "Выбор по категориям");
            ChoosenList = "Categories";

            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;

            listView.Clear();
            int j = 0;
            while (j < Categories.Count)
            {
                GUIListItem item = new GUIListItem();
                item.Label = Categories[j];
                item.IsFolder = true;
                MediaPortal.Util.Utils.SetDefaultIcons(item);
                listView.Add(item);
                j++;
            }
        }
        

        private void ShowCountries()
        {
            Navigation.Insert(0, (int)NaviPlace.COUNTRIES);
            GUIPropertyManager.SetProperty("#Header", "Выбор по странам");
            ChoosenList = "Countries";

            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;
            
            listView.Clear();
            int j = 0;
            while (j < Countries.Count)
            {
                GUIListItem item = new GUIListItem();
                item.Label = Countries[j];
                item.IsFolder = true;
                MediaPortal.Util.Utils.SetDefaultIcons(item);
                listView.Add(item);
                j++;
            }
        }

        private void ShowKinozal()
        {
            _currentTypeOfList = TypeOfList.NONE;
            chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            chooser.Reset();
            chooser.SetHeading("Кинозал");
            chooser.Add("Выбор по категориям");
            chooser.Add("Лучшие 100");
            chooser.Add("Новинки");
            chooser.Add("Поступления");
            chooser.Add("Скоро в прокате");
            chooser.DoModal(GetID);

            switch (chooser.SelectedId)
            {
                case 1:
                    ShowVODCategories();
                    break;
                case 2:
                    ShowRecords("http://www.rumote.com/export/kinozal.php?action=top100");
                    break;
                case 3:
                    ShowRecords("http://www.rumote.com/export/kinozal.php?action=newrecords");
                    break;
                case 4:
                    ShowRecords("http://www.rumote.com/export/kinozal.php?action=updates");
                    break;
                case 5:
                    ShowRecords("http://www.rumote.com/export/kinozal.php?action=comingsoon");
                    break;
                default:
                    break;
            }
        }

        private void ShowVODCategories()
        {
            StringCollection[] categories = new StringCollection[2];
            categories = (new WebTelekKinozalXML(webdata)).getCategories();

            chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            chooser.Reset();
            chooser.SetHeading("Выбор категории");

            foreach (string category in categories[1])
            {
                chooser.Add(category);
            }            
            chooser.DoModal(GetID);

            if (chooser.SelectedId >= 1)
            {
                ShowGenres(categories[0][chooser.SelectedId - 1], categories[1][chooser.SelectedId - 1]);
            }
            else
            {
                ShowKinozal();
            }
        }

        private void ShowGenres(string cid, string cname)
        {
            StringCollection[] genres = new StringCollection[2];
            genres = (new WebTelekKinozalXML(webdata)).getGenres(cid);

            chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            chooser.Reset();
            chooser.SetHeading(cname);

            foreach (string genre in genres[1])
            {
                chooser.Add(genre);
            }
            chooser.DoModal(GetID);

            if (chooser.SelectedId >= 1)
            {
                ShowRecords("http://www.rumote.com/export/kinozal.php?action=records&gid=" + genres[0][chooser.SelectedId - 1]);
            }
            else
            {
                ShowVODCategories();
            }
        }

        private void ShowRecords()
        {
            ShowRecords(kinozalurl);
        }

        private void ShowRecords(string url)
        {
            Navigation.Insert(0, (int)NaviPlace.KINOZALLIST);
            ChoosenList = KINOZAL;
            kinozalurl = url;
            records = (new WebTelekKinozalXML(webdata)).getRecords(url);

            GUIPropertyManager.SetProperty("#Header", "Кинозал");

            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;
            
            listView.Clear();
            foreach (string record in records[1])
            {
                GUIListItem item = new GUIListItem();
                item.Label = record;
                item.IsFolder = false;
                item.IconImage = "defaultVideo.png";
                listView.Add(item);
            }
            btnKinozal.Focus = false;
            listView.Focus = true;

        }

        private void ShowRecord()
        {
            ShowRecord(kinozalItemIndex);
        }

        private void ShowRecord(int index)
        {
            Navigation.Insert(0, (int)NaviPlace.KINOZALRECORD);
            recorditems = (new WebTelekKinozalXML(webdata)).getRecord("http://www.rumote.com/export/kinozal.php?action=items&rid=" + records[0][index]);
            kinozalItemIndex = index;

            ChoosenList = "ShowRecord";

            listView.IsVisible = false;
            textKinozal.IsVisible = true;
            imageKinozal.IsVisible = true;
            listKinozal.IsVisible = true;
            
            listKinozal.Clear();
            listView.Focus = false;
            btnOthers.Focus = false;
            btnArchive.Focus = false;
            btnSelection.Focus = false;
            btnKinozal.Focus = false;
            listKinozal.Focus = true;

            //imageKinozal.SetFileName("http://tvekran.ca/images/" + recorditems[4][0]);
            //Log.Info("KINOZAL: " + recorditems[8][0]);
            
            //imageKinozal.SetFileName("http://iptv-distribution.net/includes/image_vod_cached.ashx?vid="+recorditems[8][0]+"&picID=&width=100&height=148&aspect=true");
            imageKinozal.SetFileName("http://iptv-distribution.net/includes/image_vod.ashx?width=100&height=148&aspect=true&vid=" + recorditems[8][0]);

            textKinozal.Label = "Режисер: "  +   recorditems[5][0] + "\n" +
                                "Актеры: "    +   recorditems[6][0] + "\n" +
                                "Время: "     +   recorditems[7][0] + "\n" +
                                "Сюжет: "    +   recorditems[3][0];

            if (recorditems[1].Count > 1)
            {
                for (int i = 0; i < recorditems[1].Count; i++)
                {
                    GUIListItem item = new GUIListItem();
                    item.Label = recorditems[1][i] + " : " + recorditems[2][i];
                    item.IsFolder = false;
                    item.IconImage = "defaultVideo.png";
                    listKinozal.Add(item);
                }
            }
            else
            {
                GUIListItem item = new GUIListItem();
                item.Label = recorditems[2][0];
                item.IsFolder = false;
                item.IconImage = "defaultVideo.png";
                listKinozal.Add(item);
            }
            

        }

        public override void OnAction(Action action)
        {

            if ( (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_PARENT_DIR ) && LastChoosen != "")
            {

                Navigation.RemoveAt(0);
                Log.Debug("OnAction: " + Navigation[0]);

                if ( ChoosenList == "ShowRecord" ) {

                    ShowRecords();
                    return;
                }

                if (ChoosenList == "Favorites")
                {
                    ShowChannels("Favorites", "", LastSelectedChannel);
                }

                if (ChoosenList == ARCHIVESHOWS)
                {
                    ShowArchiveShow(channel, arcDateSelector, genre);
                }

                if (LastChoosen == "Countries")
                {
                    ShowCountries();
                }
                if (LastChoosen == "Categories")
                {
                    ShowCategories();
                }
                if (LastChoosen == ARCHIVESHOWS)
                {
                    ShowArchiveShow(channel, arcDateSelector, genre);
                }
                
                listView.SelectedListItemIndex = LastSelectedItem;
                
                ChoosenList = LastChoosen;
                LastSelectedItem = -1;
                LastChoosen = "";
                return;
            }

            if ( action.wID == Action.ActionType.ACTION_PARENT_DIR )
            {                
                return;
            }
            base.OnAction(action);
        }


        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        
        
        {

            int i = 0;

            if (control == listKinozal)
            {
                LastChoosen = "ShowRecord";
                string url = "http://www.rumote.com/play.php?action=vod&movieid="
                + recorditems[0][listKinozal.SelectedListItemIndex] + "&moviepart=" 
                + recorditems[1][listKinozal.SelectedListItemIndex];


                currplay = recorditems[2][listKinozal.SelectedListItemIndex];
                GUIPropertyManager.SetProperty("#Play.Current.Title", currplay);
                //string dir = Directory.GetCurrentDirectory();
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                File.Delete(dir + @"\rumote\rumote.asx");
                String _tempasx = webdata.getHTTPData(url);
                if ( preload ) if (_tempasx.IndexOf("connect.wmv") > 0) _tempasx =  _tempasx.Insert(_tempasx.IndexOf("connect.wmv") + 8, "1");
                File.WriteAllText(dir + @"\rumote\rumote.asx", _tempasx, Encoding.Default);
                url = dir + @"\rumote\rumote.asx";
                g_Player.Play(url);
                OSDInfo.wp = null;
                OSDInfo.channel_id = null;
                OSDInfo.sched_string =  "Название: " +   recorditems[2][listKinozal.SelectedListItemIndex] + "\n" +
                                        "Режисер: "  +   recorditems[5][listKinozal.SelectedListItemIndex] + "\n" +
                                        "Актеры: "   +   recorditems[6][listKinozal.SelectedListItemIndex] + "\n" ;
                GUIGraphicsContext.IsFullScreenVideo = true;
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                g_Player.FullScreen = true;
            }
            
            if (control == listView)
            {
                if (ChoosenList == KINOZAL)
                {
                    ShowRecord(listView.SelectedListItemIndex);
                }
                if ( ChoosenList == ARCHIVESHOWS || ChoosenList == ARCHIVECHANNELS || ChoosenList == ARCHIVEDATES || ChoosenList == ARCHIVECHANNELSBYDATE || ChoosenList == ARCHIVEDATESBYCHANNEL || _currentTypeOfList== TypeOfList.ResultsOfSavedSearchSelection )
                {
                    if (archiveMenuPath[1].IndexOf(ChoosenList) == -1)
                    {
                        archiveMenuPath[1].Add(ChoosenList);
                        archiveMenuPath[0].Add(listView.SelectedListItemIndex.ToString());
                    }

                    if ( ChoosenList == ARCHIVESHOWS || _currentTypeOfList == TypeOfList.ResultsOfSavedSearchSelection )
                    {
                        string url = "http://www.rumote.com/play.php?action=pvr&programid=" + archive.getShows(archivexml)[0][listView.SelectedListItemIndex];
                        currplay = archive.getShows(archivexml)[2][listView.SelectedListItemIndex];
                        GUIPropertyManager.SetProperty("#Play.Current.Title", currplay);
                        //string dir = Directory.GetCurrentDirectory();
                        string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        File.Delete(dir + @"\rumote\rumote.asx");
                        Log.Info("!!!!!:"+url);
                        String _tempasx = webdata.getHTTPData(url);
                        if (preload) if (_tempasx.IndexOf("connect.wmv") > 0) _tempasx = _tempasx.Insert(_tempasx.IndexOf("connect.wmv") + 8, "1");
                        File.WriteAllText(dir + @"\rumote\rumote.asx", _tempasx, Encoding.Default);
                        url = dir + @"\rumote\rumote.asx";
                        g_Player.Play(url);
                        OSDInfo.wp = null;
                        OSDInfo.channel_id = archive.getShows(archivexml)[1][listView.SelectedListItemIndex];
                        OSDInfo.sched_string = currplay;
                        GUIGraphicsContext.IsFullScreenVideo = true;
                        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                        g_Player.FullScreen = true;
                    }
                }
                if (_currentTypeOfList == TypeOfList.SavedSearchList || _currentTypeOfList == TypeOfList.ResultsOfSavedSearchSelection )
                {
                    //_previousTypeOfList = TypeOfList.SavedSearchList;
                    ShowResultOfSearch(_searchNames[listView.SelectedListItemIndex]);
                }
                else
                {
                    _currentTypeOfList = TypeOfList.NONE;
                    //_previousTypeOfList = TypeOfList.NONE;
                }

                if (ChoosenList == "Channels" || ChoosenList == "Favorites")
                {
                    if (ChoosenList == "Channels") i = 1; 
                    if (listView.SelectedListItemIndex == 0 && ChoosenList == "Channels")
                    {
                        if (LastChoosen == "Countries")
                        {
                            
                            ShowCountries();
                        }
                        if (LastChoosen == "Categories")
                        {
                            ShowCategories();
                        }
                        listView.SelectedListItemIndex = LastSelectedItem;
                        ChoosenList = LastChoosen;
                        LastSelectedItem = -1;
                        return;
                    }

                    LastSelectedChannel = listView.SelectedListItemIndex - i;
                    string mmsurl = string.Empty;

                    //string dir = Directory.GetCurrentDirectory();
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    File.Delete(dir + @"\rumote\rumote.asx");

                    if (curr_play_url != FUrls[listView.SelectedListItemIndex - i] || g_Player.Playing == false)
                    {
                        if (FUrls[listView.SelectedListItemIndex - i].Contains("http://www.rumote.com/play.php?ch="))
                        {
                            String _tempasx = webdata.getHTTPData(FUrls[listView.SelectedListItemIndex - i]);
                            if (preload) if (_tempasx.IndexOf("connect.wmv") > 0) _tempasx = _tempasx = _tempasx.Insert(_tempasx.IndexOf("connect.wmv") + 8, "1");
                            File.WriteAllText(dir + @"\rumote\rumote.asx", _tempasx, Encoding.Default);
                            mmsurl = dir + @"\rumote\rumote.asx";
                            //Log.Info("!!id="+FUrls[listView.SelectedListItemIndex - i].Substring(34));
                            OSDInfo.channel_id = FUrls[listView.SelectedListItemIndex - i].Substring(34);
                        }
                        else
                        {
                            mmsurl = FUrls[listView.SelectedListItemIndex - i];
                            OSDInfo.channel_id = null;
                        }

                        currplay = FNames[listView.SelectedListItemIndex - i];
                    }
                    
                    listView.Focus = false;
                    btnArchive.Focus = false;
                    btnSelection.Focus = false;

                    PlayNextUrls.Clear();
                    PlayNextNames.Clear();
                    string[] temp = new string[FNames.Count];
                    FUrls.CopyTo(temp, 0);
                    PlayNextUrls.AddRange(temp);
                    FNames.CopyTo(temp, 0);
                    PlayNextNames.AddRange(temp);
                    
                    if (curr_play_url != FUrls[listView.SelectedListItemIndex - i] || g_Player.Playing == false)
                    {
                        PlayNextIndex = listView.SelectedListItemIndex - i;
                        //Log.Debug("!!! mmsurl is: " + mmsurl); 
                        g_Player.Play(mmsurl);
                        curr_play_url = FUrls[listView.SelectedListItemIndex - i];
                        OSDInfo.wp = this;
                        OSDInfo.index = DataUrls.IndexOf(FUrls[listView.SelectedListItemIndex - i]);
                    }

                    GUIGraphicsContext.IsFullScreenVideo = true;
                    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                    g_Player.FullScreen = true;
                }

                if (ChoosenList == "Countries")
                {
                    SelectedItem = Countries[listView.SelectedListItemIndex];
                    LastSelectedItem = listView.SelectedListItemIndex;
                    GetChannelData(false);
                    ShowChannels(ChoosenList, SelectedItem, -1);
                    GUIPropertyManager.SetProperty("#Header", SelectedItem);
                }

                if (ChoosenList == "Categories")
                {
                    SelectedItem = Categories[listView.SelectedListItemIndex];
                    LastSelectedItem = listView.SelectedListItemIndex;
                    GetChannelData(false);
                    ShowChannels(ChoosenList, SelectedItem, -1);
                    GUIPropertyManager.SetProperty("#Header", SelectedItem);
                }
            }
            if (control == btnSelection)
            {
                chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                chooser.Reset();
                chooser.SetHeading("Выбор каналов");
                chooser.Add("По странам");
                chooser.Add("По категориям");
                //chooser.Add("Выбор из EPG");
                chooser.DoModal(GetID);
                _currentTypeOfList = TypeOfList.NONE;
                switch (chooser.SelectedId)
                {
                    case 1:
                        archiveMenuPath[0].Clear();
                        archiveMenuPath[1].Clear();
                        ShowCountries();
                        listView.Focus = true;
                        btnSelection.Focus = false;
                        break;
                    case 2:
                        archiveMenuPath[0].Clear();
                        archiveMenuPath[1].Clear();
                        ShowCategories();
                        listView.Focus = true;
                        btnSelection.Focus = false;
                        break;
                    case 3:
                        //MediaPortal.GUI.TV.TVGuideBase.wp = this;
                        //GUIWindowManager.ActivateWindow(WebTelek.TVGuideID);
                        break;
                    default:
                        break;
                }
            }
            if (control == btnArchive)
            {
                
                ArchiveSelector();
                if (listView.Visible )
                {
                    control.Focus = false;
                    listView.Focus = true;
                    
                }
            }

            if (control == btnFavorites)
            {
                archiveMenuPath[0].Clear();
                archiveMenuPath[1].Clear();
                GetChannelData(false);
                ShowChannels("Favorites", "", -1);
                _currentTypeOfList = TypeOfList.NONE;
                btnFavorites.Focus = false;
                listView.Focus = true;
            }
            if (control == btnKinozal)
            {
                ShowKinozal();
            }
            if (control == btnOthers)
            {
                chooseOthers();
                GetChannelData(false);
                ShowChannels("Favorites", "", -1);
            }
            base.OnClicked(controlId, control, actionType);
        }

        //private void SearchSelector()
        //{
        //    chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        //    chooser.Reset();
        //    chooser.SetHeading("Сохранённые поиски");

        //    chooser.Add("Выбрать");
        //    foreach (string item in _searchNames)
        //    {
        //        chooser.Add(item);   
        //    }
        //    chooser.DoModal(GetID);

        //    switch (chooser.SelectedId)
        //    {
        //        case 1:
        //            if (arcChannelSelector != String.Empty) channel = archive.getChannels()[0][archive.getChannels()[1].IndexOf(arcChannelSelector)];
        //            if (arcGenreSelector != String.Empty) genre = archive.getGenres()[0][archive.getGenres()[1].IndexOf(arcGenreSelector)];
        //            ShowArchiveShow(channel, arcDateSelector, genre);
        //            break;
        //        case 2:
        //            ArchiveDateSelector();
        //            break;
        //        case 3:
        //            ArchiveChannelSelector();
        //            break;
        //        case 4:
        //            ArchiveGenreSelector();
        //            break;
        //        case 5:
        //            arcDateSelector = String.Empty;
        //            arcChannelSelector = String.Empty;
        //            arcGenreSelector = String.Empty;
        //            ArchiveSelector();
        //            break;
        //        case 6:
        //            // code for displaying saved search patterns...
        //            break;
        //        default:
        //            break;
        //    }

        //}



        void ArchiveSelector()
        {
            chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            chooser.Reset();
            chooser.SetHeading("Выбор из архива");
            _currentTypeOfList = TypeOfList.NONE;
            chooser.Add("Выбрать");

            if (arcDateSelector == String.Empty) chooser.Add("Дата: любая");
            else chooser.Add("Дата: " + arcDateSelector);

            if (arcChannelSelector == String.Empty) chooser.Add("Канал: любой");
            else chooser.Add("Канал: " + arcChannelSelector);

            if (arcGenreSelector == String.Empty) chooser.Add("Жанр: любой");
            else chooser.Add("Жанр: " + arcGenreSelector);

            chooser.Add("Сбросить");

            chooser.Add("Сохраненный поиск");

            chooser.Add("Свободный поиск");

            chooser.DoModal(GetID);

            switch (chooser.SelectedId)
            {
                case 1:
                    if (arcChannelSelector != String.Empty)
                        channel = archive.getChannels()[0][archive.getChannels()[1].IndexOf(arcChannelSelector)];
                    else
                        channel = String.Empty;

                    if (arcGenreSelector != String.Empty)
                        genre = archive.getGenres()[0][archive.getGenres()[1].IndexOf(arcGenreSelector)];
                    else
                        genre = String.Empty;

                    ShowArchiveShow(channel, arcDateSelector, genre);
                    break;
                case 2:
                    ArchiveDateSelector();
                    break;
                case 3:
                    ArchiveChannelSelector();
                    break;
                case 4:
                    ArchiveGenreSelector();
                    break;
                case 5:
                    arcDateSelector = String.Empty;
                    arcChannelSelector = String.Empty;
                    arcGenreSelector = String.Empty;
                    ArchiveSelector();
                    break;
                case 6:
                    ShowSavedSearches();
                    break;
                case 7:
                    VirtualKeyboardRU keyboard = (VirtualKeyboardRU)GUIWindowManager.GetWindow((int)WebTelek.VirtualKeyboardID);
                    if (null == keyboard) return;
                    string searchterm = string.Empty;
                    //keyboard.IsSearchKeyboard = true;
                    keyboard.Reset();
                    keyboard.Text = "";
                    keyboard.DoModal(GetID); // show it...

                    Log.Info("Rumote: OSD keyboard loaded!");

                    // If input is finished, the string is saved to the searchterm var.
                    if (keyboard.IsConfirmed)
                        searchterm = keyboard.Text;

                    // If there was a string entered try getting the article.
                    if (searchterm != "")
                    {
                        if (arcChannelSelector != String.Empty)
                            channel = archive.getChannels()[0][archive.getChannels()[1].IndexOf(arcChannelSelector)];
                        else
                            channel = String.Empty;

                        if (arcGenreSelector != String.Empty)
                            genre = archive.getGenres()[0][archive.getGenres()[1].IndexOf(arcGenreSelector)];
                        else
                            genre = String.Empty;

                        ShowArchiveShow(channel, arcDateSelector, genre, searchterm);

                        Log.Info("Rumote: Searchterm gotten from OSD keyboard: {0}", searchterm);
                    }
                    break;
                default:
                    break;
            }
        }

        private void ShowSavedSearches()
        {
            listView.Visible = true;
            listView.Clear();
            foreach (string item in _searchNames)
            {
                GUIListItem itemList = new GUIListItem();
                //itemList.Label = item.Replace("%","");
                itemList.Label = item;
                itemList.IsFolder = false;
                listView.Add(itemList);
            }
            
            _currentTypeOfList = TypeOfList.SavedSearchList;
            ChoosenList = string.Empty;

        }

        private void ShowResultOfSearch(string titleToSearchFor)
        {//Retrives search result for currently selected item
            Navigation.Insert(0, (int)NaviPlace.ARCHIVELIST);
            GUIPropertyManager.SetProperty("#Header", "Результаты поиска");
            //LastChoosen = ChoosenList;
            //ChoosenList = ARCHIVESHOWS;
            _currentTypeOfList = TypeOfList.ResultsOfSavedSearchSelection;
            _lastSearchTitle = titleToSearchFor;
            LastChoosen = string.Empty;
            ChoosenList = string.Empty;
            titleToSearchFor = System.Web.HttpUtility.UrlEncode(titleToSearchFor, Encoding.GetEncoding("windows-1251"));
            //Log.Info(titleToSearchFor);
            archivexml = webdata.getHTTPData("http://www.rumote.com/export/archive.php?action=listings&version=2.0&q=" + titleToSearchFor);
            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;
            listView.Clear();
            int j = 0;
            while (j < archive.getShows(archivexml)[0].Count)
            {
                GUIListItem item = new GUIListItem();
                item.Label = archive.getShows(archivexml)[2][j];
                item.IsFolder = false;
                getChannelLogo(archive.getShows(archivexml)[1][j]);
                item.IconImage = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\rumote\\" + archive.getShows(archivexml)[1][j] + ".jpg";
                listView.Add(item);
                j++;
            }
        }

        void ArchiveDateSelector()
        {
            chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            chooser.Reset();
            chooser.SetHeading("Выбор даты");

            int j = 0;
            while (j < archive.getDates().Count)
            {
                DateTime date = DateTime.ParseExact(archive.getDates()[j], "yyyy'-'MM'-'dd", null);
                chooser.Add(date.ToString("dddd, dd MMMM"));
                j++;
            }

            chooser.DoModal(GetID);

            if (chooser.SelectedId >= 1) arcDateSelector = archive.getDates()[chooser.SelectedId-1];
            ArchiveSelector();

        }

        void ArchiveChannelSelector()
        {
            chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            chooser.Reset();
            chooser.SetHeading("Выбор канала");

            int j = 0;
            while (j < archive.getChannels()[1].Count)
            {
                chooser.Add(archive.getChannels()[1][j]);
                j++;
            }

            chooser.DoModal(GetID);

            if (chooser.SelectedId >= 1) arcChannelSelector = archive.getChannels()[1][chooser.SelectedId - 1];
            ArchiveSelector();

        }

        void ArchiveGenreSelector()
        {
            chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            chooser.Reset();
            chooser.SetHeading("Выбор жанра");

            int j = 0;
            while (j < archive.getGenres()[1].Count)
            {
                chooser.Add(archive.getGenres()[1][j]);
                j++;
            }

            chooser.DoModal(GetID);

            if (chooser.SelectedId >= 1) arcGenreSelector = archive.getGenres()[1][chooser.SelectedId - 1];
            ArchiveSelector();

        }

        public static void getChannelLogo(string channel_id)
        {
            //Log.Info("http://www.rumote.com/img/mediaportal/" + channel_id + ".jpg");
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!File.Exists(dir + @"\\rumote\\" + channel_id + ".jpg"))
            {
                try
                {
                    WebClient client = new WebClient();
                    client.DownloadFile(
                        "http://www.rumote.com/img/mediaportal/" + channel_id + ".jpg",
                        dir + @"\\rumote\\" + channel_id + ".jpg"
                    );
                }
                catch (Exception)
                {

                }
            }

        }

        public void PlayNext(int next, bool revertcurrent)
        {
            string mmsurl = string.Empty;
            //string dir = Directory.GetCurrentDirectory();
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string tempasx = String.Empty;

            if (revertcurrent)
            {
                PlayNextIndex = PlayNextUrls.IndexOf(curr_play_url);
            }

            foreach (string file in Directory.GetFiles(dir+@"\rumote\", @"rumote*.asx")) File.Delete(file);

            if (tempasx != String.Empty) File.Delete(dir + @"\rumote\" + tempasx);

            Random random = new Random();
            tempasx = "rumote" + random.Next(999).ToString() + ".asx";

            PlayNextIndex = PlayNextIndex + next;
            if (PlayNextIndex >= PlayNextNames.Count) PlayNextIndex = 0;
            if (PlayNextIndex < 0) PlayNextIndex = PlayNextNames.Count - 1;

            if (PlayNextUrls[PlayNextIndex].Contains("http://www.rumote.com/play.php?ch="))
            {
                if (next == 0 && curr_play_url != PlayNextUrls[PlayNextIndex])
                {
                    String _tempasx = webdata.getHTTPData(PlayNextUrls[PlayNextIndex]);
                    if (preload) if (_tempasx.IndexOf("connect.wmv") > 0) _tempasx = _tempasx = _tempasx.Insert(_tempasx.IndexOf("connect.wmv") + 8, "1");
                    File.WriteAllText(dir + @"\rumote\" + tempasx, _tempasx, Encoding.Default);
                }
                mmsurl = dir + @"\rumote\" + tempasx;
                OSDInfo.channel_id = PlayNextUrls[PlayNextIndex].Substring(34);
            }
            else
            {
                mmsurl = PlayNextUrls[PlayNextIndex];
                OSDInfo.channel_id = null;
            }
            currplay = PlayNextNames[PlayNextIndex];
            OSDInfo.index = DataUrls.IndexOf(PlayNextUrls[PlayNextIndex]);
            if (next == 0 && curr_play_url != PlayNextUrls[PlayNextIndex])
            {
                Log.Debug("WebTelekPlayer: Disabling player autoclose !!!");
                isPlayNextActive = true;
                _timer.Dispose();
                _timer = new Timer();
                _timer.Interval = 10000;
                _timer.Tick += new EventHandler(_timer_Tick);
                _timer.Start();
                _timer.Enabled = true;

                g_Player.Play(mmsurl);
                curr_play_url = PlayNextUrls[PlayNextIndex];
                GUIGraphicsContext.IsFullScreenVideo = true;
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                g_Player.FullScreen = true;
            }
        }

        protected virtual void _timer_Tick(object sender, EventArgs e)
        {
            Log.Debug("WebTelekPlayer: Enabling player autoclose !!!");
            isPlayNextActive = false;
            if (_timer != null) _timer.Stop();
        }

        public void PlayChannel(string channelname)
        {
            PlayNextUrls.Clear();
            PlayNextNames.Clear();
            string[] temp = new string[DataChannelName.Count];
            DataUrls.CopyTo(temp, 0);
            PlayNextUrls.AddRange(temp);
            DataChannelName.CopyTo(temp, 0);
            PlayNextNames.AddRange(temp);

            PlayNextIndex = DataChannelName.IndexOf(channelname);

            if (curr_play_url != DataUrls[PlayNextIndex] || g_Player.Playing == false)
            {
                //string dir = Directory.GetCurrentDirectory();
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                File.Delete(dir + @"\rumote\rumote.asx");
                String _tempasx = webdata.getHTTPData(DataUrls[PlayNextIndex]);
                if (preload) if (_tempasx.IndexOf("connect.wmv") > 0) _tempasx = _tempasx.Insert(_tempasx.IndexOf("connect.wmv") + 8, "1");
                File.WriteAllText(dir + @"\rumote\rumote.asx", _tempasx, Encoding.Default);
                string mmsurl = dir + @"\rumote\rumote.asx";
                OSDInfo.channel_id = DataUrls[PlayNextIndex].Substring(34);
                g_Player.Play(mmsurl);
            }
            curr_play_url = DataUrls[PlayNextIndex];
            OSDInfo.wp = this;
            OSDInfo.index = DataUrls.IndexOf(DataUrls[PlayNextIndex]);
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            g_Player.FullScreen = true;
        }

        public override bool OnMessage(GUIMessage message)
        {

            if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS || message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED)
            {
                if (listKinozal.SelectedListItemIndex != -1)
                {
                    textKinozal.Label = "";
                    //imageKinozal.SetFileName("http://tvekran.ca/images/" + recorditems[4][listKinozal.SelectedListItemIndex]);
                    imageKinozal.SetFileName("http://iptv-distribution.net/includes/image_vod.ashx?width=100&height=148&aspect=true&vid=" + recorditems[8][0]);

                    textKinozal.Label = "Режисер: " +   recorditems[5][listKinozal.SelectedListItemIndex] + "\n" +
                                        "Актеры: "  +   recorditems[6][listKinozal.SelectedListItemIndex] + "\n" +
                                        "Продолжительность: "   +   recorditems[7][listKinozal.SelectedListItemIndex] + " минут\n" +
                                        "Сюжет: "   +   recorditems[3][listKinozal.SelectedListItemIndex];
                }
                if (listView.SelectedListItemIndex != -1)
                {
                    GUIPropertyManager.SetProperty("#ChannelInfo", " ");
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    switch (ChoosenList)
                    {
                        case "Favorites":
                            GUIPropertyManager.SetProperty("#ChannelInfo", FDescriptions[listView.SelectedListItemIndex]);
                            if (FUrls[listView.SelectedListItemIndex].Contains("http://www.rumote.com/play.php?ch="))
                            {
                                getChannelLogo(FUrls[listView.SelectedListItemIndex].Substring(34));
                                ChannelLogo.SetFileName(dir + @"\\rumote\\" + FUrls[listView.SelectedListItemIndex].Substring(34) + ".jpg");
                            }
                            else
                            {
                                ChannelLogo.SetFileName(dir + @"\\rumote\\default.jpg");
                            }
                            break;
                        case "Channels":
                            if (listView.SelectedListItemIndex - 1 >= 0) {
                                GUIPropertyManager.SetProperty("#ChannelInfo", FDescriptions[listView.SelectedListItemIndex - 1]);
                                if (FUrls[listView.SelectedListItemIndex - 1].Contains("http://www.rumote.com/play.php?ch="))
                                {
                                    Log.Info(FUrls[listView.SelectedListItemIndex-1]);
                                    getChannelLogo(FUrls[listView.SelectedListItemIndex - 1].Substring(34));
                                    ChannelLogo.SetFileName(dir + @"\\rumote\\" + FUrls[listView.SelectedListItemIndex - 1].Substring(34) + ".jpg");
                                }
                                else
                                {
                                    ChannelLogo.SetFileName(dir + @"\\rumote\\default.jpg");
                                }
                            }
                            break;
                        case "ArchiveShows":
                            getChannelLogo(archive.getShows(archivexml)[1][listView.SelectedListItemIndex]);
                            ChannelLogo.SetFileName(dir + @"\\rumote\\" + archive.getShows(archivexml)[1][listView.SelectedListItemIndex] + ".jpg");
                            break;
                        default:
                            ChannelLogo.SetFileName("DefaultFolderBig.png");
                            break;
                    }
                }
            }

            return base.OnMessage(message);
        }

        protected override void OnPageDestroy(int new_windowId)
        {
            FCategories.Clear();
            FCountries.Clear();
            FNames.Clear();
            FUrls.Clear();
            FDescriptions.Clear();
            SaveFavorites();
            SaveSearches();
            if (ChoosenList != "Channels")
            {
                ChoosenList = "";
            }
            base.OnPageDestroy(new_windowId);
        }

        protected override void OnPreviousWindow()
        {
            Categories.Clear();
            Countries.Clear();
            OSDInfo.Stop();
            OSDVolume.Stop();
            OSDProgressBar.Stop();
            //OSDNotify.Stop();
            base.OnPreviousWindow();
        }

        protected override void OnShowContextMenu()
        {
            if (_currentTypeOfList == TypeOfList.SavedSearchList)
            {//Show modal dialog to delete current saved search
                GUIDialogYesNo menu = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                menu.ResetAllControls();
                menu.SetHeading("Сохранённые поиски:");
                menu.SetDefaultToYes(true);
                menu.SetLine(1, "Удалить сохранённый поиск?");
                menu.SetLine(2, listView.SelectedListItem.Label);
                menu.DoModal(GetID);
                if (!menu.IsConfirmed) return;
                _searchNames.RemoveAt(listView.SelectedListItemIndex);
                ShowSavedSearches();
            }
            if (ChoosenList == "Channels")
            {
                GUIDialogYesNo menu = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO );
                menu.ResetAllControls();
                menu.SetHeading(btnFavorites.Label + ":");
                menu.SetDefaultToYes(true);
                menu.SetLine(1, "Добавить канал");
                menu.SetLine(2, FNames[listView.SelectedListItemIndex - 1]);
                menu.SetLine(3, "в список любимых?");
                menu.DoModal(GetID);
                if (!menu.IsConfirmed) return; 
                FavURLs.Add(FUrls[listView.SelectedListItemIndex - 1]);
            }

            if (ChoosenList == ARCHIVESHOWS)
            {

                chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                chooser.Reset();
                chooser.SetHeading("Выбор из архива");

                chooser.Add("Найти все похожие");
                chooser.Add("Сохранить как поиск");

                chooser.DoModal(GetID);

                switch (chooser.SelectedId)
                {
                    case 1:
                        string currentShow = archive.getShows(archivexml)[3][listView.SelectedListItemIndex];
                        // Code for finding similar programms in all archive based on name of the show, or similar
                        ShowResultOfSearch(ParseTitle(currentShow));
                        //ShowResultOfSearch(currentShow);
                        break;
                    case 2:
                        // Code for saving search string in user's settings
                        currentShow = archive.getShows(archivexml)[3][listView.SelectedListItemIndex];
                        //currentShow = currentShow.Replace("\"", string.Empty).Replace(".", string.Empty);

                        currentShow = ParseTitle(currentShow);
                        
                        if (!_searchNames.Contains (currentShow)) _searchNames.Add(currentShow);
                        break;
                    default:
                        break;
                }

            }
            if (ChoosenList == "Favorites")
            {
                GUIDialogYesNo menu = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                menu.ResetAllControls();
                menu.SetHeading(btnFavorites.Label + ":");
                menu.SetDefaultToYes(true);
                menu.SetLine(1, "Удалить канал");
                menu.SetLine(2, FNames[listView.SelectedListItemIndex]);
                menu.SetLine(3, "из списка любимых?");
                menu.DoModal(GetID);
                if (!menu.IsConfirmed) return;
                FavURLs.Remove(FUrls[listView.SelectedListItemIndex]);
                ShowChannels("Favorites", "", -1);
            }
            base.OnShowContextMenu();
        }

        private static string ParseTitle(string currentShow)
        {
/*            if (Regex.Match(currentShow, ".*\"(.*)\".*").Groups.Count >= 2)
            {
                currentShow = Regex.Match(currentShow, ".*\"(.*)\".*").Groups[1].ToString();
            }
*/
            //currentShow = currentShow.Replace("\"", "%");
            return currentShow;

        }
    }
}
