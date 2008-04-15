using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Utils;
using MediaPortal.TV.Database;
using MediaPortal.Configuration;

namespace MediaPortal.GUI.WebTelek
{
    public partial class ConfigurationForm : Form
    {
        class KeyValuePair
        {
            public KeyValuePair(string key, string value)
            {
                this.key = key;
                this.value = value;
            }

            private string key;
            private string value;
            public string Key
            {
                get { return key; }
                set { key = value; }
            }

            public string Value
            {
                get { return value; }
                set { this.value = value; }
            }

        }
        
        public ConfigurationForm()
        {
            InitializeComponent();
            string dir = Directory.GetCurrentDirectory();
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_profile.xml"), false))
            {
                string username = Convert.ToString(xmlreader.GetValueAsString("Account", "username", "your@email.com"));
                string password = Convert.ToString(xmlreader.GetValueAsString("Account", "password", "password"));
                string region = Convert.ToString(xmlreader.GetValueAsString("Account", "region", "MSK"));
                string timezone = Convert.ToString(xmlreader.GetValueAsString("Account", "timezone", "1"));
                string epgdays = Convert.ToString(xmlreader.GetValueAsString("Account", "epgdays", "1"));
                string epgnotify = Convert.ToString(xmlreader.GetValueAsString("Account", "epgnotify", "false"));
                string osddelay = Convert.ToString(xmlreader.GetValueAsString("Account", "osddelay", "5"));
                string netdelay = Convert.ToString(xmlreader.GetValueAsString("Account", "netdelay", "15"));
                string epgload = Convert.ToString(xmlreader.GetValueAsString("Account", "epgload", "true"));
                string versioncheck = Convert.ToString(xmlreader.GetValueAsString("Account", "versioncheck", "true"));
                string switchtimeout = Convert.ToString(xmlreader.GetValueAsString("Account", "switchtimeout", "1"));
                string switchonokonly = Convert.ToString(xmlreader.GetValueAsString("Account", "switchonokonly", "false"));

                textBox1.Text = username;
                textBox2.Text = password;

                EPGdays.Value = Decimal.Parse(epgdays);
                OSDDelay.Value = Decimal.Parse(osddelay);
                NetDelay.Value = Decimal.Parse(netdelay);
                EPGNotifyCheckBox.Checked = Boolean.Parse(epgnotify);
                EPGLoadCheckBox.Checked = Boolean.Parse(epgload);
                VersionCheckBox.Checked = Boolean.Parse(versioncheck);
                SwitchTimeout.Value = Decimal.Parse(switchtimeout);
                SwitchOnOKOnly.Checked = Boolean.Parse(switchonokonly);

                if (Boolean.Parse(switchonokonly)) SwitchTimeout.Enabled = false;

                ArrayList streamZones = new ArrayList();
                ArrayList timeZones = new ArrayList();

                streamZones.Add(new KeyValuePair("est", "EST - Нью-Йорк"));
                streamZones.Add(new KeyValuePair("pst", "PST - Лос Анжелес"));
                streamZones.Add(new KeyValuePair("msk", "MSK - Москва"));
                
                timeZones.Add(new KeyValuePair("-12", "Камчатка, GMT-12"));
                timeZones.Add(new KeyValuePair("-11", "Самоа, GMT-11"));
                timeZones.Add(new KeyValuePair("-10", "Гонолулу, GMT-10"));
                timeZones.Add(new KeyValuePair("-9", "Анкоридж, GMT-9"));
                timeZones.Add(new KeyValuePair("-8", "Лос-Анджелес, GMT-8"));
                timeZones.Add(new KeyValuePair("-7", "Денвер, GMT-7"));
                timeZones.Add(new KeyValuePair("-6", "Чикаго, GMT-6"));
                timeZones.Add(new KeyValuePair("-5", "Нью-Йорк, GMT-5"));
                timeZones.Add(new KeyValuePair("-4", "Каракас, GMT-4"));
                timeZones.Add(new KeyValuePair("-3", "Буенос Айрес, GMT-3"));
                timeZones.Add(new KeyValuePair("-2", "Mid-Atlantic, GMT-2"));
                timeZones.Add(new KeyValuePair("-1", "Капе Верде, GMT-1"));
                timeZones.Add(new KeyValuePair("0", "Лондон, GMT-0"));
                timeZones.Add(new KeyValuePair("1", "Париж, GMT+1"));
                timeZones.Add(new KeyValuePair("2", "Киев, GMT+2"));
                timeZones.Add(new KeyValuePair("3", "Москва, GMT+3"));
                timeZones.Add(new KeyValuePair("4", "Тбилиси, GMT+4"));
                timeZones.Add(new KeyValuePair("5", "Екатеринбург, GMT+5"));
                timeZones.Add(new KeyValuePair("6", "Новосибирск, GMT+6"));
                timeZones.Add(new KeyValuePair("7", "Банкок, GMT+7"));
                timeZones.Add(new KeyValuePair("8", "Пекин, GMT+8"));
                timeZones.Add(new KeyValuePair("9", "Токио, GMT+9"));
                timeZones.Add(new KeyValuePair("10", "Владивосток, GMT+10"));
                timeZones.Add(new KeyValuePair("11", "Магадан, GMT+11"));
                timeZones.Add(new KeyValuePair("12", "Фиджи, GMT+12"));

                comboBox1.DataSource = streamZones;
                comboBox1.DisplayMember = "Value";
                comboBox1.ValueMember = "Key";
                comboBox1.SelectedValue = region;

                comboBox2.DataSource = timeZones;
                comboBox2.DisplayMember = "Value";
                comboBox2.ValueMember = "Key";
                comboBox2.SelectedValue = timezone;


              
            }

        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            //Cancel Button
            this.Dispose(true);
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            //OK Button
            string dir = Directory.GetCurrentDirectory();
            //using (MediaPortal.Profile.Settings writer = new MediaPortal.Profile.Settings(dir +  @"\webtelek_profile.xml",false) )
            using (MediaPortal.Profile.Settings writer = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_profile.xml"),false) )
            {   
                writer.SetValue("Account", "username", textBox1.Text.Trim());                
                writer.SetValue("Account", "password", textBox2.Text.Trim());
                writer.SetValue("Account", "region", comboBox1.SelectedValue.ToString().Trim());
                writer.SetValue("Account", "timezone", comboBox2.SelectedValue.ToString().Trim());
                writer.SetValue("Account", "epgdays",  EPGdays.Value.ToString().Trim());
                writer.SetValue("Account", "epgnotify", EPGNotifyCheckBox.Checked.ToString().Trim());
                writer.SetValue("Account", "osddelay", OSDDelay.Value.ToString().Trim());
                writer.SetValue("Account", "netdelay", NetDelay.Value.ToString().Trim());
                writer.SetValue("Account", "epgload", EPGLoadCheckBox.Checked.ToString().Trim());
                writer.SetValue("Account", "versioncheck", VersionCheckBox.Checked.ToString().Trim());
                writer.SetValue("Account", "switchtimeout",  SwitchTimeout.Value.ToString().Trim());
                writer.SetValue("Account", "switchonokonly", SwitchOnOKOnly.Checked.ToString().Trim());
            }
            this.Dispose(true);
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ConfigurationForm_Load(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void CustomButton_Click(object sender, EventArgs e)
        {
            Form customchannel = new CustomChannel();
            customchannel.Show();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = "mailto:saulyak@gmail.com";
            process.Start();
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void EPGdays_ValueChanged(object sender, EventArgs e)
        {

        }

        private void EPGNotifyCheckBox_CheckedChanged_1(object sender, EventArgs e)
        {
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                if (xmlreader.GetValue("plugins", "TV Notifier") == "yes" && EPGNotifyCheckBox.Checked == true)
                {
                    MessageBox.Show("Стандартный \"TV Notifier\" активирован. Выключите его сначала и перезапустите \"Mediaportal Configuration\".");
                    EPGNotifyCheckBox.Checked = false;
                }
            }
        }

        private void OSDDelay_ValueChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void SwitchOnOKOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (SwitchOnOKOnly.Checked) SwitchTimeout.Enabled = false;
            else SwitchTimeout.Enabled = true;

        }

        private void btnCleanAll_Click(object sender, EventArgs e)
        {
            // delete tvguide.xml
            MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"), false);
            string dirname = Convert.ToString(xmlreader.GetValueAsString("xmltv", "folder", ""));
            File.Delete(dirname + @"\tvguide.xml");
            File.Delete(dirname + @"\epglastdate.dat");
            // delete tvguide databases
            TVDatabase.RemovePrograms();
            List<TVChannel> channels = new List<TVChannel>();
            TVDatabase.GetChannels(ref channels);
            foreach (TVChannel channel in channels)
            {
                TVDatabase.RemoveChannel(channel.Name);
            }
            
            // delete channel thumbs
            foreach (string file in Directory.GetFiles( Config.GetFolder(Config.Dir.Thumbs) + @"\tv\logos\","*.jpg") ) File.Delete(file);
            MessageBox.Show("Готово!");
        }

                   
    }
}