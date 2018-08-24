# Search-ImageStoreIgnoredDirectory
Searches all ignore directory records which matches the conditions provided.

Alias: SearchIgnoredDirectory

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|FolderId|Guid?|Uses id of the related folder as an condition to search.|Yes|
|Directory|string|Uses path as an condition to search.|Yes|
|DirectoryPropertyComparingModes|[StringPropertyComparingModes](../../type/StringPropertyComparingModes.md)|The ways to use Directory in condition. Default value is ```Contains```.|Yes|
|IsSubDirectoryIncluded|bool?|Uses IsSubDirectoryIncluded property as an condition to search.|Yes|

Conditions will be ignored if not provided or set as null, unless a default value is specified above.

# Return
The list of the records which matches the conditions provided.

Type: List<[ImageStoreIgnoredDirectory](../../type/ImageStoreIgnoredDirectory.md)>

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Ignored Directory Cmdlets](../cmdlets.md#ignored-directory)