# Clear-ImageStoreEmptyFolders
Clears empty directories from file system within the folder specified.

This cmdlet will search all empty directories within the folder specified, and remove them.

*Note: IgnoredDirectory setting will not be referenced. This cmdlet will find directories only based on file system concern.*

Alias: ClearEmptyFolders

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Folder|[ImageStoreFolder](../../type/ImageStoreFolder.md)|The root folder to search within|No|
|OverrideSealedFolder|*switch*|Overrides the IsSealed mark of the folder.|-|

From Pipeline: Folder

# Return
None.

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Folder Cmdlets](../cmdlets.md#folder)