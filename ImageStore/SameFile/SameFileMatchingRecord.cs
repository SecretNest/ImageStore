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
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public bool IsIgnored { get; set; }

        public bool IsSelectedToReturn { get; set; }
        public bool IsAutoDealt { get; set; }
    }
}
