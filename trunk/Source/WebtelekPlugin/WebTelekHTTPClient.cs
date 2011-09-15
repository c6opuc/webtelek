using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Playlists;
using MediaPortal.Configuration;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.WebTelek
{
    public class WebTelekHTTPClient
    {
        public  CookieContainer cookieContainer = new CookieContainer();
        public  HttpWebRequest request = null;
        public  HttpWebResponse response = null;
        public  string responseHtml = "";
        public  string responseHTTPData = "";
        string username = "";
        string password = "";
        public string region = "";
        string timezone = "";
        string httpurl = "";
        string epgdays = "2";
        bool _workerCompleted = true; 
        StreamReader responseStream = null;
        Encoding enc = null;
        BackgroundWorker worker = null;
        int timeout = 0;
        System.Windows.Forms.Timer _timer = null;

        public WebTelekHTTPClient() 
        {
            //string dir = Directory.GetCurrentDirectory();
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_profile.xml"), false))
            {
                username = Convert.ToString(xmlreader.GetValueAsString("Account", "username", ""));
                password = Convert.ToString(xmlreader.GetValueAsString("Account", "password", ""));
                region = Convert.ToString(xmlreader.GetValueAsString("Account", "region", ""));
                timezone = Convert.ToString(xmlreader.GetValueAsString("Account", "timezone", ""));
                epgdays = Convert.ToString(xmlreader.GetValueAsString("Account", "epgdays", "2"));
                timeout = (int)Decimal.Parse(Convert.ToString(xmlreader.GetValueAsString("Account", "netdelay", "15"))) * 1000;
            }
        }

        public void getEPG(Boolean refresh)
        {

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"), false))
            {
                string dirname = Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", ""));
                if (refresh)
                {
                    if (File.Exists(dirname + @"\epglastdate.dat"))
                    {
                        File.Delete(dirname + @"\epglastdate.dat");
                    }
                }
                getEPG();
            }
        }
        public void getEPG()
        {
            
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"), false))
            {
                if (Convert.ToString(xmlreader.GetValueAsString("Account", "epgload", "true")) == "true")
                {
                    //string dirname = Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", ""));
                    string dirname = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string fromdate = DateTime.Today.ToString("yyyy-MM-dd");
                    DateTime lastdate = DateTime.Now.Date;
                    DateTime now = DateTime.Now.Date;

                    if (File.Exists(dirname + @"\epglastdate.dat"))
                    {
                        //DateTime lastdate = DateTime.Parse(File.ReadAllText(dirname + @"\epglastdate.dat"));
                        lastdate = DateTime.Parse(File.ReadAllText(dirname + @"\epglastdate.dat"));
                        if (lastdate >= now)
                        {
                            TimeSpan diff = lastdate.Subtract(now);
                            epgdays = (Decimal.Parse(epgdays) - diff.Days).ToString();
                            fromdate = lastdate.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            lastdate = now;
                        }
                    }
                    if (epgdays != "0")
                    {
                        //File.WriteAllText(dirname + @"\epg.log", "EPG:" + "http://www.rumote.com/export/epg.php?from=" + fromdate + "&days=" + epgdays);
                        string tvguide = getHTTPData("http://www.rumote.com/export/epg.php?from=" + fromdate + "&days=" + epgdays);
                        File.Delete(dirname + @"\webtelek\tvguide.xml");
                        tvguide = Regex.Replace(tvguide, "windows-1251", "utf-8");
                        File.WriteAllText(dirname + @"\webtelek\tvguide.xml", tvguide, Encoding.UTF8);
                        File.Delete(dirname + @"\webtelek\epglastdate.dat");
                        File.WriteAllText(dirname + @"\webtelek\epglastdate.dat", lastdate.AddDays(Double.Parse(epgdays)).Date.ToString());
                    }
                }
            }

        }

        public string getData()
        {
            if (_workerCompleted)
            {
                _workerCompleted = false;

                worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;

                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = timeout;
                _timer.Enabled = true;
                _timer.Tick += new EventHandler(_timer_Tick);

                worker.DoWork += new DoWorkEventHandler(get_Data);
                worker.RunWorkerAsync();

                using (WaitCursor cursor = new WaitCursor())
                {
                    while (_workerCompleted == false) GUIWindowManager.Process();
                }
            }
            _timer.Enabled = false;
            return responseHtml;
        }

        protected virtual void _timer_Tick(object sender, EventArgs e)
        {
            worker.CancelAsync();
            GUIDialogNotify info = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
            info.Reset();
            info.IsOverlayAllowed = true;
            info.SetHeading("Ошибка");
            info.SetText("Не удалось установить связь с сервером. Проверте свои сетевые настройки.");
            info.DoModal(WebTelek.PluginID);
            _timer.Dispose();
            _workerCompleted = true;
        }

        void get_Data(object sender, DoWorkEventArgs e)
        {
            _workerCompleted = false;
            if (username != "" && password != "" && region != "" && timezone != "")
            {
                string url = string.Format("https://www.rumote.com/register.php?action=process");
                request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "WebTelek Plugin for MediaPortal " + WebTelek.VERSION;
                request.Method = "POST";

                request.AllowAutoRedirect = true;
                request.Accept = "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";
                request.KeepAlive = true;
                request.ContentType = @"application/x-www-form-urlencoded";
                request.Referer = string.Format("https://www.rumote.com/register.php");
                request.CookieContainer = cookieContainer;
                string postData = string.Format("response=&email_address={0}&password={1}&persistent={2}", username, password, "y");
                request.Method = "POST";

                byte[] postBuffer = System.Text.Encoding.GetEncoding(1252).GetBytes(postData);
                request.ContentLength = postBuffer.Length;
                Stream postDataStream = request.GetRequestStream();
                postDataStream.Write(postBuffer, 0, postBuffer.Length);
                postDataStream.Close();

                response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

                // Read the response from the stream
                enc = Encoding.Default;
                responseStream = new StreamReader(response.GetResponseStream(), enc, true);
                responseHtml = responseStream.ReadToEnd();
                url = string.Format("https://www.rumote.com/export/channels-2.2.php?region=" + region + "&utcoffset=" + timezone);
                request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = cookieContainer;
                request.Method = "GET";
                response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                
                //Log.Info(response.Cookies.ToString());

                responseStream = new StreamReader(response.GetResponseStream(), enc, true);
                responseHtml = responseStream.ReadToEnd();
                
                response.Close();
                responseStream.Close();
            }
            _workerCompleted = true;
        }

        public string getHTTPData(string url)
        {
            httpurl = url;
            if (_workerCompleted)
            {
                _workerCompleted = false;

                worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;

                worker.DoWork += new DoWorkEventHandler(get_HTTP_Data);
                worker.RunWorkerAsync();

                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = timeout;
                _timer.Enabled = true;
                _timer.Tick += new EventHandler(_timer_Tick);

                using (WaitCursor cursor = new WaitCursor())
                {
                    while (_workerCompleted == false) GUIWindowManager.Process();
                }
            }
            _timer.Enabled = false;
            return responseHtml;
        }

        void get_HTTP_Data(object sender, DoWorkEventArgs e)
        {
            string url = httpurl;
            _workerCompleted = false;

            /*
            if (responseHtml == "")
            {
                getData();
            }
            */

            request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = cookieContainer;
            request.Method = "GET";
            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            responseStream = new StreamReader(response.GetResponseStream(), enc, true);
            responseHtml = responseStream.ReadToEnd();
            response.Close();
            responseStream.Close();
            _workerCompleted = true;
        }
    }
}
