# Add-ImageStoreFolder
Creates and adds a folder record to database.

Alias: AddFolder

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Path|string|Path of the root directory of this folder|No|
|Name|string|Name of this folder. Default value is the same as Path.|Yes|
|CompareImageWith|[CompareImageWith](#compare-image-with)|The scope of the target files to be compared with for each file located in this folder. Default value is ```All```.|Yes|
|IsSealed|bool|Whether this folder should be read-only. Default value is ```false```.|Yes|

From Pipeline: Path

# Return
The record of newly created folder.

Type: [ImageStoreFolder](../../type/ImageStoreFolder.md)

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Folder Cmdlets](../cmdlets.md#folder)