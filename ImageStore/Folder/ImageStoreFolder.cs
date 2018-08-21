using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Folder
{
    public class ImageStoreFolder
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }

        string _path;
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = GetExactPathName(value);
            }
        }
        public CompareImageWith CompareImageWith { get; set; }
        public int CompareImageWithCode
        {
            get
            {
                return (int)CompareImageWith;
            }
            set
            {
                CompareImageWith = (CompareImageWith)value;
            }
        }

        public bool IsSealed { get; set; }

        public ImageStoreFolder()
        {
            Id = Guid.NewGuid();
        }

        internal ImageStoreFolder(Guid id, string path)
        {
            Id = id;
            _path = path;
        }

        static string GetExactPathName(string pathName)
        {
            if (!(System.IO.File.Exists(pathName) || System.IO.Directory.Exists(pathName)))
                return pathName;

            var di = new DirectoryInfo(pathName);

            if (di.Parent != null)
            {
                return System.IO.Path.Combine(
                    GetExactPathName(di.Parent.FullName),
                    di.Parent.GetFileSystemInfos(di.Name)[0].Name);
            }
            else
            {
                return di.Name.ToUpper();
            }
        }
    }

    public enum CompareImageWith : int
    {
        All = 0,
        FilesInOtherDirectories = 1,
        FilesInOtherFolders = 2
    }
}
