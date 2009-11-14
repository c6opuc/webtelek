using System;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using MediaPortal.Configuration;


namespace MediaPortal.GUI.WebTelek
{
    public class WebTelekLiveXML 
    {
        XPathDocument xml = null;
        public WebTelekLiveXML(Stream input)
        {
            try
            {                               
                xml = new XPathDocument(input);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString()); 
            }
        }

        public StringCollection getCustom(string param)
        {
            StringCollection result = new StringCollection();
            string dir = Directory.GetCurrentDirectory();
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_custom.xml"), false))
            {
                for (int i = 0; i <= 1500; i++)
                {
                    string customstr = Convert.ToString(xmlreader.GetValueAsString(i.ToString(), param, ""));
                    if (customstr.Equals(""))
                    {
                        return result;
                    } 
                    else 
                    {
                        result.Add(customstr);
                    }
                }
            }
            return result;
        }

        public StringCollection getCountries()
        {
            StringCollection result = new StringCollection();
            try
            {
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/WebTelek/channel/country");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();
                    result.Add(nav2.Value);
                }
            }
            catch (Exception)
            {

            }
            return result;
        }

        public StringCollection getCategories()
        {
            StringCollection result = new StringCollection();
            try
            {
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/WebTelek/channel/category");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();
                    result.Add(nav2.Value);
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
        public StringCollection getChannelId()
        {
            StringCollection result = new StringCollection();
            try
            {
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/WebTelek/channel/id");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();
                    result.Add(nav2.Value);
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
        public StringCollection getChannelsNames()
        {
            StringCollection result = new StringCollection();
            try
            {
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/WebTelek/channel/name");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();
                    result.Add(nav2.Value);
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
        public StringCollection getURLs()
        {
            StringCollection result = new StringCollection();
            try 
            {
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("/WebTelek/channel/id");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();
                    result.Add("http://www.webtelek.com/members/play.php?ch=" + nav2.Value);
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
        public StringCollection getSchedules()
        {
            StringCollection result = new StringCollection();
            try
            {
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;
                for (int i = 0; i <= getChannelId().Count - 1; i++)
                {
                    string channellist = "";
                    expr = nav.Compile("/WebTelek/channel[id=" + getChannelId()[i] + "]/listing/*");
                    XPathNodeIterator iterator = nav.Select(expr);
                    int j = 0;
                    int k = 0;
                    try
                    {
                        while (iterator.MoveNext() && j <= 3)
                        {
                            string showfrom = "";
                            string showthru = "";
                            string showtype = "";
                            string showtitle = "";
                            XPathNavigator nav2 = iterator.Current.Clone();
                            showfrom = nav2.Value;
                            iterator.MoveNext();
                            nav2 = iterator.Current.Clone();
                            showthru = nav2.Value;
                            iterator.MoveNext();
                            nav2 = iterator.Current.Clone();
                            showtype = nav2.Value;
                            iterator.MoveNext();
                            nav2 = iterator.Current.Clone();
                            showtitle = nav2.Value;
                            DateTime currtime = DateTime.Now;
                            DateTime from = DateTime.Parse(showfrom);
                            DateTime to = DateTime.Parse(showthru);
                            if (to < from && k > 0) to = to.AddDays(1);
                            if (from > to  && k == 0) from = from.AddDays(-1);
                            if ( ((from <= currtime) && (to > currtime)) || ((from > currtime) && (to > currtime)) )
                            {
                                channellist = channellist + showfrom + "-" + showthru + " : " + showtitle + "\n";
                                j++;
                            }
                            k++;

                        }
                    }
                    catch (Exception)
                    {
                        channellist = "нет данных";
                    }
                    result.Add(channellist);
                }
            } 
            catch (Exception) 
            {
            }
           return result;
        }
        public void getXMLTV()
        {
            string tvguide = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<tv generator-info-name=\"generated by webtelek+\">\n";

            try
            {
                XPathNavigator nav = xml.CreateNavigator();
                XPathExpression expr;

                for (int i = 0; i <= getChannelId().Count - 1; i++)
                {
                    string channellist = "";
                    expr = nav.Compile("/WebTelek/channel[id=" + getChannelId()[i] + "]/listing/*");
                    XPathNodeIterator iterator = nav.Select(expr);

                    tvguide = tvguide + "\n<channel id=\"" + getChannelId()[i] + "\"><display-name>"+  getChannelsNames()[i] + "</display-name><icon src=\"http://www.webtelek.com/img/mediaportal/" + getChannelId()[i] + ".jpg\"></icon></channel>\n";

                    int k = 0;
                    try
                    {
                        while (iterator.MoveNext())
                        {
                            string showfrom = "";
                            string showthru = "";
                            string showtype = "";
                            string showtitle = "";
                            string showicon = "";
                            XPathNavigator nav2 = iterator.Current.Clone();
                            XPathNavigator nav3 = iterator.Current.Clone();
                            showfrom = nav2.Value;
                            iterator.MoveNext();
                            nav2 = iterator.Current.Clone();
                            showthru = nav2.Value;
                            iterator.MoveNext();
                            nav2 = iterator.Current.Clone();
                            nav3 = iterator.Current.Clone();
                            nav3.MoveToFirstChild();
                            showtype = nav3.GetAttribute("alt","");
                            showicon = nav3.GetAttribute("src", "");
                            iterator.MoveNext();
                            nav2 = iterator.Current.Clone();
                            showtitle = nav2.Value;
                            DateTime currtime = DateTime.Now;
                            DateTime from = DateTime.Parse(showfrom);
                            DateTime to = DateTime.Parse(showthru);
                            if (to < from && k > 0) to = to.AddDays(1);
                            if (from > to && k == 0) from = from.AddDays(-1);
                            if (((from <= currtime) && (to > currtime)) || ((from > currtime) && (to > currtime)))
                            {
                                channellist = channellist + showfrom + "-" + showthru + " : " + showtitle + "\n";
                                tvguide = tvguide + "<programme start=\"" +
                                from.ToString("yyyyMMddHHmm") + "\" channel=\"" + getChannelId()[i] + "\">" + 
                                "<title>" + showtitle + "</title>" +
                                "<desc>" + showtype + "</desc>" +
                                "<category>" + showtype + "</category>" +
                                "<icon src=\"http://webtelek.com" + showicon + "\"></icon>" +
                                "<video><aspect>4:3</aspect></video>" +
                                "</programme>\n";
                            }
                            k++;
                        }
                    }
                    catch (Exception)
                    {
                        channellist = "нет данных";
                    }
                }
            }
            catch (Exception)
            {
            }
            tvguide = tvguide + "</tv>\n";

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"), false))
            {
                string dirname = Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", ""));
                File.Delete(dirname + @"\tvguide.xml");
                File.WriteAllText(dirname + @"\tvguide.xml", tvguide, Encoding.UTF8);
            }
        }
   }
}
