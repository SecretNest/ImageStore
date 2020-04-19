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
    internal partial class SimilarFileOneManager : Form
    {
        Guid mainFileId; 
        List<ImageStoreSimilarFile> similarRecords;
        public SimilarFileOneManager(HashSet<Guid> selectedFiles, Dictionary<Guid, FileInfo> allFileInfo, 
            Guid mainFileId, List<ImageStoreSimilarFile> similarRecords,
            Func<Guid, IgnoredMode, bool> markIgnoreCallback)
        {
            InitializeComponent();
            similarFileCheck1.Initialize(selectedFiles, allFileInfo, markIgnoreCallback, false);
            similarFileCheck1.AutoResizePictures = checkBox3.Checked;
            similarFileCheck1.AutoMoveNext = checkBox4.Checked;
            this.mainFileId = mainFileId;
            this.similarRecords = similarRecords;
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
            LoadFile();
        }

        void LoadFile()
        {
            if (checkBox2.Checked)
                similarFileCheck1.LoadFile(mainFileId, similarRecords);
            else
                similarFileCheck1.LoadFile(mainFileId, similarRecords.Where(i => i.IgnoredMode == IgnoredMode.Effective));
        }

        private void SimilarFileOneManager_Load(object sender, EventArgs e)
        {
            LoadFile();
        }
    }
}
