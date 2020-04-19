﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretNest.ImageStore.SimilarFile
{
    internal partial class SimilarFileCheck : UserControl
    {
        public bool AutoResizePictures
        {
            get => doublePictureBox1.AutoResizePictures;
            set => doublePictureBox1.AutoResizePictures = value;
        }

        public bool AutoMoveNext { get; set; }

        public SimilarFileCheck()
        {
            InitializeComponent();
        }

        HashSet<Guid> selectedFiles;
        Dictionary<Guid, FileInfo> allFileInfo;
        FileInfo mainFile;

        class SimilarRecord
        {
            public Guid SimilarRecordId { get; set; }
            public Guid FileId { get; set; }
            public bool IsFile1IsMain { get; set; }
            public float DifferenceDegree { get; set; }
            public string FileNameWithExtension { get; set; }
            public string PathToDirectory { get; set; }
            public int FileSize { get; set; }
            public int IgnoredModeCode { get; set; }
            public IgnoredMode IgnoredMode
            {
                get => (IgnoredMode)IgnoredModeCode;
                set => IgnoredModeCode = (int)value;
            }
            public FileInfo FileInfo { get; set; }
        }
        SimilarRecord[] similarRecords;
        Func<Guid, IgnoredMode, bool> markIgnoreCallback;

        public void Initialize(HashSet<Guid> selectedFiles, Dictionary<Guid, FileInfo> allFileInfo, Func<Guid, IgnoredMode, bool> markIgnoreCallback)
        {
            this.selectedFiles = selectedFiles;
            this.allFileInfo = allFileInfo;
            this.markIgnoreCallback = markIgnoreCallback;
        }

        public void LoadFile(Guid mainFileId, IEnumerable<ImageStoreSimilarFile> similarRecords)
        {
            mainFile = allFileInfo[mainFileId];
            if (similarRecords == null)
                this.similarRecords = null;
            else
            {
                this.similarRecords = similarRecords.Select(similarFileRecord =>
                {
                    var isFile1IsMain = similarFileRecord.File1Id == mainFile.FileId;
                    FileInfo file;
                    if (isFile1IsMain)
                        file = allFileInfo[similarFileRecord.File2Id];
                    else
                        file = allFileInfo[similarFileRecord.File1Id];
                    return new SimilarRecord
                    {
                        SimilarRecordId = similarFileRecord.Id,
                        FileId = file.FileId,
                        IsFile1IsMain = isFile1IsMain,
                        DifferenceDegree = similarFileRecord.DifferenceDegree,
                        FileNameWithExtension = file.FileNameWithExtension,
                        PathToDirectory = file.PathToDirectory,
                        FileSize = file.FileSize,
                        IgnoredModeCode = similarFileRecord.IgnoredModeCode,
                        FileInfo = file
                    };
                }).ToArray();
            }

            LoadSimilarRecords();
        }

        public void ClearFile()
        {
            mainFile = null;
            similarRecords = null;

            LoadSimilarRecords();
        }

        public event EventHandler<RequestingGotoFileEventArgs> RequestingGotoFile;

        bool sortByRate = true;

        #region ListView Event
        bool fileListChanging = false;

        void LoadSimilarRecords()
        {
            fileListChanging = true;
            listView3.Items.Clear();

            if (similarRecords != null && similarRecords.Length > 0)
            {
                checkBox1.Text = mainFile.FileNameWithExtension + " (" + mainFile.PathToDirectory + ")";
                checkBox1.Checked = selectedFiles.Contains(mainFile.FileId);

                List<ListViewItem> items = new List<ListViewItem>();

                IEnumerable<SimilarRecord> sorted;
                if (sortByRate)
                {
                    sorted = similarRecords.OrderBy(i => i.IgnoredModeCode).ThenBy(i => i.DifferenceDegree);
                }
                else
                {
                    sorted = similarRecords.OrderBy(i => i.PathToDirectory).ThenBy(i => i.FileNameWithExtension);
                }

                foreach (var similarFileRecord in sorted)
                {
                    var selected = selectedFiles.Contains(similarFileRecord.FileId);
                    items.Add(new ListViewItem(new string[] {
                                similarFileRecord.DifferenceDegree.ToString("0.0000"),
                                similarFileRecord.FileNameWithExtension, similarFileRecord.PathToDirectory, similarFileRecord.FileSize.ToString(),
                                SimilarFileHelper.IgnoredModeToString(similarFileRecord.IgnoredMode)
                            })
                    {
                        Checked = selected,
                        Tag = similarFileRecord
                    });
                }

                listView3.Items.AddRange(items.OrderBy(i => i.SubItems[2].Text).ThenBy(i => i.SubItems[0].Text).ThenBy(i => i.SubItems[1].Text).ToArray());
                listView3.Items[0].Selected = true;
            }
            else
            {
                checkBox1.Text = "";
                checkBox1.Checked = false;
            }

            fileListChanging = false;
        }

        private void listView3_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (fileListChanging) return;
            if (listView3MouseDown)
            {
                e.NewValue = e.CurrentValue;
            }
        }

        private void listView3_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (fileListChanging) return;
            var file2Id = ((SimilarRecord)e.Item.Tag).FileId;
            bool check = e.Item.Checked;
            if (check)
            {
                selectedFiles.Add(file2Id);
            }
            else
            {
                selectedFiles.Remove(file2Id);
            }
        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView3.Items)
            {
                if (!item.Selected) continue;

                SimilarRecord tag = (SimilarRecord)item.Tag;

                if (tag.IsFile1IsMain)
                    doublePictureBox1.LoadPictures(mainFile, tag.FileInfo);
                else
                    doublePictureBox1.LoadPictures(tag.FileInfo, mainFile);

                button16.Enabled = true;
                button14.Enabled = true;
                button12.Enabled = true;
                button11.Enabled = true;
                button10.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;

                return;
            }

            button16.Enabled = false;
            button14.Enabled = false;
            button12.Enabled = false;
            button11.Enabled = false;
            button10.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;

            doublePictureBox1.ClearPictures();
        }

        bool listView3MouseDown = false;
        private void listView3_MouseLeave(object sender, EventArgs e)
        {
            listView3MouseDown = false;
        }

        private void listView3_MouseUp(object sender, MouseEventArgs e)
        {
            listView3MouseDown = false;
        }

        private void listView3_MouseDown(object sender, MouseEventArgs e)
        {
            listView3MouseDown = true;
        }
        #endregion

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (fileListChanging) return;
            bool check = checkBox1.Checked;
            if (check)
            {
                selectedFiles.Add(mainFile.FileId);
            }
            else
            {
                selectedFiles.Remove(mainFile.FileId);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = !checkBox1.Checked;
        }

        void MoveNext(int lastIndex)
        {
            if (!AutoMoveNext) return;
            if (lastIndex == -1) return;

            var count = listView3.Items.Count;
            listView3.SelectedItems.Clear();
            if (++lastIndex == count)
            {
                listView3.Items[0].Selected = true;
                listView3.EnsureVisible(0);
            }
            else
            {
                listView3.Items[lastIndex].Selected = true;
                listView3.EnsureVisible(lastIndex);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            int lastIndex = -1;
            foreach (ListViewItem item in listView3.Items)
            {
                if (!item.Selected) continue;

                item.Checked = !item.Checked;
                lastIndex = item.Index;
            }
            MoveNext(lastIndex);
        }

        void ChangeIgnoredMode(IgnoredMode ignoredMode)
        {
            int lastIndex = -1;
            listView3.BeginUpdate();
            foreach (ListViewItem item in listView3.Items)
            {
                if (!item.Selected) continue;

                SimilarRecord tag = (SimilarRecord)item.Tag;

                var result = markIgnoreCallback(tag.SimilarRecordId, ignoredMode);
                if (result)
                {
                    tag.IgnoredMode = ignoredMode;
                    item.SubItems[4].Text = SimilarFileHelper.IgnoredModeToString(ignoredMode);
                }
                lastIndex = item.Index;
            }
            listView3.EndUpdate();
            MoveNext(lastIndex);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            ChangeIgnoredMode(IgnoredMode.Effective);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ChangeIgnoredMode(IgnoredMode.HiddenButConnected);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ChangeIgnoredMode(IgnoredMode.HiddenAndDisconnected);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (RequestingGotoFile != null)
            {
                foreach (ListViewItem item in listView3.Items)
                {
                    if (!item.Selected) continue;

                    SimilarRecord tag = (SimilarRecord)item.Tag;
                    RequestingGotoFile(this, new RequestingGotoFileEventArgs(tag.FileId));
                    break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sortByRate = true;
            LoadSimilarRecords();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sortByRate = false;
            LoadSimilarRecords();
        }

    }

    internal class RequestingGotoFileEventArgs : EventArgs
    {
        public Guid TargetFileId { get; }

        public RequestingGotoFileEventArgs(Guid targetFileId)
        {
            TargetFileId = targetFileId;
        }
    }
}