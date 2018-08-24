# Find-ImageStoreIgnoredDirectory
Finds the record related to the ignored directory by all properties.

Alias: FindIgnoredDirectory

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|FolderId|Guid|Id of the Folder related|No|
|Directory|string|Path to be excluded from the Folder. Empty string means the root directory of the Folder specified.|No|
|IsSubDirectoryIncluded|bool|Whether sub directories should be excluded also. Default value is ```true```.|No|

# Return
The record of the ignored directory.

Type: [ImageStoreIgnoredDirectory](../../type/ImageStoreIgnoredDirectory.md)

Or, null, when no related record can be found.

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Ignored Directory Cmdlets](../cmdlets.md#ignored-directory)