# Extension
ImageStore will deal with different type of files separately. Files will be grouped by using extension name.

You need to create records for all extensions need to be processed, one for each.

Property IsImage is used to decide whether the image hashing and comparing need to be done for this kind of files. It should be set to true for all image extensions. If it set to false, image hashing will be skipped but [data based comparison for checking exactly same files](SameFile.md) will still be processed.

# Ignored
Property Ignored is used to set this kind of file ignored for all comparison.

When set to true:

  * [Sync-ImageStoreFolder](../cmdlet/Folder/SyncFolder.md) will avoid the file from adding it to database which extension is set to ignored, but will not remove existed files of this kind from database.
  * [Measure-ImageStoreFile](../cmdlet/File/MeasureFile.md) will be ended with exception if the extension of the file is set to ignored.
  * [Measure-ImageStoreFiles](../cmdlet/File/MeasureFiles.md) will skip the file from computing hashing result which extension is set to ignored, but will not remove the computed result finished previously.

While changed to true, no existed record or data will be removed.

After changed to false, no record or data will be generated automatically. [Sync-ImageStoreFolder](../cmdlet/Folder/SyncFolder.md) and [Measure-ImageStoreFiles](../cmdlet/File/MeasureFiles.md) need to be run manually.

While running [Sync-ImageStoreFolder](../cmdlet/Folder/SyncFolder.md), the extension name found in files but not having a related extension setting will be treated as ignored too, with a warning message displayed.

# Cmdlets
  * To check or modify Extensions, you need to call [Extension Cmdlets](../cmdlet/cmdlets.md#extension).
