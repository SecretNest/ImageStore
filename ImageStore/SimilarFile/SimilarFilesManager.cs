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
        public SimilarFilesManager(HashSet<Guid> selectedFiles, Dictionary<Guid, FileInfo> allFileInfo,
            
            Func<Guid, IgnoredMode, bool> markIgnoreCallback)
        {
            InitializeComponent();
            similarFileCheck1.Initialize(selectedFiles, allFileInfo, markIgnoreCallback);
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
            
        }

        //similarFileCheck1.LoadFile(mainFileId, similarRecords);  List<SimilarFileRecord>

        private void SimilarFilesManager_Load(object sender, EventArgs e)
        {

        }
    }
}
