# Same File
ImageStore use Sha1 hashing to detect same files.

To detect same files, first add all files being checked to database, then call [Measure-ImageStoreFile](../cmdlet/File/MeasureFile.md) or [Measure-ImageStoreFiles](../cmdlet/File/MeasureFiles.md) to calculate Sha1 hash, finally call [Compare-ImageStoreSameFiles](../cmdlet/SameFile/CompareSameFiles.md) to find all same files.

Same files will be grouped into Same File Groups based on Sha1 hashing result.

# Same File Group
All files with same Sha1 hashing result will be placed in one Same File Group.

# Remove Same Files
After detection, you could use [Select-ImageStoreSameFile](../cmdlet/SameFile/SelectSameFile.md) to return same files. This cmdlet powered with algorithm to deal with same files. See the cmdlet document for details. Combines the result with [Remove
-ImageStoreFile](../cmdlet/File/RemoveFile.md) to delete the same files found.

# Cmdlets
  * To check or modify Same File, you need to call [Same File Cmdlets](../cmdlet/cmdlets.md#same-file).
