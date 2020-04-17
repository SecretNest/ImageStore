using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.File
{
    class MeasureFileHelper : IDisposable
    {
        SHA1Managed sha1 = new SHA1Managed();

        byte[] ComputeSha1Hash(FileStream stream)
        {
            byte[] hash = sha1.ComputeHash(stream);
            return hash;
            //StringBuilder formatted = new StringBuilder(2 * hash.Length);
            //foreach (byte b in hash)
            //{
            //    formatted.AppendFormat("{0:X2}", b);
            //}
            //return formatted.ToString();
        }

        Digest ComputeImageHash(FileStream stream)
        {
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            {
                var hash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage());
                return hash;
            }
        }


        internal ErrorRecord ProcessFile(string filePath, bool isImage, out Digest imageHash, out byte[] sha1Hash, out int fileSize, out FileState fileState)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileSize = (int)stream.Length;
                    if (fileSize == 0)
                    {
                        fileState = FileState.SizeZero;
                        imageHash = null;
                        fileSize = 0;
                        sha1Hash = null;
                        return null;
                    }

                    sha1Hash = ComputeSha1Hash(stream);

                    if (isImage)
                    { 
                        stream.Seek(0, SeekOrigin.Begin);
                        try
                        {
                            imageHash = ComputeImageHash(stream);
                        }
                        catch (Exception ex)
                        {
                            fileState = FileState.NotImage;
                            imageHash = null;
                            return new ErrorRecord(ex, "ImageStore Measuring - Image Hashing", ErrorCategory.InvalidData, filePath);
                        }
                    }
                    else
                    {
                        imageHash = null;
                    }
                }
            }
            catch (Exception ex)
            {
                fileState = FileState.NotReadable;
                imageHash = null;
                fileSize = -1;
                sha1Hash = null;
                return new ErrorRecord(ex, "ImageStore Measuring", ErrorCategory.OpenError, filePath);
            }

            fileState = FileState.Computed;
            return null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    sha1.Dispose();
                }

                sha1 = null;
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ComputeHelper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
