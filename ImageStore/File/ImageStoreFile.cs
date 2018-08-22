using Shipwreck.Phash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    public class ImageStoreFile
    {
        public Guid Id { get; private set; }

        public Guid FolderId { get; private set; }

        public string Path { get; private set; }

        public string FileName { get; private set; }

        public Guid ExtensionId { get; private set; }

        byte[] _imageHashByte;
        Digest _imageHashDigest;

        public byte[] ImageHash
        {
            get
            {
                return _imageHashByte;
            }
            set
            {
                _imageHashByte = value;
                if (value == null)
                {
                    _imageHashDigest = null;
                }
                else
                {
                    _imageHashDigest = new Digest();
                    _imageHashDigest.Coefficents = _imageHashByte;
                }
            }
        }

        internal Digest ImageHashDigest
        {
            get
            {
                return _imageHashDigest;
            }
            set
            {
                _imageHashDigest = value;
                if (value == null)
                {
                    _imageHashByte = null;
                }
                else
                {
                    _imageHashByte = value.Coefficents;
                }
            }
        }

        public byte[] Sha1Hash { get; set; }
        public int FileSize { get; set; }

        public FileState FileState { get; set; }
        internal int FileStateCode
        {
            get
            {
                return (int)FileState;
            }
            set
            {
                FileState = (FileState)value;
            }
        }

        public float ImageComparedThreshold { get; set; }

        public ImageStoreFile(Guid folderId, string path, string fileName, Guid extensionId)
        {
            Id = Guid.NewGuid();
            FolderId = folderId;
            Path = path;
            FileName = fileName;
            ExtensionId = extensionId;
        }

        internal ImageStoreFile(Guid id, Guid folderId, string path, string fileName, Guid extensionId)
        {
            Id = id;
            FolderId = folderId;
            Path = path;
            FileName = fileName;
            ExtensionId = extensionId;
        }

    }

    public enum FileState : int
    {
        New = 0,
        NotImage = 1,
        NotReadable = 2,
        SizeZero = 254,
        Computed = 255
    }

}
