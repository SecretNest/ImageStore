using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Extension
{
    public class ImageStoreExtension
    {
        public Guid Id { get; private set; }

        public string Extension { get; set; }

        public bool IsImage { get; set; }

        public bool Ignored { get; set; }

        public ImageStoreExtension()
        {
            Id = Guid.NewGuid();
        }

        internal ImageStoreExtension(Guid id)
        {
            Id = id;
        }
    }
}
