using FileMirror.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace FileMirror
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<Mirror> Mirrors = new List<Mirror>();
        int Index = -1;

        public void RefreshList(List<Mirror> mirrors, ListView listView)
        {
            listView.Items.Clear();
            foreach (Mirror mirror in mirrors)
            {
                listView.Items.Add(mirror.Name);
            }
        }

        public void ShowDetails(Mirror mirror)
        {
            textBoxName.Text = mirror.Name;
            textBoxFileNameSource.Text = mirror.Source;
            textBoxFileNameTarget.Text = mirror.Target;
        }

        public void ClearDetails()
        {
            textBoxName.Text = string.Empty;
            textBoxFileNameSource.Text = string.Empty;
            textBoxFileNameTarget.Text = string.Empty;

        }

        private void AddMirror(object sender, EventArgs e)
        {
            Mirror mirror = new Mirror();
            Mirrors.Add(mirror);
            RefreshList(Mirrors, listView1);
        }

        private void RemoveMirror(object sender, EventArgs e)
        {
            if (Index == -1)
                return;
            Mirrors.RemoveAt(Index);
            RefreshList(Mirrors, listView1);
        }

        private void RefreshMirror(object sender, EventArgs e)
        {
            if (Index == -1)
                return;
            Mirrors[Index].Refresh();
        }

        private void RefreshAllMirrors(object sender, EventArgs e)
        {
            foreach (Mirror mirror in Mirrors)
            {
                mirror.Refresh();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Index = -1;
            ClearDetails();
            if (listView1.SelectedIndices.Count > 0)
            {
                Index = listView1.SelectedIndices[0];
                ShowDetails(Mirrors[Index]);
            }
        }

        private void buttonBrowseSource_Click(object sender, EventArgs e)
        {
            if (Index == -1)
                return;
            if (folderBrowserDialogSource.ShowDialog() == DialogResult.OK)
            {
                Mirrors[Index].Source = folderBrowserDialogSource.SelectedPath;
                ShowDetails(Mirrors[Index]);
            }
        }

        private void buttonBrowseTarget_Click(object sender, EventArgs e)
        {
            if (Index == -1)
                return;
            if (folderBrowserDialogTarget.ShowDialog() == DialogResult.OK)
            {
                Mirrors[Index].Target = folderBrowserDialogTarget.SelectedPath;
                ShowDetails(Mirrors[Index]);
            }
        }

        private void textBoxName_Leave(object sender, EventArgs e)
        {
            if (Index == -1)
                return;
            Mirrors[Index].Name = textBoxName.Text;
            RefreshList(Mirrors, listView1);
        }

        private void textBoxFileNameSource_Leave(object sender, EventArgs e)
        {
            if (Index == -1)
                return;
            Mirrors[Index].Source = textBoxFileNameSource.Text;
        }

        private void textBoxFileNameTarget_Leave(object sender, EventArgs e)
        {
            if (Index == -1)
                return;
            Mirrors[Index].Target = textBoxFileNameTarget.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Settings.Default.Mirrors != string.Empty)
                Mirrors = JsonSerializer.Deserialize<List<Mirror>>(Settings.Default.Mirrors);
            RefreshList(Mirrors, listView1);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Mirrors = JsonSerializer.Serialize(Mirrors);
            Settings.Default.Save();
        }
    }

    public class Mirror
    {
        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public Mirror(string Name = "Unnamed", string Source = "", string Target = "", bool SubDirectories = true)
        {
            this.Name = Name;
            this.Source = Source;
            this.Target = Target;
            this.SubDirectories = SubDirectories;
        }
        public string Source { get; set; }
        public string Target { get; set; }
        public string Name { get; set; }
        public bool SubDirectories { get; set; }
        public Exception Refresh()
        {
            try
            {
                CopyDirectory(this.Source, this.Target, this.SubDirectories);
            }
            catch (Exception e)
            {
                return e;
            }
            return new Exception();
        }
    }

}
