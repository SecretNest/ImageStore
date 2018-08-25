# Remove-ImageStoreFile
Removes the file record specified and the pointed file.

Alias: RemoveFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|File|[ImageStoreFile](../../type/ImageStoreFile.md)|File entity to be removed|No(*1)|
|Id|Guid|Id of the record to be removed|No(*1)|
|SkipFile|*switch*|Skips the file moving operating in file system.|-|
|OverrideSealedFolder|*switch*|Overrides the IsSealed mark of the folder.|-|

From Pipeline: File, Id

*1: One and only one parameter within this group should be provided.

# Return
None.

# Thumbprint Cache
To remove the related cache files, call [Set-ImageStoreThumbprintCacheFolder](../SimilarFile/SetThumbprintCacheFolder.md) before this operating.

# See also
  * [Concept: File](../../concept/File.md)
  * [Concept: ThumbprintCache](../../concept/ThumbprintCache.md)
  * [File Cmdlets](../cmdlets.md#file)
