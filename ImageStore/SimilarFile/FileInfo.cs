using SecretNest.ImageStore.File;
using SecretNest.ImageStore.Folder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    class FileInfo
    {
        public Guid FileId { get; private set; }
        public string FolderName { get; private set; }
        public string PathToDirectory { get; private set; }
        public string FileNameWithExtension { get; private set; }
        public string ExtensionName { get; private set; }
        public int FileSize { get; private set; }
        public bool FolderIsSealed { get; private set; }
        public string FilePath { get; private set; }

        public void SetData(ImageStoreFile file, ImageStoreFolder folder, string extensionName)
        {
            FileId = file.Id;
            ExtensionName = extensionName;
            FolderName = folder.Name;
            FolderIsSealed = folder.IsSealed;
            if (extensionName == "")
                FileNameWithExtension = file.FileName;
            else
                FileNameWithExtension = file.FileName + "." + extensionName;
            if (file.Path == "")
            {
                PathToDirectory = folder.Path;
                if (folder.Path.EndsWith(DirectorySeparatorString.Value))
                    FilePath = folder.Path + FileNameWithExtension;
                else
                    FilePath = folder.Path + DirectorySeparatorString.Value + FileNameWithExtension;
            }
            else
            {
                if (folder.Path.EndsWith(DirectorySeparatorString.Value))
                    PathToDirectory = folder.Path + file.Path;
                else
                    PathToDirectory = folder.Path + DirectorySeparatorString.Value + file.Path;
                FilePath = PathToDirectory + DirectorySeparatorString.Value + FileNameWithExtension;
            }
            FileSize = file.FileSize;
        }

        //public FileInfo(ImageStoreFile file, ImageStoreFolder folder, string extensionName)
        //{
        //    FileId = file.Id;
        //    ExtensionName = extensionName;
        //    FolderName = folder.Name;
        //    FolderIsSealed = folder.IsSealed;
        //    if (extensionName == "")
        //        FileNameWithExtension = file.FileName;
        //    else
        //        FileNameWithExtension = file.FileName + "." + extensionName;
        //    if (file.Path == "")
        //    {
        //        PathToDirectory = folder.Path;
        //        if (folder.Path.EndsWith(DirectorySeparatorString.Value))
        //            FilePath = folder.Path + FileNameWithExtension;
        //        else
        //            FilePath = folder.Path + DirectorySeparatorString.Value + FileNameWithExtension;
        //    }
        //    else
        //    {
        //        if (folder.Path.EndsWith(DirectorySeparatorString.Value))
        //            PathToDirectory = folder.Path + file.Path;
        //        else
        //            PathToDirectory = folder.Path + DirectorySeparatorString.Value + file.Path;
        //        FilePath = PathToDirectory + DirectorySeparatorString.Value + FileNameWithExtension;
        //    }
        //    FileSize = file.FileSize;
        //}
    }
}
