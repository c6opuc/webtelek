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
    public class WebTelekArchiveXML
    {
        XPathDocument xml = null;
        String archivedates = null;
        String archivechannels = null;
        public WebTelekArchiveXML(String dates, String channels)
        {
            try
            {
                archivechannels = channels;
                archivedates = dates;
            }
            catch (Exception e)
            {
                string dir = Directory.GetCurrentDirectory();
                File.AppendAllText(dir + @"\webtelek.log", "getChannels: " + e.ToString() + " \n");
            }
        }

        public StringCollection[] getChannels()
        {
            StringCollection[] result = new StringCollection[2];
            result[0] = new StringCollection();
            result[1] = new StringCollection();
            try
            {
                xml = new XPathDocument(new MemoryStream(UTF8Encoding.Default.GetBytes(archivechannels)));
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/archive/CHANNEL");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();
                    string dir = Directory.GetCurrentDirectory();
                    File.AppendAllText(dir + @"\webtelek.log", nav2.Value);
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

        public StringCollection getDates()
        {
            StringCollection result = new StringCollection();
            try
            {
                xml = new XPathDocument(new MemoryStream(UTF8Encoding.Default.GetBytes(archivedates)));
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/archive/DAY");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();
                    string dir = Directory.GetCurrentDirectory();
                    File.AppendAllText(dir + @"\webtelek.log", nav2.Value);
                    result.Add(nav2.GetAttribute("id", ""));
                }
            }
            catch (Exception e)
            {
                string dir = Directory.GetCurrentDirectory();
                File.AppendAllText(dir + @"\webtelek.log", "getDates: " + e.ToString() + " \n");
            }
            return result;
        }

        public StringCollection[] getShows(string archivexml)
        {
            StringCollection[] result = new StringCollection[2];
            result[0] = new StringCollection();
            result[1] = new StringCollection();
            try
            {
                xml = new XPathDocument(new MemoryStream(UTF8Encoding.Default.GetBytes(archivexml)));
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/archive/listing");
                XPathNodeIterator iterator = nav.Select(expr);

                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();

                    nav2.MoveToFirstChild();

                    if (nav2.Value != "")
                    {
                        string channel_id = nav2.Value;

                        nav2.MoveToNext();
                        string from = nav2.Value;

                        nav2.MoveToNext();
                        string to = nav2.Value;

                        nav2.MoveToNext(); nav2.MoveToNext();
                        string showname = nav2.Value;

                        result[0].Add(channel_id);
                        result[1].Add(from + "-" + to + " : " + showname);
                    }
                }
            }
            catch (Exception e)
            {
                string dir = Directory.GetCurrentDirectory();
                File.AppendAllText(dir + @"\webtelek.log", "ShowArchiveShow: " + e.ToString() + " \n");
            }
            return result;
        }
    }
}
