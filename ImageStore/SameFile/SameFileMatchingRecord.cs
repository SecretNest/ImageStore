using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    class SameFileMatchingRecord
    {
        //public Guid SameFileId { get; set; }
        public int GroupNumber { get; set; }
        public byte[] Sha1Hash { get; set; }
        public bool IsInTargetFolder { get; set; }
        public Guid FolderId { get; set; }
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public bool IsIgnored { get; set; }

        int folderOrder = -2;

        public int GetFolderOrder(List<Guid> folderIds)
        {
            if (folderOrder != -2)
                return folderOrder;
            else
            {
                folderOrder = folderIds.IndexOf(FolderId);
                return folderOrder;
            }
        }

        public bool IsSelectedToReturn { get; set; }
        public bool IsAutoDealt { get; set; }
    }
}
