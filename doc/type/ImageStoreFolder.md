# ImageStoreFolder
Represents a [folder](../concept/Folder.md) for storing image files.

Namespace: SecretNest.ImageStore.Folder

# Properties
|Name|Type|Description|ReadOnly|
|---|---|---|---|
|Id|Guid|Record id|Yes|
|Name|string|Name of this folder|No|
|Path|string|Path of the root directory of this folder|No|
|CompareImageWith|[CompareImageWith](#compare-image-with)|The scope of the target files to be compared with for each file located in this folder|No|
|IsSealed|bool|Whether this folder should be read-only|No|

# Compare Image With
Enum: SecretNest.ImageStore.Folder.CompareImageWith

|Member|Value|Description|
|---|---|---|
|All|0|The files in this folder will be compared with all files.|
|FilesInOtherDirectories|1|The files in this folder will be compared with all files other than those from same directory (sub directories are not included).|
|FilesInOtherFolders|2|The files in this folder will be compared with all files other than those in this folder.|

# Cmdlets
  * [Add-ImageStoreFolder](../cmdlet/Folder/AddFolder.md)
  * [Find-ImageStoreFolder](../cmdlet/Folder/FindFolder.md)
  * [Get-ImageStoreFolder](../cmdlet/Folder/GetFolder.md)
  * [Remove-ImageStoreFolder](../cmdlet/Folder/RemoveFolder.md)
  * [Search-ImageStoreFolder](../cmdlet/Folder/SearchFolder.md)
  * [Sync-ImageStoreFolder](../cmdlet/Folder/SyncFolder.md)
  * [Update-ImageStoreFolder](../cmdlet/Folder/UpdateFolder.md)
  * [Clear-ImageStoreEmptyFolders](../cmdlet/Folder/ClearEmptyFolders.md)
  * [Add-ImageStoreFile](../cmdlet/File/AddFile.md)
  * [Measure-ImageStoreFiles](../cmdlet/File/MeasureFiles.md)
  * [Move-ImageStoreFile](../cmdlet/File/MoveFile.md)
  * [Remove-ImageStoreDirectory](../cmdlet/File/RemoveDirectory.md)
  * [Resolve-ImageStoreSimilarFiles](../cmdlet/SimilarFile/ResolveSimilarFiles.md) 
  * [Select-ImageStoreSameFile](../cmdlet/SameFile/SelectSameFile.md)