using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using MediaPortal.Configuration;

namespace MediaPortal.GUI.WebTelek
{
    public partial class CustomChannel : Form
    {
/*       class KeyValuePair
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

 */
        public CustomChannel()
        {
            InitializeComponent();
        }

        private bool editBtnPressed = false;
        private bool newBtnPressed = false;

        private void CustomChannel_Load(object sender, EventArgs e)
        {
            //string dir = Directory.GetCurrentDirectory();
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(dir + @"\webtelek_custom.xml", true))
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "webtelek_custom.xml")))
            {
                for (int i = 0; i <= 1500; i++)
                {
                    if (Convert.ToString(xmlreader.GetValueAsString(i.ToString(), "name", "")) != "")
                    {
                        ListViewItem item = new ListViewItem(new string[] {
                        Convert.ToString(xmlreader.GetValueAsString(i.ToString(), "name", "")),
                        Convert.ToString(xmlreader.GetValueAsString(i.ToString(), "url", "")),
                        Convert.ToString(xmlreader.GetValueAsString(i.ToString(), "country", "")),
                        Convert.ToString(xmlreader.GetValueAsString(i.ToString(), "category", "")),
                        Convert.ToString(xmlreader.GetValueAsString(i.ToString(), "description", ""))
                        });
                        ChannelsView.Items.Insert(ChannelsView.Items.Count, item);
                    }
                }
                DisplayAll();
            }
        }

        private void DisplayAll()
        {
            try
            {
                textName.ReadOnly = true;
                textURL.ReadOnly = true;
                textCountry.ReadOnly = true;
                textCategory.ReadOnly = true;
                textDescription.ReadOnly = true;
                textName.Text = ChannelsView.SelectedItems[0].SubItems[0].Text;
                textURL.Text = ChannelsView.SelectedItems[0].SubItems[1].Text;
                textCountry.Text = ChannelsView.SelectedItems[0].SubItems[2].Text;
                textCategory.Text = ChannelsView.SelectedItems[0].SubItems[3].Text;
                textDescription.Text = ChannelsView.SelectedItems[0].SubItems[4].Text;
            }
            catch (Exception)
            {
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void bntOk_Click(object sender, EventArgs e)
        {
            SaveAll();
            this.Dispose();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            SaveAll();
            textName.Text = "";
            textURL.Text = "";
            textCountry.Text = "";
            textCategory.Text = "";
            textDescription.Text = "";
            textName.ReadOnly = false;
            textURL.ReadOnly = false;
            textCountry.ReadOnly = false;
            textCategory.ReadOnly = false;
            textDescription.ReadOnly = false;
            newBtnPressed = true;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            textName.ReadOnly = false;
            textURL.ReadOnly = false;
            textCountry.ReadOnly = false;
            textCategory.ReadOnly = false;
            textDescription.ReadOnly = false;
            editBtnPressed = true;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (textName.Text.Trim() == "" ||
                textURL.Text.Trim() == "" ||
                textCountry.Text.Trim() == "" ||
                textCategory.Text.Trim() == "" ||
                textDescription.Text.Trim() == "")
            {
                MessageBox.Show("Все поля должны быть заполнены");
                return;
            }
            if (editBtnPressed)
            {
                editBtnPressed = false;
                ChannelsView.SelectedItems[0].SubItems[0].Text = textName.Text.Trim();
                ChannelsView.SelectedItems[0].SubItems[1].Text = textURL.Text.Trim();
                ChannelsView.SelectedItems[0].SubItems[2].Text = textCountry.Text.Trim();
                ChannelsView.SelectedItems[0].SubItems[3].Text = textCategory.Text.Trim();
                ChannelsView.SelectedItems[0].SubItems[4].Text = textDescription.Text.Trim();
            }
            if (newBtnPressed)
            {
                newBtnPressed = false;
                ListViewItem item = new ListViewItem(new string[] {
                        textName.Text.Trim(),
                        textURL.Text.Trim(),
                        textCountry.Text.Trim(),
                        textCategory.Text.Trim(),
                        textDescription.Text.Trim()
                        });
                ChannelsView.Items.Insert(ChannelsView.Items.Count, item);
            }
            DisplayAll();
            SaveAll();

        }

        private void SaveAll()
        {
            //string dir = Directory.GetCurrentDirectory();
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            File.Delete(Config.GetFile(Config.Dir.Config, "webtelek_custom.xml"));
//            XmlTextWriter writer = new XmlTextWriter(dir + @"\webtelek_custom.xml", null);
            XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "webtelek_custom.xml"), null); 
            writer.WriteStartDocument();
            writer.Formatting = Formatting.Indented;
            writer.WriteStartElement("profile");

                for (int i = 0; i < ChannelsView.Items.Count; i++)
                {
                    writer.WriteStartElement("section");
                    writer.WriteAttributeString("name", i.ToString());

                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("name", "name");
                    writer.WriteString(ChannelsView.Items[i].SubItems[0].Text.Trim());
                    writer.WriteEndElement();

                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("name", "url");
                    writer.WriteString(ChannelsView.Items[i].SubItems[1].Text.Trim());
                    writer.WriteEndElement();

                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("name", "country");
                    writer.WriteString(ChannelsView.Items[i].SubItems[2].Text.Trim());
                    writer.WriteEndElement();

                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("name", "category");
                    writer.WriteString(ChannelsView.Items[i].SubItems[3].Text.Trim());
                    writer.WriteEndElement();

                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("name", "description");
                    writer.WriteString(ChannelsView.Items[i].SubItems[4].Text.Trim());
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        private void btnDel_Click(object sender, EventArgs e)
        {
            textName.Text = "";
            textURL.Text = "";
            textCountry.Text = "";
            textCategory.Text = "";
            textDescription.Text = "";
            try
            {
                ChannelsView.Items.Remove(ChannelsView.SelectedItems[0]);
            }
            catch (Exception)
            {
            }

        }

        private void ChannelsView_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayAll();
        }
    }
}