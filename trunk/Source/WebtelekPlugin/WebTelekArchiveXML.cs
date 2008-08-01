using System;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.WebTelek
{
    public class WebTelekArchiveXML
    {
        XPathDocument xml = null;
        String archivedates = null;
        String archivechannels = null;
        String archivegenres = null;

        public WebTelekArchiveXML(String dates, String channels, String genres)
        {
            try
            {
                archivechannels = channels;
                archivedates = dates;
                archivegenres = genres;
            }
            catch (Exception e)
            {
                Log.Error(e);
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
                    result[0].Add(nav2.GetAttribute("id", ""));
                    result[1].Add(nav2.GetAttribute("name", ""));
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return result;
        }

        public StringCollection[] getGenres()
        {
            StringCollection[] result = new StringCollection[2];
            result[0] = new StringCollection();
            result[1] = new StringCollection();
            try
            {
                xml = new XPathDocument(new MemoryStream(UTF8Encoding.Default.GetBytes(archivegenres)));
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/archive/GENRE");
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
                Log.Error(e);
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
                    result.Add(nav2.GetAttribute("id", ""));
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return result;
        }

        public StringCollection[] getShows(string archivexml)
        {
            StringCollection[] result = new StringCollection[4];
            result[0] = new StringCollection();
            result[1] = new StringCollection();
            result[2] = new StringCollection();
            result[3] = new StringCollection();
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
                        string show_id = nav2.Value;

                        nav2.MoveToNext();
                        string from = nav2.Value;

                        nav2.MoveToNext();
                        string to = nav2.Value;

                        nav2.MoveToNext(); nav2.MoveToNext();
                        string showname = nav2.Value;

                        DateTime datefrom = DateTime.ParseExact(from, "yyyyMMddHHmmss", null);
                        DateTime dateto = DateTime.ParseExact(to, "yyyyMMddHHmmss", null);
                        string titel = //getChannels()[1][getChannels()[0].IndexOf(channel_id)] + ":" +
                        datefrom.ToString("dddd, dd MMMM") + " " +
                        datefrom.ToString("HH:mm") + "-" + dateto.ToString("HH:mm") + " " + 
                        showname;

                        result[0].Add(show_id);
                        result[1].Add(channel_id);
                        result[2].Add(titel);
                        result[3].Add(showname);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return result;
        }
    }
}
