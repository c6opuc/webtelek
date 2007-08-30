using System;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace MediaPortal.GUI.WebTelek
{
    public class WebTelekKinozalXML
    {
        WebTelekHTTPClient webdata = null;
        XPathDocument xml = null;
        public WebTelekKinozalXML(WebTelekHTTPClient webdata)
        {
            try
            {
                this.webdata = webdata;
            }
            catch (Exception e)
            {
                string dir = Directory.GetCurrentDirectory();
                File.AppendAllText(dir + @"\webtelek.log", "Kinozal: " + e.ToString() + " \n");
            }
        }
        public StringCollection[] getCategories()
        {
            StringCollection[] result = new StringCollection[2];
            result[0] = new StringCollection();
            result[1] = new StringCollection();
            try
            {
                xml = new XPathDocument(new MemoryStream(UTF8Encoding.Default.GetBytes(webdata.getHTTPData("http://www.webtelek.com/export/kinozal.php?action=categories"))));
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/vod/CATEGORY");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();
                    
                    result[0].Add(nav2.GetAttribute("id", ""));
                    result[1].Add(nav2.GetAttribute("name", ""));
                    //string dir = Directory.GetCurrentDirectory();
                    //File.AppendAllText(dir + @"\webtelek.log", nav2.GetAttribute("id", "") + nav2.GetAttribute("name", ""));
                }
            }
            catch (Exception e)
            {
                string dir = Directory.GetCurrentDirectory();
                File.AppendAllText(dir + @"\webtelek.log", "getChannels: " + e.ToString() + " \n");
            }
            return result;
        }
        public StringCollection[] getGenres(string cid)
        {
            StringCollection[] result = new StringCollection[2];
            result[0] = new StringCollection();
            result[1] = new StringCollection();

            try
            {
                xml = new XPathDocument(new MemoryStream(UTF8Encoding.Default.GetBytes(webdata.getHTTPData("http://www.webtelek.com/export/kinozal.php?action=genres&cid="+cid))));
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/vod/GENRE");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();

                    result[0].Add(nav2.GetAttribute("id", ""));
                    result[1].Add(nav2.GetAttribute("name", ""));
                }
            }
            catch (Exception e)
            {
                string dir = Directory.GetCurrentDirectory();
                File.AppendAllText(dir + @"\webtelek.log", "getChannels: " + e.ToString() + " \n");
            }

            return result;
        }
        public StringCollection[] getRecords(string url)
        {
            StringCollection[] result = new StringCollection[2];
            result[0] = new StringCollection();
            result[1] = new StringCollection();

            try
            {
                xml = new XPathDocument(new MemoryStream(UTF8Encoding.Default.GetBytes(webdata.getHTTPData(url))));
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/vod/RECORD");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();

                    result[0].Add(nav2.GetAttribute("id", ""));
                    result[1].Add(nav2.GetAttribute("name", ""));
                }
            }
            catch (Exception e)
            {
                string dir = Directory.GetCurrentDirectory();
                File.AppendAllText(dir + @"\webtelek.log", "getChannels: " + e.ToString() + " \n");
            }

            return result;
        }
    }
}
