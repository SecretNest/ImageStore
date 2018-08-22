# Thumbprint Cache
In the UI of [Resolve-ImageStoreSimilarFiles](../cmdlet/SimilarFile/ResolveSimilarFiles.md), each group will be displayed with a small size image generated from one of the image files within the group. The generating process will cost huge IO reading flow and CPU time. With thumbprint cache function enabled, those small sized image files will be save in the folder specified by user, and will be used next time directly.

**Note**: If you plan to use thumbprint cache, you should keep it on while all file operating. All file removal processing will delete the related cache file automatically, only if the thumbprint cache folder is set while operating.

# Cmdlets
  * To set, clear or clean the Thumbprint Cache, you need to call [Similar File Cmdlets](../cmdlet/cmdlets.md#similar-file).