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

namespace MediaPortal.GUI.WebTelek
{
    public partial class OSDProgressBar : Form
    {
        Bitmap _indicator;
        Bitmap _mid;
        Bitmap _background;
        Bitmap _top;
        Bitmap _bitmap;
        Timer _timer = new Timer();
        Form _parent;
        static bool _enabled;
        static OSDProgressBar _osd = null;
        LinearGradientBrush _brushBottom;
        LinearGradientBrush _brushTop;
        LinearGradientBrush _brushPosBottom;
        LinearGradientBrush _brushPosTop;
        g_Player.EndedHandler _gpeh;
        EventHandler _losc;
        OnActionHandler _ahandler;
        readonly PointF[] _pathPoints;
        int refreshCounter = 0;
        Action _action;

        public static void Start()
        {
            if (_osd == null)
            {
                _osd = new OSDProgressBar(Application.OpenForms[0]);
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

        public OSDProgressBar(Form parent)
        {                      
            //TODO: MediaPortal closes when WMP9OSD file is not found, why? 
            ConfigXmlDocument cxd = new ConfigXmlDocument();
            cxd.Load(GUIGraphicsContext.Skin + @"\WMP9OSD.xml");
            _top = new Bitmap(GUIGraphicsContext.Skin + @"\Media\" + cxd.GetElementsByTagName("TopBackground")[0].InnerText);
            _mid = new Bitmap(GUIGraphicsContext.Skin + @"\Media\" + cxd.GetElementsByTagName("ProgressMid")[0].InnerText);
            _indicator = new Bitmap(GUIGraphicsContext.Skin + @"\Media\" + cxd.GetElementsByTagName("ProgressIndicator")[0].InnerText);
            _background = new Bitmap(GUIGraphicsContext.Skin + @"\Media\" + cxd.GetElementsByTagName("ProgressBackground")[0].InnerText);
            _bitmap = new Bitmap(1246, 65);

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
            g_Player.PlayBackEnded += _gpeh;//TODO: Does not work as expected, add g_Player.PlayBackStopped  += _gpeh;?
            _ahandler = new OnActionHandler(GUIWindowManager_OnNewAction);
            GUIWindowManager.OnNewAction += _ahandler;
            _brushBottom = new LinearGradientBrush(new Rectangle(0, 0, 10, _bitmap.Height / 2), Color.FromArgb(7, 142, 2), Color.FromArgb(14, 180, 11), LinearGradientMode.Vertical);
            _brushTop = new LinearGradientBrush(new Rectangle(0, _bitmap.Height / 2 - 1, 1, _bitmap.Height), Color.FromArgb(128, 196, 128), Color.FromArgb(61, 164, 57), LinearGradientMode.Vertical);
            _brushPosBottom = new LinearGradientBrush(new Rectangle(0, 0, 10, _bitmap.Height / 2), Color.Yellow, Color.DarkOrange, LinearGradientMode.Vertical);
            _brushPosTop = new LinearGradientBrush(new Rectangle(0, _bitmap.Height / 2 - 1, 1, _bitmap.Height), Color.LightYellow, Color.Yellow, LinearGradientMode.Vertical);
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

        void drawOSD()
        {
            try
            {
                switch (_action.wID)
                {
                    //TODO: Is it adjustible????
                    case Action.ActionType.ACTION_BIG_STEP_FORWARD:
                    case Action.ActionType.ACTION_BIG_STEP_BACK:
                    case Action.ActionType.ACTION_STEP_BACK:
                    case Action.ActionType.ACTION_STEP_FORWARD:
                    case Action.ActionType.ACTION_MOVE_LEFT:
                    case Action.ActionType.ACTION_MOVE_RIGHT:
                    case Action.ActionType.ACTION_MOVE_UP:
                    case Action.ActionType.ACTION_MOVE_DOWN:
                        if (_enabled)
                            if ((g_Player.Playing | g_Player.Paused) & g_Player.FullScreen & g_Player.HasVideo & (g_Player.Player.GetType() == typeof(MediaPortal.Player.AudioPlayerWMP9)))
                            {
                                _timer.Enabled = false;
                                if (g_Player.FullScreen)
                                {
                                    double cs = 0; bool s, e;
                                    cs = g_Player.GetSeekStep(out s, out e) + g_Player.CurrentPosition;
                                    if (cs > g_Player.Duration) cs = g_Player.Duration;
                                    if (cs < 0) cs = 0;
                                    this.Show(g_Player.Duration, g_Player.CurrentPosition, cs, g_Player.GetStepDescription());
                                }
                                _timer.Interval = 1000;
                                _timer.Enabled = true;
                            }
                        break;
                    case Action.ActionType.ACTION_SHOW_OSD:
                    case Action.ActionType.ACTION_CONTEXT_MENU:
                    case Action.ActionType.ACTION_SELECT_ITEM:
                        if (_enabled)
                            if ((g_Player.Playing | g_Player.Paused) & g_Player.FullScreen & g_Player.HasVideo & (g_Player.Player.GetType() == typeof(MediaPortal.Player.AudioPlayerWMP9)))
                            {
                                _timer.Enabled = false;
                                if (g_Player.FullScreen)
                                {
                                    double cs = 0;
                                    this.Show(g_Player.Duration, g_Player.CurrentPosition, cs, DateTime.Now.ToString("HH:mm:ss"));
                                }
                                _timer.Interval = 1000;
                                _timer.Enabled = true;
                            }
                        break;
                    default:
                        break;
                }

            }
            catch (Exception) { }
        }

        void GUIWindowManager_OnNewAction(Action action)
        {
            //string dir = Directory.GetCurrentDirectory();
            //File.AppendAllText(dir + @"\webtelek.log", "OSD: " + action.wID.ToString() + " \n");

            switch (action.wID)
            {
                case Action.ActionType.ACTION_BIG_STEP_FORWARD:
                case Action.ActionType.ACTION_BIG_STEP_BACK:
                case Action.ActionType.ACTION_STEP_BACK:
                case Action.ActionType.ACTION_STEP_FORWARD:
                case Action.ActionType.ACTION_MOVE_LEFT:
                case Action.ActionType.ACTION_MOVE_RIGHT:
                case Action.ActionType.ACTION_MOVE_UP:
                case Action.ActionType.ACTION_MOVE_DOWN:
                    refreshCounter = 3;
                    _action = action;
                    drawOSD();
                    break;
                case Action.ActionType.ACTION_SHOW_OSD:
                case Action.ActionType.ACTION_CONTEXT_MENU:
                case Action.ActionType.ACTION_SELECT_ITEM:
                        refreshCounter = 4;
                        _action = action;
                        drawOSD();
                    break;
            }
        }

        void parent_LocationOrSizeChanged(object sender, EventArgs e)
        {
            double w = 0.96, h = 0.07;// , yp = 10;//TODO: Move to config file.
            this.Location = new Point((int)(_parent.Location.X + _parent.Size.Width * (1.0 - w) / 2), (int)(_parent.Location.Y + _parent.Height * 0.01));
            this.Size = new Size((int)(_parent.Size.Width * w), (int)(_parent.Size.Height * h));
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
            if (refreshCounter < 1)
            {
                _timer.Enabled = false;
                this.Hide();
            }
            else
            {
                refreshCounter--;
                drawOSD();
                _timer.Interval = 1000;
                _timer.Enabled = true;
            }
        }

        private new void Show()
        {
        }

        public void Show(double d, double p, double m, string info)
        {
            //TODO: Check with indefinite duration.
            if (d == 0.0) d = double.MaxValue;
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                try
                {
                    g.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(0, _bitmap.Height), Color.White, Color.Black), 0, 0, _bitmap.Width, _bitmap.Height);
                    g.DrawImage(_top, 0, 0, _bitmap.Width, _bitmap.Height);
                    int x = (int)(_bitmap.Width * 0.11);
                    int y = (int)(_bitmap.Height * 0.25);
                    int h = (int)(_bitmap.Height * 0.5);
                    int w = (int)(_bitmap.Width * 0.65);
                    int wm = (int)((m / d) * (_bitmap.Width * 0.65));
                    int wp = (int)((p / d) * (_bitmap.Width * 0.65));
                    int iw = (int)(_indicator.Width / (_indicator.Height / (_bitmap.Height * 0.5)));
                    g.DrawImage(_background, x, y, w, h);
                    g.DrawImage(_mid, x, y, wm, h);
                    g.DrawImage(_indicator, x + wp - iw / 2, y, iw, h);
                    //TODO: Add colors, size and position to config !!!
                    g.DrawString(new TimeSpan(0, 0, (int)p).ToString(), this.Font, new SolidBrush(Color.White), 15, y);
                    if (d < double.MaxValue)
                    {
                        g.DrawString(new TimeSpan(0, 0, (int)d).ToString(), this.Font, new SolidBrush(Color.White), 960, y);
                    }
                    else
                    {
                        g.DrawString("STREAM", this.Font, new SolidBrush(Color.White), 950, y);//TODO: Think about text;
                    }
                    g.DrawString(info, this.Font, new SolidBrush(Color.Black), 1100, y);
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