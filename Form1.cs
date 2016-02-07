using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PictureRenamer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        

        private void Form1_Load_1(object sender, EventArgs e)
        {
            if (Directory.Exists(@"D:\foto"))
            {
                openFileDialog.InitialDirectory = @"D:\foto";
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                LoadImages(openFileDialog.FileNames);
                outputDirectoryTextBox.Text = Path.GetDirectoryName(openFileDialog.FileName);
                outputFolderBrowserDialog.SelectedPath = Path.GetDirectoryName(openFileDialog.FileName);
            }
        }

        private void LoadImages(string[] files)
        {
            toolStripStatusLabel.Text = string.Format("Loading {0} images", toolStripProgressBar.Maximum);
            listView1.BeginUpdate();
            listView1.Items.Clear();
            var imagesCollection = this.images.Images;
            foreach (Image img in imagesCollection)
            {
                img.Dispose();
            }

            imagesCollection.Clear();
            var i = 0;
            
            StartBatch(files.Count());
            
            foreach (var file in files)
            {
                using (var img = Image.FromFile(file))
                {
                    var sizedImage = new Bitmap(img, 256, 256);
                    imagesCollection.Add(sizedImage);
                    listView1.Items.Add(new ListViewItem(new[] { i.ToString(), file }, i++));

                    toolStripProgressBar.Value++;
                }
            }

            foreach (ListViewItem item in listView1.Items)
            {
                item.Text = string.Format("[{0}] {1}", item.Index+1, item.SubItems[1].Text);
            }
            
            listView1.EndUpdate();

            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = listView1.Items.Count;

            BatchFinished();
            toolStripStatusLabel.Text = string.Format("{0} images", toolStripProgressBar.Maximum);
        }

        private void StartBatch(int filesCount)
        {
            toolStripProgressBar.Visible = true;
            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Maximum = filesCount;
            toolStripStatusLabel.Visible = true;
            splitContainer1.Panel1.Enabled = false;
        }

        private void BatchFinished()
        {
            splitContainer1.Panel1.Enabled = true;
            toolStripProgressBar.Visible = false;
            
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            
            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];
                if (item.Index > 0)
                {
                    MoveItem(item, item.Index - 1);
                }
            }
        }

        private void MoveItem(ListViewItem item, int newIndex)
        {
            listView1.View = View.List;
            var items = listView1.Items;
            var movedItem = items[newIndex];
            items.Remove(item);
            items.Insert(newIndex, item);
            listView1.View = View.LargeIcon;

            UpdateItemText(item);
            UpdateItemText(movedItem);

            listView1.EnsureVisible(newIndex);
        }

        private void UpdateItemText(ListViewItem item)
        {
            item.Text = string.Format("[{0}] {1}", item.Index + 1, item.SubItems[1].Text);
        }


        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];
                if (item.Index < listView1.Items.Count)
                {
                    MoveItem(item, item.Index + 1);
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                MoveUpButton.Enabled = listView1.SelectedItems[0].Index > 0;
                MoveDownButton.Enabled = listView1.SelectedItems[0].Index < listView1.Items.Count-1;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];
                MoveItem(item, (int)numericUpDown1.Value - 1);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var result = outputFolderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                outputDirectoryTextBox.Text = outputFolderBrowserDialog.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var outputDirectory = outputDirectoryTextBox.Text;

            if (!Directory.Exists(outputDirectory))
            {
                MessageBox.Show("Output directory does not exist! Please pick an existing output directory", "Output directory not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var fileName = fileNameTextBox.Text;
            if (string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("File name not set! Please specify new file name", "File name not set", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StartBatch(listView1.Items.Count);
            toolStripStatusLabel.Text = string.Format("Renaming {0} images", toolStripProgressBar.Maximum);
            var fileNumber = 1;
            foreach (ListViewItem file in listView1.Items)
            {
                var extension = Path.GetExtension(file.SubItems[1].Text);
                File.Copy(file.SubItems[1].Text, Path.Combine(outputDirectory, string.Format("{0}_{1}{2}", fileName, fileNumber, extension)));
                fileNumber++;
                toolStripProgressBar.Value++;
            }
            Process.Start(outputDirectory);
            BatchFinished();
        }

        private void progressStatusBar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripStatusLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
