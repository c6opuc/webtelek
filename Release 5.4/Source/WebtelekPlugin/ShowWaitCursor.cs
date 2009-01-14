using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.WebTelek
{
    public class ShowWaitCursor
    {
        private string result = String.Empty;
        private string workUrl = String.Empty;
        private bool _workerCompleted = true;
        private bool Cookies = false;
        private string agent = "Mozilla/4.0 (compatible; MSIE 6.0;  WindowsNT 5.0; .NET CLR 1 .1.4322)";
        private string postType = "application/x-www-form-urlencoded";
        private string secondUrl = String.Empty;
        private HttpWebRequest firstRequest = null;
        private HttpWebResponse firstResponse = null;
        private Stream recstream = null;

        public string GetUrl(string url)
        {
           workUrl = url;

           if (_workerCompleted)
           {
               _workerCompleted = false;

                BackgroundWorker worker = new BackgroundWorker();

                worker.DoWork += new DoWorkEventHandler(DownloadWorker);
                worker.RunWorkerAsync(url);

                using (WaitCursor cursor = new WaitCursor())
                {
                    while (_workerCompleted == false)
                        GUIWindowManager.Process();
                }
            }
            
            result = ConvertString(result);
            return result;
        }

        public void DownloadWorker(object sender, DoWorkEventArgs e)
        {
                                 
            if (Cookies == false)
            {
                    firstRequest = (HttpWebRequest)WebRequest.Create(workUrl);
                    firstRequest.UserAgent = agent;
                    firstRequest.ContentType = postType;
                    firstRequest.CookieContainer = new CookieContainer();
                    firstResponse = (HttpWebResponse)
                    firstRequest.GetResponse();
                    recstream = firstResponse.GetResponseStream();
            }
            
            StringBuilder sb = new StringBuilder();

            byte[] buf = new byte[8192];

            string tempstring = null;
            int count = 0;

            do
            {
                count = recstream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    
                    tempstring = Encoding.Default.GetString(buf, 0, count);
                    sb.Append(tempstring);
                }
            }
            while (count > 0);

            result = sb.ToString(); 
            Cookies = false;
            
            _workerCompleted = true;
        }  

        public static string ConvertString(string convertstring)
        {
//            convertstring = convertstring.Replace("\n", "");
//            convertstring = convertstring.Replace("\r", "");
            convertstring = convertstring.Replace("|", "~#~");            
            convertstring = convertstring.Replace("&amp;", "&");
            convertstring = convertstring.Replace("&#39;", "'");
            convertstring = convertstring.Replace("&#193;", "Á");
            convertstring = convertstring.Replace("&#196;", "Ä");
            convertstring = convertstring.Replace("&#201;", "É");
            convertstring = convertstring.Replace("&#214;", "Ö");
            convertstring = convertstring.Replace("&#220;", "Ü");
            convertstring = convertstring.Replace("&#223;", "ß");
            convertstring = convertstring.Replace("&#224;", "à");
            convertstring = convertstring.Replace("&#225;", "á");
            convertstring = convertstring.Replace("&#226;", "â");
            convertstring = convertstring.Replace("&#227;", "ã");
            convertstring = convertstring.Replace("&#228;", "ä");
            convertstring = convertstring.Replace("&#231;", "ç");
            convertstring = convertstring.Replace("&#232;", "è");
            convertstring = convertstring.Replace("&#233;", "é");
            convertstring = convertstring.Replace("&#234;", "ê");
            convertstring = convertstring.Replace("&#235;", "ë");
            convertstring = convertstring.Replace("&#236;", "ì");
            convertstring = convertstring.Replace("&#237;", "í");
            convertstring = convertstring.Replace("&#238;", "î");
            convertstring = convertstring.Replace("&#239;", "ï");
            convertstring = convertstring.Replace("&#241;", "ñ");
            convertstring = convertstring.Replace("&#242;", "ò");
            convertstring = convertstring.Replace("&#243;", "ó");
            convertstring = convertstring.Replace("&#244;", "ô");
            convertstring = convertstring.Replace("&#245;", "õ");
            convertstring = convertstring.Replace("&#246;", "ö");
            convertstring = convertstring.Replace("&#249;", "ù");
            convertstring = convertstring.Replace("&#250;", "ú");
            convertstring = convertstring.Replace("&#251;", "û");
            convertstring = convertstring.Replace("&#252;", "ü");
            convertstring = convertstring.Replace("&#254;", "þ");

            convertstring.Trim();
            return convertstring;
        }

    }
}
