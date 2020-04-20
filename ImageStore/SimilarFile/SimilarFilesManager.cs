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
    internal partial class SimilarFilesManager : Form
    {
        List<Tuple<Guid, List<ImageStoreSimilarFile>>> allRecords;
        Dictionary<Guid, int> indicesOfListViewItems;
        public SimilarFilesManager(HashSet<Guid> selectedFiles, Dictionary<Guid, FileInfo> allFileInfo,
            List<Tuple<Guid, List<ImageStoreSimilarFile>>> allRecords, //fileId, similarRecords
            Func<Guid, IgnoredMode, bool> markIgnoreCallback)
        {
            InitializeComponent();
            similarFileCheck1.Initialize(selectedFiles, allFileInfo, markIgnoreCallback, true);
            this.allRecords = allRecords;
            similarFileCheck1.AutoResizePictures = checkBox3.Checked;
            similarFileCheck1.AutoMoveNext = checkBox4.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            similarFileCheck1.AutoMoveNext = checkBox4.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            similarFileCheck1.AutoResizePictures = checkBox3.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            var selectedMainFileId = similarFileCheck1.MainFileId;

            LoadMainFiles();

            if (selectedMainFileId == null || !indicesOfListViewItems.TryGetValue(selectedMainFileId.Value, out var index))
            {
                if (listView1.Items.Count > 0)
                    listView1.Items[0].Selected = true;
            }
            else
            {
                listView1.Items[index].Selected = true;
                listView1.EnsureVisible(index);
            }
        }

        public void LoadThumbs(Image[] images)
        {
            imageList1.Images.AddRange(images);
        }

        void LoadMainFiles()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            indicesOfListViewItems = new Dictionary<Guid, int>();
            var fileCount = allRecords.Count;

            if (fileCount > 0) //cannot be 1 due to relation is two-way.
            {
                groupBox1.Text += string.Format(" {0} files", fileCount);
            }
            else
            {
                listView1.EndUpdate();
                return;
            }

            List<ListViewItem> listViewItems = new List<ListViewItem>();
            var showHidden = checkBox2.Checked;

            int indexOfListView = 0;
            for (int indexOfAllRecords = 0; indexOfAllRecords < allRecords.Count; indexOfAllRecords++)
            {
                var file = allRecords[indexOfAllRecords];
                if (showHidden || file.Item2.Any(i => i.IgnoredMode == IgnoredMode.Effective))
                {
                    listViewItems.Add(new ListViewItem(string.Format("{0} files", file.Item2.Count), indexOfAllRecords) { Tag = indexOfAllRecords });
                    indicesOfListViewItems.Add(file.Item1, indexOfListView++);
                }
            }

            listView1.Items.AddRange(listViewItems.ToArray());

            listView1.EndUpdate();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Selected)
                {
                    int index = (int)item.Tag;
                    var selected = allRecords[index];
                    var showHidden = checkBox2.Checked;
                    if (showHidden)
                    {
                        similarFileCheck1.LoadFile(selected.Item1, selected.Item2);
                    }
                    else
                    {
                        similarFileCheck1.LoadFile(selected.Item1, selected.Item2.Where(i => i.IgnoredMode == IgnoredMode.Effective));
                    }
                    return;
                }
            }
            similarFileCheck1.ClearFile();
        }

        private void similarFileCheck1_RequestingGotoFile(object sender, RequestingGotoFileEventArgs e)
        {
            var index = indicesOfListViewItems[e.TargetFileId];
            listView1.Items[index].Selected = true;
            listView1.EnsureVisible(index);
        }

        private void SimilarFilesManager_Load(object sender, EventArgs e)
        {
            LoadMainFiles();

            if (listView1.Items.Count > 0)
                listView1.Items[0].Selected = true;

            Focus();
        }
    }
}
