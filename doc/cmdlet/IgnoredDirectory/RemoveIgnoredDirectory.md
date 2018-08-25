# Remove-ImageStoreIgnoredDirectory
Removes the ignored directory record specified from database.

Alias: RemoveIgnoredDirectory

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|IgnoredDirectory|[ImageStoreIgnoredDirectory](../../type/ImageStoreIgnoredDirectory.md)|Ignored directory entity to be removed|No(*1)|
|Id|Guid|Id of the record to be removed|No(*1)|

From Pipeline: IgnoredDirectory, Id

*1: One and only one parameter within this group should be provided.

# Return
None.

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Ignored Directory Cmdlets](../cmdlets.md#ignored-directory)