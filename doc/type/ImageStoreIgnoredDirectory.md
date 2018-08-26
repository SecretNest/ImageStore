# ImageStoreIgnoredDirectory
Represents an exclusion a directory from a [folder](../concept/Folder.md).

Namespace: SecretNest.ImageStore.IgnoredDirectory

# Properties
|Name|Type|Description|ReadOnly|
|---|---|---|---|
|Id|Guid|Record id|Yes|
|FolderId|Guid|Record id of the [folder](ImageStoreFolder.md) related|No|
|Directory|string|Path of the directory to be excluded|No|
|IsSubDirectoryIncluded|bool|Whether sub directory should be included or not|No|

# Cmdlets
  * [Add-ImageStoreIgnoredDirectory](../cmdlet/IgnoredDirectory/AddIgnoredDirectory.md)
  * [Find-ImageStoreIgnoredDirectory](../cmdlet/IgnoredDirectory/FindIgnoredDirectory.md)
  * [Get-ImageStoreIgnoredDirectory](../cmdlet/IgnoredDirectory/GetIgnoredDirectory.md)
  * [Remove-ImageStoreIgnoredDirectory](../cmdlet/IgnoredDirectory/RemoveIgnoredDirectory.md)
  * [Search-ImageStoreIgnoredDirectory](../cmdlet/IgnoredDirectory/SearchIgnoredDirectory.md)
  * [Update-ImageStoreIgnoredDirectory](../cmdlet/IgnoredDirectory/UpdateIgnoredDirectory.md)
