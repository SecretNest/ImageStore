# Search-ImageStoreIgnoredDirectory
Searches all ignored directory records matched with the conditions provided.

Alias: SearchIgnoredDirectory

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|FolderId|Guid?|Filters records by the folder linked.|Yes|
|Directory|string|Filters records by matching the path to be ignored.|Yes|
|DirectoryPropertyComparingModes|[StringPropertyComparingModes](../../type/StringPropertyComparingModes.md)|The ways to use Directory in condition. Default value is ```Contains```.|Yes|
|IsSubDirectoryIncluded|bool?|Filters records by whether sub directory is included.|Yes|

From Pipeline: Directory

Conditions will be ignored if not provided or set as null, unless a default value is specified above.

# Return
The list of the records which matches the conditions provided.

Type: List<[ImageStoreIgnoredDirectory](../../type/ImageStoreIgnoredDirectory.md)>

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Ignored Directory Cmdlets](../cmdlets.md#ignored-directory)