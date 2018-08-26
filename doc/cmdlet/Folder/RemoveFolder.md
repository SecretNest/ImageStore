# Remove-ImageStoreFolder
Removes the folder record specified from database.

Alias: RemoveFolder

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Folder|[ImageStoreFolder](../../type/ImageStoreFolder.md)|Folder entity to be removed|No(*1)|
|Id|Guid|Id of the record to be removed|No(*1)|

From Pipeline: Folder, Id

*1: One and only one parameter within this group should be provided.

# Return
None.

# Thumbprint Cache
To remove the related cache files, call [Set-ImageStoreThumbprintCacheFolder](../SimilarFile/SetThumbprintCacheFolder.md) before this operating.

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Concept: ThumbprintCache](../../concept/ThumbprintCache.md)
  * [Folder Cmdlets](../cmdlets.md#folder)