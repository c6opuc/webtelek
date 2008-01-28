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

namespace MediaPortal.GUI.WebTelek
{
    public partial class OSDInfo : Form
    {
        Bitmap _top;
        Bitmap _bitmap;
        Timer _timer = new Timer();
        Timer _playtimer = new Timer();
        Boolean _changeOnOKonly = false;
        Form _parent;
        static bool _enabled;
        static OSDInfo _osd = null;
        g_Player.EndedHandler _gpeh;
        EventHandler _losc;
        OnActionHandler _ahandler;
        readonly PointF[] _pathPoints;
        public static string sched_string;
        public static WebTelek wp;
        public static int index;
        public static string channel_id;
        int interval;

        public static void Start()
        {
            if (_osd == null)
            {
                _osd = new OSDInfo(Application.OpenForms[0]);
            }
            _enabled = true;
        }

        public static void Stop()
        {
            //TODO: Use g_Player events ?
            if (_osd != null)
            {
                _osd.Dispose(true);
                _osd = null;
            }
        }

        public OSDInfo(Form parent)
        {
            //TODO: MediaPortal closes when WMP9OSD file is not found, why? 
            ConfigXmlDocument cxd = new ConfigXmlDocument();
            cxd.Load(GUIGraphicsContext.Skin + @"\WMP9SCHED.xml");
            _top = new Bitmap(GUIGraphicsContext.Skin + @"\Media\" + cxd.GetElementsByTagName("TopBackground")[0].InnerText);
            _bitmap = new Bitmap(600, 200);

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_profile.xml"), false))
            {
                _timer.Interval = (int)Decimal.Parse(Convert.ToString(xmlreader.GetValueAsString("Account", "osddelay", "5"))) * 1000;
                _changeOnOKonly = Boolean.Parse(Convert.ToString(xmlreader.GetValueAsString("Account", "switchonokonly", "true")));
                _playtimer.Interval = (int)Decimal.Parse(Convert.ToString(xmlreader.GetValueAsString("Account", "switchtimeout", "1"))) * 1000;
                if (_changeOnOKonly) _playtimer.Interval = _timer.Interval;
                interval = _timer.Interval;
            }

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
            _gpeh = new g_Player.EndedHandler(g_Player_PlayBackEnded);
            InitializeComponent();
            _parent = parent;
            this.Opacity = 0; 
            base.Show(_parent);
            parent_LocationOrSizeChanged(null, null);
            _parent.LocationChanged += _losc;
            _parent.SizeChanged += _losc;
            _timer.Tick += new EventHandler(_timer_Tick);
            _playtimer.Tick += new EventHandler(_playtimer_Tick);
            g_Player.PlayBackEnded += _gpeh;//TODO: Does not work as expected, add g_Player.PlayBackStopped  += _gpeh;?
            _ahandler = new OnActionHandler(GUIWindowManager_OnNewAction);
            GUIWindowManager.OnNewAction += _ahandler;
            parent.Focus();
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            g_Player.PlayBackEnded -= _gpeh;
            GUIWindowManager.OnNewAction -= _ahandler;
            _parent.LocationChanged += _losc;
            _parent.SizeChanged += _losc;
            _timer.Dispose();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        void g_Player_PlayBackEnded(g_Player.MediaType type, string filename)
        {
            _enabled = false;
            _timer.Enabled = false;
        }

        void GUIWindowManager_OnNewAction(Action action)
        {
            //string dir = Directory.GetCurrentDirectory();
            //File.AppendAllText(dir + @"\webtelek.log", "OSD: " + action.wID.ToString() + " \n");
            if (g_Player.Player != null)
            {
                switch (action.wID)
                {
                    case Action.ActionType.ACTION_SHOW_OSD:
                    case Action.ActionType.ACTION_CONTEXT_MENU:
                        if (_enabled)
                            if ((g_Player.Playing | g_Player.Paused) & g_Player.FullScreen & g_Player.HasVideo & (g_Player.Player.GetType() == typeof(MediaPortal.Player.AudioPlayerWMP9)))
                            {
                                _timer.Enabled = false;
                                if (g_Player.FullScreen)
                                {
                                    if (wp == null)
                                    {
                                        this.Show(sched_string);
                                    }
                                    else
                                    {
                                        wp.GetChannelData(false);
                                        this.Show(wp.DataDescriptions[index]);
                                    }
                                }
                                _timer.Enabled = true;
                            }
                        break;
                    case Action.ActionType.ACTION_NEXT_ITEM:
                    case Action.ActionType.ACTION_NEXT_CHANNEL:
                    case Action.ActionType.ACTION_PAGE_UP:
                        if (wp != null)
                        {
                            if ((g_Player.Playing | g_Player.Paused) & g_Player.FullScreen & g_Player.HasVideo & (g_Player.Player.GetType() == typeof(MediaPortal.Player.AudioPlayerWMP9)))
                            {
                                wp.PlayNext(1,false);
                                wp.GetChannelData(false);
                                _timer.Stop();
                                this.Show(wp.DataDescriptions[index]);
                                _playtimer.Enabled = false;
                                _playtimer.Enabled = true;
                                _playtimer.Start();
                                _timer.Interval = interval;
                                _timer.Enabled = true;
                                _timer.Start();
                            }
                        }
                        break;
                    case Action.ActionType.ACTION_PREV_CHANNEL:
                    case Action.ActionType.ACTION_PREV_ITEM:
                    case Action.ActionType.ACTION_PAGE_DOWN:
                        if (wp != null)
                        {
                            if ((g_Player.Playing | g_Player.Paused) & g_Player.FullScreen & g_Player.HasVideo & (g_Player.Player.GetType() == typeof(MediaPortal.Player.AudioPlayerWMP9)))
                            {
                                wp.PlayNext(-1,false);
                                wp.GetChannelData(false);
                                _timer.Stop();
                                this.Show(wp.DataDescriptions[index]);
                                _playtimer.Stop();
                                _playtimer.Enabled = true;
                                _playtimer.Start();
                                _timer.Interval = interval;
                                _timer.Enabled = true;
                                _timer.Start();
                            }
                        }
                        break;
                    case Action.ActionType.ACTION_SELECT_ITEM:
                        if ((g_Player.Playing | g_Player.Paused) & g_Player.FullScreen & g_Player.HasVideo & (g_Player.Player.GetType() == typeof(MediaPortal.Player.AudioPlayerWMP9)))
                        {
                            if (_changeOnOKonly) wp.PlayNext(0, false);
                            _timer.Stop();
                            this.Show(wp.DataDescriptions[index]);
                            _timer.Interval = interval;
                            _timer.Enabled = true;
                            _timer.Start();
                        }
                        break;
                    default:
                        break;
                }
            }
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

        protected virtual void _playtimer_Tick(object sender, EventArgs e)
        {
            //string dir = Directory.GetCurrentDirectory();
            //File.AppendAllText(dir + @"\webtelek.log", "timer has gone\n");
                _playtimer.Stop();
                wp.PlayNext(0, _changeOnOKonly);
        }

        private new void Show()
        {
        }

        public void Show(string info)
        {
            //TODO: Check with indefinite duration.
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                try
                {
                    g.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(0, _bitmap.Height), Color.White, Color.Black), 0, 0, _bitmap.Width, _bitmap.Height);
                    g.DrawImage(_top, 0, 0, _bitmap.Width, _bitmap.Height);
                    //TODO: Add colors, size and position to config !!!
                    //g.MeasureString(info, this.Font, new SizeF(_bitmap.Width,_bitmap.Height));
                    //g.DrawString(
                    Font f = new Font("Arial", 20);                    
                    g.DrawString(info, f, new SolidBrush(Color.Black), new RectangleF(42, 22, _bitmap.Width - 70 - 49, _bitmap.Height - 40));
                    g.DrawString(info, f, new SolidBrush(Color.White), new RectangleF(40, 20, _bitmap.Width - 70 - 49, _bitmap.Height - 40)); 
                    if (channel_id != null)
                    {
                        if (File.Exists(Config.GetFile(Config.Dir.Config, @"webtelek\", channel_id + ".jpg")))
                        {
                            Bitmap logo = new Bitmap(Config.GetFile(Config.Dir.Config, @"webtelek\", channel_id + ".jpg"));
                            g.DrawRectangle(new Pen(Color.Black, 2), _bitmap.Width - 70 - 40 + 17 + 2, 27 + 2, 49, 37);
                            g.DrawImage(logo, _bitmap.Width - 70 - 40 + 17 , 27, 49, 37);
                            g.DrawRectangle(new Pen(Color.Gray, 2), _bitmap.Width - 70 - 40 + 17, 27, 49, 37);
                        }
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