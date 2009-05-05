#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;

using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace MediaPortal.GUI.WebTelek
{
    public class WebTelekPlugin : ISetupForm, IShowPlugin
    {
        // Returns the name of the plugin which is shown in the plugin menu
        public string PluginName()
        {
            return "WEBTELEK";
        }

        // Returns the description of the plugin is shown in the plugin menu
        public string Description()
        {
            return "WEBTELEK+ Frontend Plugin " + WebTelek.VERSION;
        }

        // Returns the author of the plugin which is shown in the plugin menu
        public string Author()
        {
            return "6opuc (Borys Saulyak)";
        }

        // show the setup dialog
        public void ShowPlugin()
        {
            Form setup = new ConfigurationForm();
            setup.ShowDialog();
        }
        // Indicates whether plugin can be enabled/disabled
        public bool CanEnable()
        {
            return true;
        }

        // get ID of windowplugin belonging to this setup
        public int GetWindowId()
        {
            return WebTelek.PluginID;
        }

        // Indicates if plugin is enabled by default;
        public bool DefaultEnabled()
        {
            return true;
        }
        // indicates if a plugin has its own setup screen
        public bool HasSetup()
        {
            return true;
        }

        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = "WEBTELEK+";
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_profile.xml"), false))
            {
                strButtonText = Convert.ToString(xmlreader.GetValueAsString("Account", "pluginname", "WEBTELEK+"));
            }
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = "hover_webtelek.png";
            return true;
        }

        #region IShowPlugin Members

        public bool ShowDefaultHome()
        {
            return true;
        }

        #endregion

    }
}
