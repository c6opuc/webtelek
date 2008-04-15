#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Reflection;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace MediaPortal.Player
{
  /// <summary>
  /// 
  /// </summary>
  public class WebTelekPlayerFactory : IPlayerFactory
  {
    static ArrayList _externalPlayerList = new ArrayList();
    static bool _externalPlayersLoaded = false;

    public WebTelekPlayerFactory()
    {
    }

    public enum StreamingPlayers : int
    {
      BASS = 0,
      WMP9 = 1,
      VMR7 = 2,
      RTSP = 3,
    }
    private bool CheckMpgFile(string fileName)
    {
      try
      {
        if (!System.IO.File.Exists(fileName)) return false;
        using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
          using (BinaryReader reader = new BinaryReader(stream))
          {
            stream.Seek(0, SeekOrigin.Begin);
            byte[] header = reader.ReadBytes(5);
            if (header[0] != 0 || header[1] != 0 || header[2] != 1 || header[3] != 0xba) return false;
            if ((header[4] & 0x40) == 0) return false;
            stream.Seek(0x800, SeekOrigin.Begin); header = reader.ReadBytes(5);
            if (header[0] != 0 || header[1] != 0 || header[2] != 1 || header[3] != 0xba) return false;
            if ((header[4] & 0x40) == 0) return false;
            stream.Seek(0x8000, SeekOrigin.Begin); header = reader.ReadBytes(5);
            if (header[0] != 0 || header[1] != 0 || header[2] != 1 || header[3] != 0xba) return false;
            if ((header[4] & 0x40) == 0) return false;
            return true;
          }
        }
      }
      catch (Exception e)
      {
        // If an IOException is raised, the file may be in use/being recorded so we assume that it is a correct mpeg file
        // This fixes replaying mpeg files while being recorded
        if (e.GetType().ToString() == "System.IO.IOException")
          return true;
        Log.Info("Exception in CheckMpgFile with message: {0}", e.Message);
      }
      return false;
    }
    private void LoadExternalPlayers()
    {
      Log.Info("Loading external players plugins");
      string[] fileList = System.IO.Directory.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "ExternalPlayers"), "*.dll");
      foreach (string fileName in fileList)
      {
        try
        {
          Assembly assem = Assembly.LoadFrom(fileName);
          if (assem != null)
          {
            Type[] types = assem.GetExportedTypes();
            foreach (Type t in types)
            {
              try
              {
                if (t.IsClass)
                {
                  if (t.IsSubclassOf(typeof(IExternalPlayer)))
                  {
                    object newObj = (object)Activator.CreateInstance(t);
                    Log.Info("  found plugin:{0} in {1}", t.ToString(), fileName);

                    IExternalPlayer player = (IExternalPlayer)newObj;
                    Log.Info("  player:{0}.  author: {1}", player.PlayerName, player.AuthorName);
                    _externalPlayerList.Add(player);
                  }
                }
              }
              catch (Exception e)
              {
                Log.Info("Error loading external player: {0}", t.ToString());
                Log.Info("Error: {0}", e.StackTrace);
              }
            }
          }
        }
        catch (Exception e)
        {
          Log.Info("Error loading external player: {0}", e);
        }
      }
      _externalPlayersLoaded = true;
    }

    public IExternalPlayer GetExternalPlayer(string fileName)
    {
      if (!_externalPlayersLoaded)
      {
        LoadExternalPlayers();
      }

      foreach (IExternalPlayer player in _externalPlayerList)
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          bool enabled = xmlreader.GetValueAsBool("plugins", player.PlayerName, false);
          player.Enabled = enabled;
        }

        if (player.Enabled && player.SupportsFile(fileName))
        {
          return player;
        }
      }
      return null;
    }

    public IPlayer Create(string fileName)
    {
      string strAudioPlayer = string.Empty;
      int streamPlayer = 0;

      // Free BASS to avoid problems with Digital Audio, when watching movies
      if (BassMusicPlayer.IsDefaultMusicPlayer)
      {
        if (!MediaPortal.Util.Utils.IsAudio(fileName))
          BassMusicPlayer.Player.FreeBass();
      }

      IPlayer newPlayer = null;
      if (fileName.ToLower().IndexOf("rtsp:") >= 0)
      {
        // return new TSReaderPlayer();
      }
      if (fileName.StartsWith("mms:") && fileName.EndsWith(".ymvp"))
      {
        bool useVMR9;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          useVMR9 = xmlreader.GetValueAsBool("musicvideo", "useVMR9", true);
        }
        if (useVMR9)
        {
          return new VideoPlayerVMR9();
        }
        else
        {
          return new WebTelekWMP();
        }
      }
      string extension = System.IO.Path.GetExtension(fileName).ToLower();
      if (extension != ".tv" && extension != ".sbe" && extension != ".dvr-ms"
              && fileName.ToLower().IndexOf(".tsbuffer") < 0
              && fileName.ToLower().IndexOf("radio.tsbuffer") < 0)
      {
        newPlayer = GetExternalPlayer(fileName);
        if (newPlayer != null)
        {
          Log.Info("PlayerFactory: Disabling DX9 exclusive mode");
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(msg);
          return newPlayer;
        }
      }

      if (MediaPortal.Util.Utils.IsVideo(fileName))
      {
        if (extension == ".tv" || extension == ".sbe" || extension == ".dvr-ms")
        {
          if (extension == ".sbe" || extension == ".dvr-ms")
          {
            //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
            //GUIWindowManager.SendMessage(msg);
          }

          newPlayer = new Player.StreamBufferPlayer9();
          return newPlayer;
        }
      }

      // Use TsReader for timeshift buffer file for TvEngine3 & .ts recordings etc.
      if (extension == ".tsbuffer" || extension == ".ts")
      {
        if (fileName.ToLower().IndexOf("radio.tsbuffer") >= 0)
        {
          //return new Player.BaseTSReaderPlayer();
        }
        //return new Player.TSReaderPlayer();
      }

      if (!MediaPortal.Util.Utils.IsAVStream(fileName) && MediaPortal.Util.Utils.IsVideo(fileName))
      {
        newPlayer = new Player.VideoPlayerVMR9();
        return newPlayer;
      }

      if (extension == ".radio")
      {
        newPlayer = new Player.RadioTuner();
        return newPlayer;
      }


      if (MediaPortal.Util.Utils.IsCDDA(fileName))
      {
        // Check if, we should use BASS for CD Playback
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "Internal dshow player");
          if (String.Compare(strAudioPlayer, "BASS engine", true) == 0)
          {
            if (BassMusicPlayer.BassFreed)
              BassMusicPlayer.Player.InitBass();

            return BassMusicPlayer.Player;
          }
        }
        newPlayer = new Player.WebTelekWMP();
        return newPlayer;
      }


      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "Internal dshow player");
        streamPlayer = xmlreader.GetValueAsInt("audioscrobbler", "streamplayertype", 0);

      }

      if (MediaPortal.Util.Utils.IsAudio(fileName))
      {
        // choose player for Internet radio streams 
        if (Util.Utils.IsLastFMStream(fileName))
        {
          switch (streamPlayer)
          {
            case 0:
              if (BassMusicPlayer.BassFreed)
                BassMusicPlayer.Player.InitBass();
              return BassMusicPlayer.Player;
            case 1:
              return new Player.WebTelekWMP();
            case 2:
              return new Player.AudioPlayerVMR7();
            case 3:
              return new RTSPPlayer();
            default:
              if (BassMusicPlayer.BassFreed)
                BassMusicPlayer.Player.InitBass();
              return BassMusicPlayer.Player;
          }
        }

        if (String.Compare(strAudioPlayer, "BASS engine", true) == 0)
        {
          if (BassMusicPlayer.BassFreed)
            BassMusicPlayer.Player.InitBass();

          return BassMusicPlayer.Player;
        }

        else if (String.Compare(strAudioPlayer, "Windows Media Player 9", true) == 0)
        {
          newPlayer = new Player.WebTelekWMP();
          return newPlayer;
        }
        newPlayer = new Player.AudioPlayerVMR7();
        return newPlayer;
      }

      // Use WMP Player as Default
      newPlayer = new Player.WebTelekWMP();
      return newPlayer;

    }

    public IPlayer Create(string fileName, g_Player.MediaType type)
    {
      // Free BASS to avoid problems with Digital Audio, when watching movies
      if (!MediaPortal.Util.Utils.IsAudio(fileName))
        BassMusicPlayer.Player.FreeBass();

      IPlayer newPlayer = null;
      if (fileName.ToLower().IndexOf("rtsp:") >= 0)
      {
        // return new TSReaderPlayer(type);
      }
      string extension = System.IO.Path.GetExtension(fileName).ToLower();
      if (extension != ".tv" && extension != ".sbe" && extension != ".dvr-ms"
              && fileName.ToLower().IndexOf(".tsbuffer") < 0
              && fileName.ToLower().IndexOf("radio.tsbuffer") < 0)
      {
        newPlayer = GetExternalPlayer(fileName);
        if (newPlayer != null)
        {
          Log.Info("PlayerFactory: Disabling DX9 exclusive mode");
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(msg);
          return newPlayer;
        }
      }

      if (MediaPortal.Util.Utils.IsVideo(fileName))
      {
        if (extension == ".tv" || extension == ".sbe" || extension == ".dvr-ms")
        {
          if (extension == ".sbe" || extension == ".dvr-ms")
          {
            //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
            //GUIWindowManager.SendMessage(msg);
          }

          newPlayer = new Player.StreamBufferPlayer9();
          return newPlayer;
        }
      }

      // Use TsReader for timeshift buffer file for TvEngine3 & .ts recordings etc.
      if (extension == ".tsbuffer" || extension == ".ts")
      {
        if (fileName.ToLower().IndexOf("radio.tsbuffer") >= 0)
        {
          //return new Player.BaseTSReaderPlayer(type);
        }
        //return new Player.TSReaderPlayer(type);
      }

      if (!MediaPortal.Util.Utils.IsAVStream(fileName) && MediaPortal.Util.Utils.IsVideo(fileName))
      {
        newPlayer = new Player.VideoPlayerVMR9(type);
        return newPlayer;
      }

      if (extension == ".radio")
      {
        newPlayer = new Player.RadioTuner();
        return newPlayer;
      }

      if (MediaPortal.Util.Utils.IsCDDA(fileName))
      {
        newPlayer = new Player.WebTelekWMP();

        return newPlayer;
      }

      if (MediaPortal.Util.Utils.IsAudio(fileName))
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "Internal dshow player");
          int streamPlayer = xmlreader.GetValueAsInt("audioscrobbler", "streamplayertype", 0);

          // choose player for Internet radio streams 
          if (Util.Utils.IsLastFMStream(fileName))
          {
            switch (streamPlayer)
            {
              case 0:
                if (BassMusicPlayer.BassFreed)
                  BassMusicPlayer.Player.InitBass();
                return BassMusicPlayer.Player;
              case 1:
                return new Player.WebTelekWMP();
              case 2:
                return new Player.AudioPlayerVMR7();
              case 3:
                return new RTSPPlayer();
              default:
                if (BassMusicPlayer.BassFreed)
                  BassMusicPlayer.Player.InitBass();
                return BassMusicPlayer.Player;
            }
          }

          if (String.Compare(strAudioPlayer, "BASS engine", true) == 0)
          {
            if (BassMusicPlayer.BassFreed)
              BassMusicPlayer.Player.InitBass();

            return BassMusicPlayer.Player;
          }
          else if (String.Compare(strAudioPlayer, "Windows Media Player 9", true) == 0)
          {
            newPlayer = new Player.WebTelekWMP();
            return newPlayer;
          }
          newPlayer = new Player.AudioPlayerVMR7();
          return newPlayer;
        }
      }

      newPlayer = new Player.WebTelekWMP();
      return newPlayer;
    }
  }
}