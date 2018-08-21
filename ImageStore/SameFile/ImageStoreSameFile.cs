using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SameFile
{
    public class ImageStoreSameFile
    {
        public Guid Id { get; private set; }

        public byte[] Sha1Hash { get; private set; }

        public Guid FileId { get; private set; }

        public bool IsIgnored { get; set; }

        internal ImageStoreSameFile(Guid id, byte[] sha1Hash, Guid fileId)
        {
            Id = id;
            Sha1Hash = sha1Hash;
            FileId = fileId;
        }
    }
}
