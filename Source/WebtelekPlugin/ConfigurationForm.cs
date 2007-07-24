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
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(dir + @"\webtelek_profile.xml", false))
            {
                string username = Convert.ToString(xmlreader.GetValueAsString("Account", "username", ""));
                string password = Convert.ToString(xmlreader.GetValueAsString("Account", "password", ""));
                string region = Convert.ToString(xmlreader.GetValueAsString("Account", "region", ""));
                string timezone = Convert.ToString(xmlreader.GetValueAsString("Account", "timezone", ""));
                textBox1.Text = username;
                textBox2.Text = password;

                ArrayList streamZones = new ArrayList();
                ArrayList timeZones = new ArrayList();

                streamZones.Add(new KeyValuePair("est", "EST - ���-����"));
                streamZones.Add(new KeyValuePair("pst", "PST - ��� �������"));
                streamZones.Add(new KeyValuePair("msk", "MSK - ������"));
                
                timeZones.Add(new KeyValuePair("-12", "��������, GMT-12"));
                timeZones.Add(new KeyValuePair("-11", "�����, GMT-11"));
                timeZones.Add(new KeyValuePair("-10", "��������, GMT-10"));
                timeZones.Add(new KeyValuePair("-9", "��������, GMT-9"));
                timeZones.Add(new KeyValuePair("-8", "���-��������, GMT-8"));
                timeZones.Add(new KeyValuePair("-7", "������, GMT-7"));
                timeZones.Add(new KeyValuePair("-6", "������, GMT-6"));
                timeZones.Add(new KeyValuePair("-5", "���-����, GMT-5"));
                timeZones.Add(new KeyValuePair("-4", "�������, GMT-4"));
                timeZones.Add(new KeyValuePair("-3", "������ �����, GMT-3"));
                timeZones.Add(new KeyValuePair("-2", "Mid-Atlantic, GMT-2"));
                timeZones.Add(new KeyValuePair("-1", "���� �����, GMT-1"));
                timeZones.Add(new KeyValuePair("0", "������, GMT-0"));
                timeZones.Add(new KeyValuePair("1", "�����, GMT+1"));
                timeZones.Add(new KeyValuePair("2", "����, GMT+2"));
                timeZones.Add(new KeyValuePair("3", "������, GMT+3"));
                timeZones.Add(new KeyValuePair("4", "�������, GMT+4"));
                timeZones.Add(new KeyValuePair("5", "������������, GMT+5"));
                timeZones.Add(new KeyValuePair("6", "�����������, GMT+6"));
                timeZones.Add(new KeyValuePair("7", "������, GMT+7"));
                timeZones.Add(new KeyValuePair("8", "�����, GMT+8"));
                timeZones.Add(new KeyValuePair("9", "�����, GMT+9"));
                timeZones.Add(new KeyValuePair("10", "�����������, GMT+10"));
                timeZones.Add(new KeyValuePair("11", "�������, GMT+11"));
                timeZones.Add(new KeyValuePair("12", "�����, GMT+12"));

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
            using (MediaPortal.Profile.Settings writer = new MediaPortal.Profile.Settings(dir +  @"\webtelek_profile.xml"))
            {   
                writer.SetValue("Account", "username", textBox1.Text.Trim());                
                writer.SetValue("Account", "password", textBox2.Text.Trim());
                writer.SetValue("Account", "region", comboBox1.SelectedValue.ToString().Trim());
                writer.SetValue("Account", "timezone", comboBox2.SelectedValue.ToString().Trim());
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

        
    }
}