# Similar File
ImageStore use pHash to find the similar image files.

To detect similar files, first add all files being checked to database, second call [Measure-ImageStoreFile](../cmdlet/File/MeasureFile.md) or [Measure-ImageStoreFiles](../cmdlet/File/MeasureFiles.md) to calculate Sha1 hash, then call [Compare-ImageStoreSimilarFiles](../cmdlet/SimilarFile/CompareSimilarFiles.md) to find all similar relationships.

[Resolve-ImageStoreSimilarFiles](../cmdlet/SimilarFile/ResolveSimilarFiles.md) will group the similar relationships and display them with a UI.

**Note**: [Thumbprint cache](ThumbprintCache.md) may be useful while you calling Resolve-ImageStoreSimilarFiles repeatedly.

# Cmdlets
  * To check or modify Similar Files, you need to call [Similar File Cmdlets](../cmdlet/cmdlets.md#similar-file).