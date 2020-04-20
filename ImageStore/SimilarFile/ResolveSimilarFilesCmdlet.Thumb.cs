using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    partial class ResolveSimilarFilesCmdlet
    {
        ConcurrentDictionary<Guid, Image> thumbs = new ConcurrentDictionary<Guid, Image>();

        void PrepareFileThumb(Guid fileId)
        {
            thumbs.GetOrAdd(fileId, i => LoadImageHelper.GetThumbprintImage(fileId, allFileInfo[fileId].FilePath));
        }

        Image LoadFileThumb(Guid fileId)
        {
            return thumbs[fileId];
        }
    }
}
