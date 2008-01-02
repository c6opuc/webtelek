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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Configuration;
using MediaPortal.TV.Database;
using MediaPortal.Util;


namespace MediaPortal.GUI.WebTelek
{
    public partial class OSDNotify : Form
    {
        Bitmap _top;
        Bitmap _bitmap;
        static Timer _timer = new Timer();
        static Timer _notifytimer = null;
        Form _parent;
        static OSDNotify _osd = null;
        EventHandler _losc;
        readonly PointF[] _pathPoints;
        bool _notifiesListChanged;
        int _preNotifyConfig;
        List<TVNotify> _notifiesList;
        Boolean origNotifier;

        public static void Start()
        {
            if (_osd == null)
            {
                _osd = new OSDNotify(Application.OpenForms[0]);
            }
        }

        public static void Stop()
        {
            if (_osd != null)
            {
                _osd.Hide();
            }
        }

        public OSDNotify(Form parent)
        {
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
                _preNotifyConfig = xmlreader.GetValueAsInt("movieplayer", "notifyTVBefore", 300);

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
                if (xmlreader.GetValue("plugins", "TV Notifier") == "yes") origNotifier = false; else origNotifier = true;

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_profile.xml"), false))
                _timer.Interval = (int)Decimal.Parse(Convert.ToString(xmlreader.GetValueAsString("Account", "osddelay", "5")))*1000;

            _notifiesList = new List<TVNotify>();
            TVDatabase.OnNotifiesChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(OnNotifiesChanged);
            _notifytimer = new System.Windows.Forms.Timer();
            _notifytimer.Interval = 15000;
            _notifytimer.Enabled = false;
            _notifytimer.Tick += new EventHandler(_notifytimer_Tick);

            LoadNotifies();
            _notifytimer.Enabled = true;

            ConfigXmlDocument cxd = new ConfigXmlDocument();
            cxd.Load(GUIGraphicsContext.Skin + @"\WMP9Notify.xml");
            _top = new Bitmap(GUIGraphicsContext.Skin + @"\Media\" + cxd.GetElementsByTagName("TopBackground")[0].InnerText);
            _bitmap = new Bitmap(600, 200);

            try
            {
                XmlSerializer s = new XmlSerializer(typeof(PointF[]));
                FileStream fs = new FileStream(GUIGraphicsContext.Skin + @"\" + cxd.GetElementsByTagName("TopRegion")[0].InnerText, FileMode.Open);
                _pathPoints = (PointF[])s.Deserialize(fs);
            }
            catch (Exception ex)
            {
                Log.Error(ex);  
            }           
            _losc = new EventHandler(parent_LocationOrSizeChanged);
            InitializeComponent();
            _parent = parent;
            this.Opacity = 0; 
            base.Show(_parent);
            parent_LocationOrSizeChanged(null, null);
            _parent.LocationChanged += _losc;
            _parent.SizeChanged += _losc;
            _timer.Tick += new EventHandler(_timer_Tick);
            parent.Focus();
        }

        void OnNotifiesChanged()
        {
            _notifiesListChanged = true;
        }
        void LoadNotifies()
        {
            _notifiesList.Clear();
            TVDatabase.GetNotifies(_notifiesList, true);
        }

        void _notifytimer_Tick(object sender, EventArgs e)
        {
            if (origNotifier)
            {
                DateTime preNotifySecs = DateTime.Now.AddSeconds(_preNotifyConfig);
                if (_notifiesListChanged)
                {
                    LoadNotifies();
                    _notifiesListChanged = false;
                }
                for (int i = 0; i < _notifiesList.Count; ++i)
                {
                    TVNotify notify = _notifiesList[i];
                    if (preNotifySecs > notify.Program.StartTime)
                    {
                        TVDatabase.DeleteNotify(notify);
                        _timer.Enabled = false;
                        if (g_Player.Player != null)
                        {
                            if ((g_Player.Playing | g_Player.Paused) & g_Player.FullScreen & g_Player.HasVideo & (g_Player.Player.GetType() == typeof(MediaPortal.Player.AudioPlayerWMP9)))
                            {
                                MediaPortal.Util.Utils.PlaySound("notify.wav", false, true);
                                this.Show(notify);
                                _timer.Enabled = true;
                            }
                            else
                            {
                                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM, 0, 0, 0, 0, 0, null);
                                msg.Object = notify.Program;
                                GUIGraphicsContext.SendMessage(msg);
                                msg = null;
                            }
                        }
                        else
                        {
                            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM, 0, 0, 0, 0, 0, null);
                            msg.Object = notify.Program;
                            GUIGraphicsContext.SendMessage(msg);
                            msg = null;
                        }

                    }                   
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            _parent.LocationChanged += _losc;
            _parent.SizeChanged += _losc;
            _timer.Dispose();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        void parent_LocationOrSizeChanged(object sender, EventArgs e)
        {
            this.Location = new Point((int)(_parent.Location.X + _parent.Width/2-_bitmap.Width/2),(int)(_parent.Location.Y + _parent.Height-_bitmap.Height-_parent.Height*0.01));
            this.Size = new Size(600,200);
            if (_pathPoints != null)
            {
                try
                {
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddPolygon(_pathPoints);
                    Matrix m = new Matrix();
                    m.Scale(this.Size.Width / (float)_top.Width, this.Size.Height / (float)_top.Height);
                    gp.Transform(m);
                    this.Region = new Region(gp);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        protected virtual void _timer_Tick(object sender, EventArgs e)
        {
            this.Hide();
        }

        private new void Show()
        {
        }

        public void Show(TVNotify notify)
        {
            this.Size = new Size(600, 200);
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                try
                {
                    g.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(0, _bitmap.Height), Color.White, Color.Black), 0, 0, _bitmap.Width, _bitmap.Height);
                    g.DrawImage(_top, 0, 0, _bitmap.Width, _bitmap.Height);
                    Font f = new Font("Arial", 20);
                    g.DrawString(notify.Program.Genre + "\n" + notify.Program.Title + "\nначнется в " + notify.Program.StartTime.TimeOfDay + " на канале " + notify.Program.Channel, f, new SolidBrush(Color.Black), new RectangleF(42, 22, _bitmap.Width - 70 - 49, _bitmap.Height - 40));
                    g.DrawString(notify.Program.Genre + "\n" + notify.Program.Title + "\nначнется в " + notify.Program.StartTime.TimeOfDay + " на канале " + notify.Program.Channel, f, new SolidBrush(Color.White), new RectangleF(40, 20, _bitmap.Width - 70 - 49, _bitmap.Height - 40));
                    if (File.Exists(Config.GetFile(Config.Dir.Config, @"Thumbs\TV\logos", notify.Program.Channel + ".jpg"))) 
                    {
                        Bitmap logo = new Bitmap(Config.GetFile(Config.Dir.Config, @"Thumbs\TV\logos", notify.Program.Channel + ".jpg"));
                        g.DrawRectangle(new Pen(Color.Black, 2), _bitmap.Width - 70 - 40 + 17 + 2, 27 + 2, 49, 37);
                        g.DrawImage(logo, _bitmap.Width - 70 - 40 + 17, 27, 49, 37);
                        g.DrawRectangle(new Pen(Color.Gray, 2), _bitmap.Width - 70 - 40 + 17, 27, 49, 37);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
            this.Opacity = 1;
            this.Refresh();
        }

        private new void Hide()
        {
            this.Size = new Size(0, 0);
            this.Opacity = 0;
            this.Refresh();
        } 


        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                e.Graphics.DrawImage(_bitmap, new Rectangle(0, 0, this.Width, this.Height));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}