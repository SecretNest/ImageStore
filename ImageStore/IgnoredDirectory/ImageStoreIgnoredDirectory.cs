using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.IgnoredDirectory
{
    public class ImageStoreIgnoredDirectory
    {
        public Guid Id { get; private set; }
        public Guid FolderId { get; set; }

        public string Directory { get; set; }

        public bool IsSubDirectoryIncluded { get; set; }

        public ImageStoreIgnoredDirectory()
        {
            Id = Guid.NewGuid();
        }

        internal ImageStoreIgnoredDirectory(Guid id)
        {
            Id = id;
        }
    }
}
