# Add-ImageStoreIgnoredDirectory
Creates and adds an ignored directory record to database.

Alias: AddIgnoredDirectory

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|FolderId|Guid|Id of the Folder related|No|
|Directory|string|Path to be excluded from the Folder. Empty string means the root directory of the Folder specified.|No|
|IsSubDirectoryIncluded|bool|Whether sub directories should be excluded also. Default value is ```true```.|Yes|

# Return
The record of newly created ignored directory.

Type: [ImageStoreIgnoredDirectory](../../type/ImageStoreIgnoredDirectory.md)

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Ignored Directory Cmdlets](../cmdlets.md#ignored-directory)