using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;
using MediaPortal.GUI.TV;
using MediaPortal.TV.Recording;
using MediaPortal.Configuration;
using System.Xml;
using System.IO;

namespace MediaPortal.GUI.WebTelek
{

    public class WebTelek : GUIWindow
    {
        
        #region SkinControlAttributes
        [SkinControlAttribute(4)]
        protected GUIButtonControl btnSelection = null;
        [SkinControlAttribute(7)]
        protected GUIButtonControl btnFavorites = null;
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
                
        public static string VERSION = "4.2";
        public static int PluginID = 6926;

        GUIDialogMenu airzonechooser = null;
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

        private string archive_channel = "";
        private string archive_date = "";

        private string currplay = "";
        private DateTime lastrefresh = DateTime.Today;
        private string kinozalurl = "";
        private StringCollection[] records = new StringCollection[2];
        private StringCollection[] recorditems = new StringCollection[8];
        private int kinozalItemIndex;
        private string SelectedChannelLogo;
        private string archive_channel_name = "";

        
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
            archiveMenuPath[0] = new StringCollection();
            archiveMenuPath[1] = new StringCollection();
            return Load(GUIGraphicsContext.Skin + @"\webtelek.xml");
        }

        protected override void OnPageLoad()
        {
            OSDVolume.Stop();
            OSDInfo.Stop();
            OSDProgressBar.Stop();
            OSDNotify.Stop();
            OSDVolume.Start();
            OSDInfo.Start();
            OSDProgressBar.Start();
            OSDNotify.Start();

            base.OnPageLoad();
            GUIPropertyManager.SetProperty("#Header",       " ");
            GUIPropertyManager.SetProperty("#ChannelInfo",  " ");

            GUIPropertyManager.SetProperty("#favorites",    "Любимые");
            GUIPropertyManager.SetProperty("#select",       "Выбор каналов");
            GUIPropertyManager.SetProperty("#archive",      "Архив передач");
            GUIPropertyManager.SetProperty("#kinozal",      "Кинозал");
            GUIPropertyManager.SetProperty("#others",       "Дополнительно");

            listView.Focus = true;
            btnArchive.Focus = false;
            btnSelection.Focus = false;
            btnFavorites.Focus = false;
            btnOthers.Focus = false;

            textKinozal.IsVisible = false;
            listKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;

            LoadFavorites();
            if (currplay != "") GUIPropertyManager.SetProperty("#Play.Current.Title", currplay);
            GetChannelData(false);

            if (airzone == "" ) airzone = webdata.region;

            //string dir = Directory.GetCurrentDirectory();
            //File.AppendAllText(dir + @"\webtelek.log", "LastChoosen: " + LastChoosen + " \n");

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
                
                if (archiveMenuPath[0].Count > 0)
                {
                    string archiveSelectedIndex = archiveMenuPath[0][archiveMenuPath[0].Count - 1];
                    string archiveSelected = archiveMenuPath[1][archiveMenuPath[1].Count - 1];
                    if (archiveSelected == ARCHIVESHOWS)
                    {
                        archiveMenuPath[0].RemoveAt(archiveMenuPath[0].Count - 1);
                        archiveMenuPath[1].RemoveAt(archiveMenuPath[1].Count - 1);
                    }
                    LastChoosen = archiveSelected;

                    switch (archiveSelected)
                    {
                        case ARCHIVEDATES:
                            ShowArchiveDates();
                            break;
                        case ARCHIVECHANNELS:
                            ShowArchiveChannels();
                            break;
                        case ARCHIVECHANNELSBYDATE:
                            ShowArchiveChannels("");
                            break;
                        case ARCHIVEDATESBYCHANNEL:
                            ShowArchiveDates("");
                            break;
                        case ARCHIVESHOWS:
                            ShowArchiveShow(archive_channel, archive_date);
                            break;
                        default:
                            break;
                    }
                    listView.SelectedListItemIndex = Int32.Parse(archiveSelectedIndex);
                    LastChoosen = archiveSelected;
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
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_profile.xml"), false))
            {
                if (Convert.ToString(xmlreader.GetValueAsString("Account", "versioncheck", "true")) == "true")
                {
                    string infomessage = webdata.getHTTPData("http://www.webtelek.com/export/maintenance-updates.php?ver=" + VERSION);
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

        private void LoadFavorites()
        {
            FavURLs.Clear();
            string dir = Directory.GetCurrentDirectory();
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_favorites.xml"), false))
            {
                for (int i = 0; i <= 1500; i++)
                {
                    string temp = Convert.ToString(xmlreader.GetValueAsString("Favorites", i.ToString(), "")).Trim();
                    if (temp != "") FavURLs.Add(temp);
                }
            }
        }

        private void SaveFavorites()
        {
            string dir = Directory.GetCurrentDirectory();
            File.Delete(Config.GetFile(Config.Dir.Config, "webtelek_favorites.xml"));
            XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "webtelek_favorites.xml"), null);
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
                archive = new WebTelekArchiveXML(webdata.getHTTPData("http://www.webtelek.com/export/archive.php?action=days"), webdata.getHTTPData("http://www.webtelek.com/export/archive.php?action=channels"));
                //xml.getXMLTV();
                webdata.getEPG();
            }
            else
            {
                if (webdata == null)
                {
                    webdata = new WebTelekHTTPClient();
                    xml = new WebTelekLiveXML(new MemoryStream(UTF8Encoding.Default.GetBytes(webdata.getData())));
                    archive = new WebTelekArchiveXML(webdata.getHTTPData("http://www.webtelek.com/export/archive.php?action=days"), webdata.getHTTPData("http://www.webtelek.com/export/archive.php?action=channels"));
                    //xml.getXMLTV();
                    webdata.getEPG();
                    GetInfoMessage();
                }
                if (xml == null) xml = new WebTelekLiveXML(new MemoryStream(UTF8Encoding.Default.GetBytes(webdata.getData())));
                if (archive == null) archive = new WebTelekArchiveXML(webdata.getHTTPData("http://www.webtelek.com/export/archive.php?action=days"), webdata.getHTTPData("http://www.webtelek.com/export/archive.php?action=channels"));

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


        private void chooseOthers()
        {
            airzonechooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            airzonechooser.Reset();
            airzonechooser.SetHeading("Дополнительно");
            airzonechooser.Add("Обновить соединение");
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
            airzonechooser.Add("Программа передач (EPG)");

            airzonechooser.DoModal(GetID);
            MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"), false);
            switch (airzonechooser.SelectedId)
            {
                case 1:
                    GetChannelData(true);
                    break;
                case 2:
                    File.Delete(Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", "")) + @"\epglastdate.dat");
                    airzone = "msk";
                    GetChannelData(true);
                    break;
                case 3:
                    File.Delete(Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", "")) + @"\epglastdate.dat");
                    airzone = "est";
                    GetChannelData(true);
                    break;
                case 4:
                    File.Delete(Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", "")) + @"\epglastdate.dat");
                    airzone = "pst";
                    GetChannelData(true);
                    break;
                case 5:
                    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVGUIDE);
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


        private void ShowArchiveDates()
        {
            GUIPropertyManager.SetProperty("#Header", "Выбор по дате");
            LastChoosen = ChoosenList;
            ChoosenList = ARCHIVEDATES;

            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;

            listView.Clear();
            int j = 0;
            while (j < archive.getDates().Count)
            {
                GUIListItem item = new GUIListItem();
                DateTime date = DateTime.ParseExact(archive.getDates()[j], "yyyy'-'MM'-'dd",null);
                item.Label = date.ToString("dddd, dd MMMM");
                item.IsFolder = true;
                MediaPortal.Util.Utils.SetDefaultIcons(item);
                listView.Add(item);
                j++;
            }
        }
        private void ShowArchiveDates(string channel)
        {
            GUIPropertyManager.SetProperty("#Header", "Выбор по дате");
            ChoosenList = ARCHIVEDATESBYCHANNEL;

            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;
            
            listView.Clear();
            int j = 0;
            while (j < archive.getDates().Count)
            {
                GUIListItem item = new GUIListItem();
                DateTime date = DateTime.ParseExact(archive.getDates()[j], "yyyy'-'MM'-'dd",null);
                item.Label = date.ToString("dddd, dd MMMM");
                item.IsFolder = true;
                MediaPortal.Util.Utils.SetDefaultIcons(item);
                listView.Add(item);
                j++;
            }
        }
        private void ShowArchiveChannels()
        {
            GUIPropertyManager.SetProperty("#Header", "Выбор по каналу");
            LastChoosen = ChoosenList;
            ChoosenList = ARCHIVECHANNELS;

            listView.Clear();
            int j = 0;
            while (j < archive.getChannels()[0].Count)
            {
                GUIListItem item = new GUIListItem();
                item.Label = archive.getChannels()[1][j];
                item.IsFolder = true;
                MediaPortal.Util.Utils.SetDefaultIcons(item);
                listView.Add(item);
                j++;
            }
        }
        private void ShowArchiveChannels(string date)
        {
            GUIPropertyManager.SetProperty("#Header", "Выбор по каналу");
            LastChoosen = ChoosenList;
            ChoosenList = ARCHIVECHANNELSBYDATE;

            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;
            
            listView.Clear();
            int j = 0;
            while (j < archive.getChannels()[0].Count)
            {
                GUIListItem item = new GUIListItem();
                item.Label = archive.getChannels()[1][j];
                item.IsFolder = true;
                MediaPortal.Util.Utils.SetDefaultIcons(item);
                listView.Add(item);
                j++;
            }
        }
        private void ShowArchiveShow(string channel, string date)
        {
            GUIPropertyManager.SetProperty("#Header", "Список выбранных программ");
            LastChoosen = ChoosenList;
            ChoosenList = ARCHIVESHOWS;
            archivexml = webdata.getHTTPData("http://www.webtelek.com/export/archive.php?action=listings&ch=" + channel + "&day=" + date);

            listView.IsVisible = true;
            textKinozal.IsVisible = false;
            imageKinozal.IsVisible = false;
            listKinozal.IsVisible = false;

            listView.Clear();
            int j = 0;
            while (j < archive.getShows(archivexml)[0].Count)
            {
                GUIListItem item = new GUIListItem();
                item.Label = archive.getShows(archivexml)[1][j];
                item.IsFolder = false;
                item.IconImage = "defaultVideo.png";
                listView.Add(item);
                j++;
            }
        }
        private void ShowCategories()
        {
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
                    ShowRecords("http://www.webtelek.com/export/kinozal.php?action=top100");
                    break;
                case 3:
                    ShowRecords("http://www.webtelek.com/export/kinozal.php?action=newrecords");
                    break;
                case 4:
                    ShowRecords("http://www.webtelek.com/export/kinozal.php?action=updates");
                    break;
                case 5:
                    ShowRecords("http://www.webtelek.com/export/kinozal.php?action=comingsoon");
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
                ShowRecords("http://www.webtelek.com/export/kinozal.php?action=records&gid=" + genres[0][chooser.SelectedId - 1]);
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
            recorditems = (new WebTelekKinozalXML(webdata)).getRecord("http://www.webtelek.com/export/kinozal.php?action=items&rid="+records[0][index]);
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

            imageKinozal.SetFileName("http://tvekran.ca/images/" + recorditems[4][0]);
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

                if ( ChoosenList == "ShowRecord" ) {

                    ShowRecords();
                    return;
                }

                if (archiveMenuPath[0].Count > 0)
                {
                    string archiveSelectedIndex = archiveMenuPath[0][archiveMenuPath[0].Count - 1];
                    string archiveSelected = archiveMenuPath[1][archiveMenuPath[1].Count - 1];
                    archiveMenuPath[0].RemoveAt(archiveMenuPath[0].Count - 1);
                    archiveMenuPath[1].RemoveAt(archiveMenuPath[1].Count - 1);
                    LastChoosen = archiveSelected;
                    switch (archiveSelected)
                    {
                        case ARCHIVEDATES:
                            ShowArchiveDates();
                            break;
                        case ARCHIVECHANNELS:
                            ShowArchiveChannels();
                            break;
                        case ARCHIVECHANNELSBYDATE:
                            ShowArchiveChannels("");
                            break;
                        case ARCHIVEDATESBYCHANNEL:
                            ShowArchiveDates("");
                            break;
                        case ARCHIVESHOWS:
                            ShowArchiveShow(archive_channel, archive_date);
                            break;
                        default:
                            break;
                    }
                    listView.SelectedListItemIndex = Int32.Parse(archiveSelectedIndex);
                    LastChoosen = archiveSelected;
                    return;
                }
                else
                {
                    ShowChannels("Favorites", "", LastSelectedChannel);
                }


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
                string url = "http://www.webtelek.com/play.php?action=vod&movieid="
                + recorditems[0][listKinozal.SelectedListItemIndex] + "&moviepart=" 
                + recorditems[1][listKinozal.SelectedListItemIndex];


                currplay = recorditems[2][listKinozal.SelectedListItemIndex];
                GUIPropertyManager.SetProperty("#Play.Current.Title", currplay);
                string dir = Directory.GetCurrentDirectory();
                File.Delete(dir + @"\webtelek.asx");
                File.WriteAllText(dir + @"\webtelek.asx", webdata.getHTTPData(url), Encoding.Default);
                url = dir + @"\webtelek.asx";
                g_Player.Play(url);
                OSDInfo.wp = null;
                OSDInfo.channel_id = null;
                OSDInfo.sched_string =  "Название :" +   recorditems[2][listKinozal.SelectedListItemIndex] + "\n" +
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

                if ( ChoosenList == ARCHIVESHOWS || ChoosenList == ARCHIVECHANNELS || ChoosenList == ARCHIVEDATES || ChoosenList == ARCHIVECHANNELSBYDATE || ChoosenList == ARCHIVEDATESBYCHANNEL)
                {
                    if (archiveMenuPath[1].IndexOf(ChoosenList) == -1)
                    {
                        archiveMenuPath[1].Add(ChoosenList);
                        archiveMenuPath[0].Add(listView.SelectedListItemIndex.ToString());
                    }

                    if (ChoosenList == ARCHIVECHANNELS)
                    {
                        archive_channel = archive.getChannels()[0][listView.SelectedListItemIndex];
                        archive_channel_name = archive.getChannels()[1][listView.SelectedListItemIndex];
                        ShowArchiveDates(archive_channel);
                        return;
                    }
                    if (ChoosenList == ARCHIVEDATES)
                    {
                        archive_date = archive.getDates()[listView.SelectedListItemIndex];
                        ShowArchiveChannels(archive_date);
                        return;
                    }
                    if (ChoosenList == ARCHIVECHANNELSBYDATE)
                    {
                        archive_channel = archive.getChannels()[0][listView.SelectedListItemIndex];
                        archive_channel_name = archive.getChannels()[1][listView.SelectedListItemIndex];
                        ShowArchiveShow(archive_channel, archive_date);
                        return;
                    }
                    if (ChoosenList == ARCHIVEDATESBYCHANNEL)
                    {
                        archive_date = archive.getDates()[listView.SelectedListItemIndex];
                        ShowArchiveShow(archive_channel, archive_date);
                        return;
                    }
                    if ( ChoosenList == ARCHIVESHOWS)
                    {
                        string url = "http://www.webtelek.com/play_pvr.php?programid=" + archive.getShows(archivexml)[0][listView.SelectedListItemIndex];
                        currplay = archive.getShows(archivexml)[1][listView.SelectedListItemIndex];
                        GUIPropertyManager.SetProperty("#Play.Current.Title", currplay);
                        string dir = Directory.GetCurrentDirectory();
                        File.Delete(dir + @"\webtelek.asx");
                        File.WriteAllText(dir + @"\webtelek.asx",webdata.getHTTPData(url),Encoding.Default);
                        url = dir + @"\webtelek.asx";
                        g_Player.Play(url);
                        OSDInfo.wp = null;
                        OSDInfo.channel_id = archive_channel;
                        OSDInfo.sched_string = archive_channel_name + " : " + archive_date + "\n" + currplay;
                        GUIGraphicsContext.IsFullScreenVideo = true;
                        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                        g_Player.FullScreen = true;
                    }
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
                        //LastChoosen = "";
                        return;
                    }

                    LastSelectedChannel = listView.SelectedListItemIndex - i;
                    string mmsurl = string.Empty;

                    string dir = Directory.GetCurrentDirectory();
                    File.Delete(dir + @"\webtelek.asx");


                    if (FUrls[listView.SelectedListItemIndex - i].Contains("http://www.webtelek.com/play.php?ch="))
                    {
                        File.WriteAllText(dir + @"\webtelek.asx",webdata.getHTTPData(FUrls[listView.SelectedListItemIndex - i]),Encoding.Default);
                        mmsurl = dir + @"\webtelek.asx";
                        OSDInfo.channel_id = FUrls[listView.SelectedListItemIndex - i].Substring(36);
                    }
                    else
                    {
                        OSDInfo.channel_id = null;
                    }
                    
                    currplay = FNames[listView.SelectedListItemIndex - i];

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

                    PlayNextIndex = listView.SelectedListItemIndex - i;
                    g_Player.Play(mmsurl);
                    OSDInfo.wp = this;
                    OSDInfo.index = DataUrls.IndexOf(FUrls[listView.SelectedListItemIndex - i]);
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
                chooser.DoModal(GetID);

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
                    default:
                        break;
                }
            }
            if (control == btnArchive)
            {
                chooser = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                chooser.Reset();
                chooser.SetHeading("Выбор из архива");
                chooser.Add("По дате");
                chooser.Add("По названию канала");
                chooser.DoModal(GetID);

                switch (chooser.SelectedId)
                {
                    case 1:
                        ShowArchiveDates();
                        listView.Focus = true;
                        btnArchive.Focus = false;
                        break;
                    case 2:
                        ShowArchiveChannels();
                        listView.Focus = true;
                        btnArchive.Focus = false;
                        break;
                    default:
                        break;
                }
            }
            if (control == btnFavorites)
            {
                archiveMenuPath[0].Clear();
                archiveMenuPath[1].Clear();
                GetChannelData(false);
                ShowChannels("Favorites", "", -1);
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

        public void PlayNext(int next)
        {
            string mmsurl = string.Empty;
            string dir = Directory.GetCurrentDirectory();
            File.Delete(dir + @"\webtelek.asx");

            PlayNextIndex = PlayNextIndex + next;
            if (PlayNextIndex >= PlayNextNames.Count) PlayNextIndex = 0;
            if (PlayNextIndex < 0) PlayNextIndex = PlayNextNames.Count - 1;

            if (PlayNextUrls[PlayNextIndex].Contains("http://www.webtelek.com/play.php?ch="))
            {
                if (next == 0) File.WriteAllText(dir + @"\webtelek.asx", webdata.getHTTPData(PlayNextUrls[PlayNextIndex]), Encoding.Default);
                mmsurl = dir + @"\webtelek.asx";
                OSDInfo.channel_id = PlayNextUrls[PlayNextIndex].Substring(36);
            }
            else
            {
                OSDInfo.channel_id = null;
            }
            currplay = PlayNextNames[PlayNextIndex];
            //g_Player.Stop();
            OSDInfo.index = DataUrls.IndexOf(PlayNextUrls[PlayNextIndex]);
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            g_Player.FullScreen = true;
            if ( next == 0 )  g_Player.Play(mmsurl);
        }

        public override bool OnMessage(GUIMessage message)
        {

            if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS || message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED)
            {
                if (listKinozal.SelectedListItemIndex != -1)
                {
                    textKinozal.Label = "";
                    imageKinozal.SetFileName("http://tvekran.ca/images/" + recorditems[4][listKinozal.SelectedListItemIndex]);
                    textKinozal.Label = "Режисер: " +   recorditems[5][listKinozal.SelectedListItemIndex] + "\n" +
                                        "Актеры: "  +   recorditems[6][listKinozal.SelectedListItemIndex] + "\n" +
                                        "Продолжительность: "   +   recorditems[7][listKinozal.SelectedListItemIndex] + " минут\n" +
                                        "Сюжет: "   +   recorditems[3][listKinozal.SelectedListItemIndex];
                }
                if (listView.SelectedListItemIndex != -1)
                {
                    GUIPropertyManager.SetProperty("#ChannelInfo", " ");

                    switch (ChoosenList)
                    {
                        case "Favorites":
                            GUIPropertyManager.SetProperty("#ChannelInfo", FDescriptions[listView.SelectedListItemIndex]);
                            if (FUrls[listView.SelectedListItemIndex].Contains("http://www.webtelek.com/play.php?ch="))
                            {
                                if ( ! File.Exists("webtelek\\" + FUrls[listView.SelectedListItemIndex].Substring(36) + ".jpg"))
                                {
                                    try
                                    {
                                        WebClient client = new WebClient();
                                        client.DownloadFile(
                                            "http://www.webtelek.com/img/mediaportal/" + FUrls[listView.SelectedListItemIndex].Substring(36) + ".jpg",
                                            "webtelek\\" + FUrls[listView.SelectedListItemIndex].Substring(36) + ".jpg"
                                        );
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                                ChannelLogo.SetFileName("webtelek\\" + FUrls[listView.SelectedListItemIndex].Substring(36) + ".jpg");
                            }
                            else
                            {
                                ChannelLogo.SetFileName("webtelek\\default.jpg");
                            }
                            break;
                        case "Channels":
                            if (listView.SelectedListItemIndex - 1 >= 0) {
                                GUIPropertyManager.SetProperty("#ChannelInfo", FDescriptions[listView.SelectedListItemIndex - 1]);
                                if (FUrls[listView.SelectedListItemIndex - 1].Contains("http://www.webtelek.com/play.php?ch="))
                                {
                                    if (!File.Exists("webtelek\\" + FUrls[listView.SelectedListItemIndex - 1].Substring(36) + ".jpg"))
                                    {
                                        try {
                                            WebClient client = new WebClient();
                                            client.DownloadFile(
                                            "http://www.webtelek.com/img/mediaportal/" + FUrls[listView.SelectedListItemIndex - 1].Substring(36) + ".jpg",
                                            "webtelek\\" + FUrls[listView.SelectedListItemIndex - 1].Substring(36) + ".jpg"
                                            );
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }
                                    ChannelLogo.SetFileName("webtelek\\" + FUrls[listView.SelectedListItemIndex - 1].Substring(36) + ".jpg");
                                }
                                else
                                {
                                    ChannelLogo.SetFileName("webtelek\\default.jpg");
                                }
                            }
                            break;
                        case "ArchiveChannels":
                        case "ArchiveChannelsByDate":
                            SelectedChannelLogo = "webtelek\\" + archive.getChannels()[0][listView.SelectedListItemIndex] + ".jpg";
                            if (!File.Exists(SelectedChannelLogo))
                            {
                                try
                                {
                                    WebClient client = new WebClient();
                                    client.DownloadFile(
                                        "http://www.webtelek.com/img/mediaportal/" + archive.getChannels()[0][listView.SelectedListItemIndex] + ".jpg",
                                        SelectedChannelLogo
                                    );
                                }
                                catch (Exception)
                                {

                                }
                            }
                            ChannelLogo.SetFileName(SelectedChannelLogo);
                            break;
                        case "ArchiveShows":
                            ChannelLogo.SetFileName(SelectedChannelLogo);
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
            OSDNotify.Stop();
            base.OnPreviousWindow();
        }

        protected override void OnShowContextMenu()
        {
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
    }
}
