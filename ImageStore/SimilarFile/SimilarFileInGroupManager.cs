using SecretNest.ImageStore.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretNest.ImageStore.SimilarFile
{
    partial class SimilarFileInGroupManager : Form
    {

        HashSet<Guid> selectedFiles;
        Dictionary<Guid, FileInfo> allFileInfo;
        Func<Guid, Image> loadImage;
        Dictionary<int, Dictionary<Guid, List<Guid>>> groupedFiles;
        Dictionary<Guid, ImageStoreSimilarFile> allRecords;
        Func<Guid, IgnoredMode, bool> markIgnoreCallback;
        public SimilarFileInGroupManager(HashSet<Guid> selectedFiles, Dictionary<Guid, FileInfo> allFileInfo, Func<Guid, Image> loadImage,
            Dictionary<int, Dictionary<Guid, List<Guid>>> groupedFiles,
            Dictionary<Guid, ImageStoreSimilarFile> allRecords, Func<Guid, IgnoredMode, bool> markIgnoreCallback)
        {
            InitializeComponent();
            this.selectedFiles = selectedFiles;
            this.allFileInfo = allFileInfo;
            this.loadImage = loadImage;
            this.groupedFiles = groupedFiles;
            this.allRecords = allRecords;
            this.markIgnoreCallback = markIgnoreCallback;
            doublePictureBox1.AutoResizePictures = checkBox3.Checked;
        }
        bool hideHiddenGroup = true;

        ListViewItem[] hiddenGroups;
        ListViewItem disconnectedGroupItem;

        private void SimilarFileManager_Load(object sender, EventArgs e)
        {
            List<ListViewItem> effectiveGroups = new List<ListViewItem>();
            List<ListViewItem> hiddenGroups = new List<ListViewItem>();

            listView1.BeginUpdate();
            var groupCount = groupedFiles.Count;
            groupBox1.Text = string.Format("Groups: {0}", groupCount);
            if (groupedFiles.ContainsKey(-1))
            {
                groupCount--;
            }
            Image[] images = new Image[groupCount];
            //Tuple<ListViewItem, bool>[] listViewItems = new Tuple<ListViewItem, bool>[groupCount];

            for (int i = 0; i < groupCount; i++)
            {
                var fileGroup = groupedFiles[i];
                images[i] = loadImage(fileGroup.Keys.First());
                bool isHiddenGroup = !fileGroup.Any(file => file.Value.Any(id => allRecords[id].IgnoredMode == IgnoredMode.Effective));
                var listViewItem = new ListViewItem(string.Format("{0} files", fileGroup.Count), i) { Tag = i };
                if (isHiddenGroup)
                {
                    hiddenGroups.Add(listViewItem);
                }
                else
                {
                    listViewItem.Group = listView1.Groups[0];
                    effectiveGroups.Add(listViewItem);
                }
            }

            imageList1.Images.AddRange(images);
            images = null;

            listView1.Items.AddRange(effectiveGroups.ToArray());
            this.hiddenGroups = hiddenGroups.ToArray();

            if (groupedFiles.TryGetValue(-1, out var disconnectedGroup))
            {
                int index = imageList1.Images.Count;
                imageList1.Images.Add(Resources.Disconnected);
                disconnectedGroupItem = new ListViewItem(string.Format("{0} files", disconnectedGroup.Count), index) { Tag = -1 };
            }
            else
            {
                disconnectedGroupItem = null;
            }

            listView1.EndUpdate();
            if (effectiveGroups.Count == 0)
            {
                if (listView1.Items.Count > 0)
                    listView1.Items[0].Selected = true;
                else
                {
                    selectedGroupId = -2;
                }
            }
            else
            {
                listView1.Groups[0].Items[0].Selected = true;
            }
            effectiveGroups = null;
            hiddenGroups = null;

            Focus();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach(ListViewItem item in listView1.Items)
            {
                if (item.Selected)
                {
                    int tag = (int)item.Tag;
                    if (tag == -1 && hideHiddenGroup)
                    {
                        shownHiddenChanging = true;
                        checkBox2.Checked = false;
                        shownHiddenChanging = false;
                    }
                    LoadGroup(tag);
                    return;
                }
            }
            selectedGroupId = -2;
            ClearGroup();
        }

        void ClearGroup()
        {
            dataGridView1.Rows.Clear();
            fileModeFile1Changing = true;
            fileModeFile2Changing = true;
            listView2.Items.Clear();
            listView3.Items.Clear();
            fileModeFile1Changing = false;
            fileModeFile2Changing = false;
        }

        int selectedGroupId; //-1: disconnected; -2: none; else: group id
        bool relationMode;
        void LoadGroup(int groupId)
        {
            selectedGroupId = groupId;
            RefreshWhenHiddenVisibleChanged();
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            relationMode = tabControl1.SelectedIndex == 1;
            RefreshWhenHiddenVisibleChanged();
        }

        void RefreshWhenHiddenVisibleChanged()
        {
            if (relationMode)
                LoadToRelationTab();
            else
                LoadToFileTab();
        }

        #region FileMode

        bool fileModeFile1Changing = false;
        bool fileModeFile2Changing = false;
        Guid fileModeFile1SelectedFileId;
        ImageStoreSimilarFile selectedFileModeSimilarFileRecord;
        int selectedFileModeFile2RowIndex;
        bool fileModeSimilarFile1IsFile1;
        void LoadToFileTab()
        {
            fileModeFile1Changing = true;
            fileModeFile2Changing = true;
            listView2.Items.Clear();
            listView3.Items.Clear();
            fileModeFile2Changing = false;
            var showHiddenRecord = checkBox1.Checked;
            if (selectedGroupId != -2)
            {
                var grouped = groupedFiles[selectedGroupId];

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (var file1 in grouped)
                {
                    var fileId = file1.Key;
                    var show = showHiddenRecord || file1.Value.Select(i => allRecords[i]).Any(i => i.IgnoredMode == IgnoredMode.Effective);
                    if (show)
                    {
                        var selected = selectedFiles.Contains(fileId);
                        var fileInfo = allFileInfo[fileId];
                        items.Add(new ListViewItem(new string[] { fileInfo.FileNameWithExtension, fileInfo.PathToDirectory, fileInfo.FileSize.ToString() }) { Checked = selected, Tag = fileId });
                    }
                }

                listView2.Items.AddRange(items.OrderBy(i => i.SubItems[1].Text).ThenBy(i => i.SubItems[0].Text).ToArray());
            }
            fileModeFile1Changing = false;
            if (listView2.Items.Count > 0)
            {
                listView2.Items[0].Selected = true;
            }
            else
            {
                button16.Enabled = false;
                button15.Enabled = false;
                button14.Enabled = false;
                button13.Enabled = false;
                button12.Enabled = false;
                button11.Enabled = false;
                button10.Enabled = false;
                doublePictureBox1.ClearPictures();
            }
        }
        
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach(ListViewItem item in listView2.Items)
            {
                if (!item.Selected) continue;

                fileModeFile1SelectedFileId = (Guid)item.Tag;
                FileModeLoadSimilarRecords();

                return;
            }

            listView3.Items.Clear();
            fileModeFile1SelectedFileId = Guid.Empty;
            doublePictureBox1.ClearPictures();
        }

        private void listView2_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (fileModeFile1Changing) return;
            var file1Id = (Guid)e.Item.Tag;
            bool check = e.Item.Checked;
            if (check)
            {
                selectedFiles.Add(file1Id);
            }
            else
            {
                selectedFiles.Remove(file1Id);
            }
            fileModeFile2Changing = true;
            foreach(ListViewItem item in listView3.Items)
            {
                if (((Tuple<Guid, Guid, bool>)item.Tag).Item2 == file1Id)
                    item.Checked = check;
            }
            fileModeFile2Changing = false;
        }

        void FileModeLoadSimilarRecords()
        {
            var showHiddenRecord = checkBox1.Checked;
            fileModeFile2Changing = true;
            listView3.Items.Clear();
            var relatedRecords = groupedFiles[selectedGroupId][fileModeFile1SelectedFileId];
            ImageStoreSimilarFile[] grouped;
            if (showHiddenRecord)
            {
                grouped = relatedRecords.ConvertAll(i => allRecords[i]).OrderBy(i => i.DifferenceDegree).ToArray();
            }
            else
            {
                grouped = relatedRecords.ConvertAll(i => allRecords[i]).Where(i => i.IgnoredMode == IgnoredMode.Effective).OrderBy(i => i.IgnoredModeCode).ThenBy(i => i.DifferenceDegree).ToArray();
            }

            if (grouped.Length > 0)
            {
                List<ListViewItem> items = new List<ListViewItem>();
                foreach (var similarFileRecord in grouped)
                {
                    var isFile1InMain = similarFileRecord.File1Id == fileModeFile1SelectedFileId;
                    FileInfo file;
                    if (isFile1InMain)
                        file = allFileInfo[similarFileRecord.File2Id];
                    else
                        file = allFileInfo[similarFileRecord.File1Id];
                    var selected = selectedFiles.Contains(file.FileId);
                    items.Add(new ListViewItem(new string[] {
                        similarFileRecord.DifferenceDegree.ToString("0.0000"),
                        file.FileNameWithExtension, file.PathToDirectory, file.FileSize.ToString(),
                        SimilarFileHelper.IgnoredModeToString(similarFileRecord.IgnoredMode)
                    })
                    {
                        Checked = selected,
                        Tag = new Tuple<Guid, Guid, bool>(similarFileRecord.Id, file.FileId, isFile1InMain)
                    });
                }

                listView3.Items.AddRange(items.OrderBy(i=>i.SubItems[2].Text).ThenBy(i=>i.SubItems[0].Text).ThenBy(i=>i.SubItems[1].Text).ToArray());
                listView3.Items[0].Selected = true;
            }
            fileModeFile2Changing = false;
        }

        private void listView3_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (fileModeFile2Changing) return;
            if (listView3MouseDown)
            {
                e.NewValue = e.CurrentValue;
            }
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

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView3.Items)
            {
                if (!item.Selected) continue;

                Tuple<Guid, Guid, bool> tag = (Tuple<Guid, Guid, bool>)item.Tag;
                selectedFileModeSimilarFileRecord = allRecords[tag.Item1];
                selectedFileModeFile2RowIndex = item.Index;
                fileModeSimilarFile1IsFile1 = tag.Item3;

                if (fileModeSimilarFile1IsFile1)
                    doublePictureBox1.LoadPictures(allFileInfo[selectedFileModeSimilarFileRecord.File1Id], allFileInfo[selectedFileModeSimilarFileRecord.File2Id]);
                else
                    doublePictureBox1.LoadPictures(allFileInfo[selectedFileModeSimilarFileRecord.File2Id], allFileInfo[selectedFileModeSimilarFileRecord.File1Id]);

                button16.Enabled = true;
                button15.Enabled = true;
                button14.Enabled = true;
                button13.Enabled = true;
                button12.Enabled = true;
                button11.Enabled = true;
                button10.Enabled = true;

                return;
            }

            button16.Enabled = false;
            button15.Enabled = false;
            button14.Enabled = false;
            button13.Enabled = false;
            button12.Enabled = false;
            button11.Enabled = false;
            button10.Enabled = false;

            //fileModeFile2SelectedFileId = Guid.Empty;
            selectedFileModeSimilarFileRecord = null;
            selectedFileModeFile2RowIndex = -1;
            doublePictureBox1.ClearPictures();
        }

        private void listView3_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (fileModeFile2Changing) return;
            var file2Id = ((Tuple<Guid, Guid, bool>)e.Item.Tag).Item2;
            bool check = e.Item.Checked;
            if (check)
            {
                selectedFiles.Add(file2Id);
            }
            else
            {
                selectedFiles.Remove(file2Id);
            }
            fileModeFile1Changing = true;
            foreach (ListViewItem item in listView2.Items)
            {
                if ((Guid)item.Tag == file2Id)
                    item.Checked = check;
            }
            fileModeFile1Changing = false;
        }

        void FileModeReloadSelectedState()
        {
            fileModeFile1Changing = true;
            foreach (ListViewItem row in listView2.Items)
            {
                var fileId = (Guid)row.Tag;
                var selected = selectedFiles.Contains(fileId);
                if (selected != row.Checked)
                    row.Checked = selected;
            }
            fileModeFile1Changing = false;

            fileModeFile2Changing = true;
            foreach (ListViewItem row in listView3.Items)
            {
                var fileId = ((Tuple<Guid, Guid, bool>)row.Tag).Item2;
                var selected = selectedFiles.Contains(fileId);
                if (selected != row.Checked)
                    row.Checked = selected;
            }
            fileModeFile2Changing = false;
        }

        void FileModeChangeSelection(bool? select1st, bool? select2nd)
        {
            if (selectedFileModeFile2RowIndex == -1) return;
            bool changed = false;

            if (select1st.HasValue)
            {
                if (select1st.Value)
                    changed = selectedFiles.Add(selectedFileModeSimilarFileRecord.File1Id) || changed;
                else
                    changed = selectedFiles.Remove(selectedFileModeSimilarFileRecord.File1Id) || changed;
            }

            if (select2nd.HasValue)
            {
                if (select2nd.Value)
                    changed = selectedFiles.Add(selectedFileModeSimilarFileRecord.File2Id) || changed;
                else
                    changed = selectedFiles.Remove(selectedFileModeSimilarFileRecord.File2Id) || changed;
            }

            if (changed)
                FileModeReloadSelectedState();

            FileModeMoveNext();
        }

        void FileModeMoveNext()
        {
            if (checkBox4.Checked)
            {
                int index = selectedFileModeFile2RowIndex;
                FileModeMoveNext(index);
            }
        }

        void FileModeMoveNext(int selectedIndex)
        {
            var count = listView3.Items.Count;
            listView3.SelectedItems.Clear();
            if (++selectedIndex == count)
            {
                listView3.Items[0].Selected = true;
                listView3.EnsureVisible(0);
            }
            else
            {
                listView3.Items[selectedIndex].Selected = true;
                listView3.EnsureVisible(selectedIndex);
            }
        }

        void FileModeChangeIgnore(IgnoredMode mode)
        {
            if (selectedFileModeFile2RowIndex == -1) return;
            int lastSelected = selectedFileModeFile2RowIndex;

            listView3.BeginUpdate();
            foreach(ListViewItem listViewItem in listView3.Items)
            {
                if (listViewItem.Selected)
                {
                    //Tuple<Guid, Guid, bool>(similarFileRecordId, fileId, isFile1InMain)
                    Tuple<Guid, Guid, bool> tag = (Tuple<Guid, Guid, bool>)listViewItem.Tag;
                    var record = allRecords[tag.Item1];
                    var result = markIgnoreCallback(tag.Item1, mode);
                    if (result)
                    {
                        record.IgnoredMode = mode;
                        listViewItem.SubItems[4].Text = SimilarFileHelper.IgnoredModeToString(mode);
                    }
                    lastSelected = listViewItem.Index;
                }
            }
            listView3.EndUpdate();
            FileModeMoveNext(lastSelected);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            FileModeChangeSelection(false, false);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            FileModeChangeSelection(true, true);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (fileModeSimilarFile1IsFile1)
                FileModeChangeSelection(true, null);
            else
                FileModeChangeSelection(null, true);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (fileModeSimilarFile1IsFile1)
                FileModeChangeSelection(null, true);
            else
                FileModeChangeSelection(true, null);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            FileModeChangeIgnore(IgnoredMode.Effective);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            FileModeChangeIgnore(IgnoredMode.HiddenButConnected);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            FileModeChangeIgnore(IgnoredMode.HiddenAndDisconnected);
        }

        #endregion

        #region RelationMode
        void LoadToRelationTab()
        {
            dataGridView1.Rows.Clear();
            var showHiddenRecord = checkBox1.Checked;

            //Dictionary<Guid, List<ImageStoreSimilarFile>> grouped;
            //Dictionary<Guid, List<Guid>> grouped;

            List<ImageStoreSimilarFile> grouped;

            if (selectedGroupId != -2)
            {
                if (showHiddenRecord)
                {
                    grouped = groupedFiles[selectedGroupId].Values.SelectMany(i => i.Select(j => allRecords[j])).Distinct().OrderBy(i => i.IgnoredModeCode).ThenBy(i => i.DifferenceDegree).ToList();
                }
                else
                {
                    grouped = groupedFiles[selectedGroupId].Values.SelectMany(i => i.Select(j => allRecords[j])).Where(i => i.IgnoredMode == IgnoredMode.Effective).Distinct().OrderBy(i => i.DifferenceDegree).ToList();
                }
            }
            else
            {
                grouped = null;
            }

            if (grouped?.Count > 0)
            {
                DataGridViewRow[] rows = new DataGridViewRow[grouped.Count];
                Parallel.For(0, grouped.Count, rowIndex =>
                {
                    var similarFileRecord = grouped[rowIndex];
                    var selected1 = selectedFiles.Contains(similarFileRecord.File1Id);
                    var selected2 = selectedFiles.Contains(similarFileRecord.File2Id);
                    string selectedText;
                    if (selected1)
                    {
                        if (selected2)
                            selectedText = "✓ 1:\n✓ 2:";
                        else
                            selectedText = "✓ 1:\n2:";
                    }
                    else
                    {
                        if (selected2)
                            selectedText = "1:\n✓ 2:";
                        else
                            selectedText = "1:\n2:";
                    }
                    var file1 = allFileInfo[similarFileRecord.File1Id];
                    var file2 = allFileInfo[similarFileRecord.File2Id];

                    DataGridViewRow row = new DataGridViewRow();
                    row.Height = (int)(row.Height * 1.5);
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = similarFileRecord.DifferenceDegree.ToString("0.0000")});
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = selectedText });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = file1.FileNameWithExtension + "\n" + file2.FileNameWithExtension });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = file1.PathToDirectory + "\n" + file2.PathToDirectory });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = file1.FileSize.ToString() + "\n" + file2.FileSize.ToString() });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = SimilarFileHelper.IgnoredModeToString(similarFileRecord.IgnoredMode) });
                    row.Tag = similarFileRecord.Id;
                    rows[rowIndex] = row;
                });
                dataGridView1.Rows.AddRange(rows);
                dataGridView1.Rows[0].Selected = true;
            }
            else
            {
                doublePictureBox1.ClearPictures();
            }
        }


        void RelationModeReloadSelectedState()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var similarFileId = (Guid)row.Tag;
                var similarFileRecord = allRecords[similarFileId];
                var selected1 = selectedFiles.Contains(similarFileRecord.File1Id);
                var selected2 = selectedFiles.Contains(similarFileRecord.File2Id);
                string selectedText;
                if (selected1)
                {
                    if (selected2)
                        selectedText = "✓ 1:\n✓ 2:";
                    else
                        selectedText = "✓ 1:\n2:";
                }
                else
                {
                    if (selected2)
                        selectedText = "1:\n✓ 2:";
                    else
                        selectedText = "1:\n2:";
                }
                row.Cells[1].Value = selectedText;
            }
        }

        ImageStoreSimilarFile selectedRelationModeSimilarFileRecord;
        int selectedRalationModeRowIndex;
        private void dataGridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //selectedRalationModeRowIndex = -1;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.Selected) continue;
                selectedRalationModeRowIndex = row.Index;
                var similarFileId = (Guid)row.Tag;
                selectedRelationModeSimilarFileRecord = allRecords[similarFileId];

                doublePictureBox1.LoadPictures(allFileInfo[selectedRelationModeSimilarFileRecord.File1Id], allFileInfo[selectedRelationModeSimilarFileRecord.File2Id]);

                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
                button8.Enabled = true;
                button9.Enabled = true;
                return;
            }

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            selectedRalationModeRowIndex = -1;
            selectedRelationModeSimilarFileRecord = null;
            doublePictureBox1.ClearPictures();
        }

        void RelationModeChangeSelection(bool select1st, bool select2nd)
        {
            if (selectedRalationModeRowIndex == -1) return;
            bool changed = false;

            if (select1st)
                changed = selectedFiles.Add(selectedRelationModeSimilarFileRecord.File1Id) || changed;
            else
                changed = selectedFiles.Remove(selectedRelationModeSimilarFileRecord.File1Id) || changed;

            if (select2nd)
                changed = selectedFiles.Add(selectedRelationModeSimilarFileRecord.File2Id) || changed;
            else
                changed = selectedFiles.Remove(selectedRelationModeSimilarFileRecord.File2Id) || changed;

            if (changed)
                RelationModeReloadSelectedState();

            RelationModeMoveNext();
        }

        void RelationModeChangeIgnore(IgnoredMode mode)
        {
            if (selectedRalationModeRowIndex == -1) return;
            int lastSelected = selectedRalationModeRowIndex;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Selected)
                {
                    var record = allRecords[(Guid)row.Tag];
                    var result = markIgnoreCallback(record.Id, mode);
                    if (result)
                    {
                        record.IgnoredMode = mode;
                        row.Cells[5].Value = SimilarFileHelper.IgnoredModeToString(mode);
                    }
                    lastSelected = row.Index;
                }
            }

            RelationModeMoveNext(lastSelected);
        }

        void RelationModeMoveNext()
        {
            if (checkBox4.Checked)
            {
                int index = selectedRalationModeRowIndex;
                RelationModeMoveNext(index);
            }
        }

        void RelationModeMoveNext(int selectedIndex)
        {
            dataGridView1.ClearSelection();
            if (++selectedIndex == dataGridView1.RowCount)
            {
                dataGridView1.Rows[0].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = 0;
            }
            else
            {
                dataGridView1.Rows[selectedIndex].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = Math.Max(0, selectedIndex - 2);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RelationModeChangeSelection(false, false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RelationModeChangeSelection(true, true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RelationModeChangeSelection(true, false);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            RelationModeChangeSelection(false, true);
        }


        private void button7_Click(object sender, EventArgs e)
        {
            RelationModeChangeIgnore(IgnoredMode.Effective);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            RelationModeChangeIgnore(IgnoredMode.HiddenButConnected);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            RelationModeChangeIgnore(IgnoredMode.HiddenAndDisconnected);
        }
        #endregion

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            doublePictureBox1.AutoResizePictures = checkBox3.Checked;
        }

        private void SimilarFileManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            doublePictureBox1.ClearPictures();
        }


        bool shownHiddenChanging = false;
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            hideHiddenGroup = !checkBox2.Checked;

            listView1.BeginUpdate();
            if (hideHiddenGroup)
            {
                if (disconnectedGroupItem != null)
                    listView1.Items.Remove(disconnectedGroupItem);
                foreach (var item in hiddenGroups)
                    listView1.Items.Remove(item);
            }
            else
            {
                if (disconnectedGroupItem != null)
                {
                    disconnectedGroupItem.Group = listView1.Groups[2];
                    listView1.Items.Add(disconnectedGroupItem);
                }
                foreach (var item in hiddenGroups)
                    item.Group = listView1.Groups[1];
                listView1.Items.AddRange(hiddenGroups);
                if (listView1.SelectedItems.Count == 0 && listView1.Items.Count > 0)
                {
                    listView1.Items[0].Selected = true;
                }
            }
            listView1.EndUpdate();


            if (shownHiddenChanging) return;

            RefreshWhenHiddenVisibleChanged();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            RefreshWhenHiddenVisibleChanged();
        }
    }
}
