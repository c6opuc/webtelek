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
        XPathDocument xml = null;
        public WebTelekKinozalXML()
        {
            try
            {
            }
            catch (Exception e)
            {
                string dir = Directory.GetCurrentDirectory();
                File.AppendAllText(dir + @"\webtelek.log", "Kinozal: " + e.ToString() + " \n");
            }
        }

    }
}
