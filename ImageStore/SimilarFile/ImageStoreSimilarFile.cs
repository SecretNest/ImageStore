using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    public enum IgnoredMode : int
    {
        Normal = 0,
        HiddenButConnected = 1,
        HiddenAndDisconnected = 2
    }

    public class ImageStoreSimilarFile
    {
        public Guid Id { get; private set; }

        public Guid File1Id { get; private set; }
        public Guid File2Id { get; private set; }
        public float DifferenceDegree { get; private set; }
        public IgnoredMode IgnoredMode { get; set; }

        internal int IgnoredModeCode
        {
            get
            {
                return (int)IgnoredMode;
            }
            set
            {
                IgnoredMode = (IgnoredMode)value;
            }
        }

        internal ImageStoreSimilarFile(Guid id, Guid file1Id, Guid file2Id, float differenceDegree)
        {
            Id = id;
            File1Id = file1Id;
            File2Id = file2Id;
            DifferenceDegree = differenceDegree;
        }
    }
}
