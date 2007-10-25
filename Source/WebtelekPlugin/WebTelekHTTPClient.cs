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
        bool _workerCompleted = true; 
        StreamReader responseStream = null;
        Encoding enc = null;

        public WebTelekHTTPClient() 
        {
            string dir = Directory.GetCurrentDirectory();
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_profile.xml"), false))
            {
                username = Convert.ToString(xmlreader.GetValueAsString("Account", "username", ""));
                password = Convert.ToString(xmlreader.GetValueAsString("Account", "password", ""));
                region = Convert.ToString(xmlreader.GetValueAsString("Account", "region", ""));
                timezone = Convert.ToString(xmlreader.GetValueAsString("Account", "timezone", ""));
            }
        }

        public string getData()
        {
            if (_workerCompleted)
            {
                _workerCompleted = false;

                BackgroundWorker worker = new BackgroundWorker();

                worker.DoWork += new DoWorkEventHandler(get_Data);
                worker.RunWorkerAsync();

                using (WaitCursor cursor = new WaitCursor())
                {
                    while (_workerCompleted == false) GUIWindowManager.Process();
                }
            }
            return responseHtml;
        }
        void get_Data(object sender, DoWorkEventArgs e)
        {
            _workerCompleted = false;
            if (username != "" && password != "" && region != "" && timezone != "")
            {
                string url = string.Format("https://www.webtelek.com/register.php?action=process");
                request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "WebTelek Plugin for MediaPortal " + WebTelek.VERSION;
                request.Method = "POST";

                request.AllowAutoRedirect = true;
                request.Accept = "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";
                request.KeepAlive = true;
                request.ContentType = @"application/x-www-form-urlencoded";
                request.Referer = string.Format("https://www.webtelek.com/register.php");
                request.CookieContainer = cookieContainer;
                string postData = string.Format("response=&email_address={0}&password={1}", username, password);
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
                url = string.Format("https://www.webtelek.com/export/channels-2.2.php?region=" + region + "&utcoffset=" + timezone);
                request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = cookieContainer;
                request.Method = "GET";
                response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
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

                BackgroundWorker worker = new BackgroundWorker();

                worker.DoWork += new DoWorkEventHandler(get_HTTP_Data);
                worker.RunWorkerAsync();

                using (WaitCursor cursor = new WaitCursor())
                {
                    while (_workerCompleted == false) GUIWindowManager.Process();
                }
            }
            return responseHtml;
        }

        void get_HTTP_Data(object sender, DoWorkEventArgs e)
        {
            string url = httpurl;
            _workerCompleted = false;

            if (responseHtml == "")
            {
                getData();
            }

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
