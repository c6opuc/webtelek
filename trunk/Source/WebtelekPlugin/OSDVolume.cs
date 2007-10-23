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
    public partial class OSDVolume : Form
    {
        Bitmap _bitmap;
        Timer _timer = new Timer();
        Form _parent;
        static bool _enabled;
        static OSDVolume _osd = null;
        g_Player.EndedHandler _gpeh;
        EventHandler _losc;
        OnActionHandler _ahandler;

        // Bar size
        int vheight = 25;
        int vwidth = 10;
        int vseparator = 2; 
        int vsize = 10; // number of elements

        public static void Start()
        {
            if (_osd == null)
            {
                _osd = new OSDVolume(Application.OpenForms[0]);
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

        public OSDVolume(Form parent)
        {                      

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
            _bitmap = new Bitmap((vwidth+vseparator)*vsize-vseparator,vheight);
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
                switch (action.wID)
                {
                    case Action.ActionType.ACTION_VOLUME_UP:
                    case Action.ActionType.ACTION_VOLUME_DOWN:
                    case Action.ActionType.ACTION_VOLUME_MUTE:
                        if (_enabled)
                            if ((g_Player.Playing | g_Player.Paused) & g_Player.FullScreen & g_Player.HasVideo & (g_Player.Player.GetType() == typeof(MediaPortal.Player.AudioPlayerWMP9)))
                            {
                                _timer.Enabled = false;
                                if (g_Player.FullScreen)
                                {
                                    this.Show(action);
                                }
                                _timer.Interval = 3000;
                                _timer.Enabled = true;
                            }
                        break;
                    default:
                        break;
                }             
        }

        void parent_LocationOrSizeChanged(object sender, EventArgs e)
        {
            this.Location = new Point((int)(_parent.Location.X + _parent.Width - (vwidth + vseparator) * vsize - _parent.Width*0.01), (int)(_parent.Location.Y + vheight + _parent.Height * 0.05));
            this.Size = new Size((vwidth + vseparator)*vsize-vseparator,vheight);
            this.BackColor = Color.Gray;
        }

        protected virtual void _timer_Tick(object sender, EventArgs e)
        {
            this.Hide();
        }

        private new void Show()
        {
        }

        public void Show(Action action)
        {
            int max = VolumeHandler.Instance.Maximum;
            int min = VolumeHandler.Instance.Minimum;
            int currentVolume = VolumeHandler.Instance.Volume;
            int volume = 0;

            if (currentVolume > 0)
            {
                volume = (int)(currentVolume * vsize / max) + 1;
            }

            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                for (int i = 0; i < vsize; i++)
                {
                        try
                        {
                            if (i < volume)
                            {
                                g.FillRectangle(new SolidBrush(System.Drawing.Color.White), new Rectangle((vwidth + vseparator) * i, 0, (vwidth + vseparator) * i + vwidth, vheight));
                            }
                            else
                            {
                                g.FillRectangle(new SolidBrush(System.Drawing.Color.Black), new Rectangle((vwidth + vseparator) * i, 0, (vwidth + vseparator) * i + vwidth, vheight));
                            }
                            g.FillRectangle(new SolidBrush(System.Drawing.Color.Gray), new Rectangle((vwidth + vseparator) * i + vwidth, 0, (vwidth + vseparator) * i + vwidth + vseparator, vheight));
                            if (VolumeHandler.Instance.IsMuted) g.DrawLine(new Pen(Color.Red,4), new Point(0, vheight/2), new Point((vwidth + vseparator) * vsize - vseparator, vheight/2)); 
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
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
                    e.Graphics.DrawImage(_bitmap, new Point(0,0));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}