# Remove-ImageStoreExtension
Removes the extension record specified from database.

Alias: RemoveExtension

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Extension|[ImageStoreExtension](../../type/ImageStoreExtension.md)|Extension entity to be removed|No(*1)|
|Id|Guid|Id of the record to be removed|No(*1)|

From Pipeline: Extension, Id

*1: One and only one parameter within this group should be provided.

# Return
None.

# Thumbprint Cache
To remove the related cache files, call [Set-ImageStoreThumbprintCacheFolder](../SimilarFile/SetThumbprintCacheFolder.md) before this operating.

# See also
  * [Concept: Extension](../../concept/Extension.md)
  * [Concept: ThumbprintCache](../../concept/ThumbprintCache.md)
  * [Extension Cmdlets](../cmdlets.md#extension)
