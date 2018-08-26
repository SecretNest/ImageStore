# ImageStoreSameFile
Represents a [record](../concept/SameFile.md) that a file detected to be the same as at least one other file.

Namespace: SecretNest.ImageStore.SameFile

# Properties
|Name|Type|Description|ReadOnly|
|---|---|---|---|
|Id|Guid|Record id|Yes|
|Sha1Hash|byte[]|Sha1 hashing result of this file|Yes|
|FileId|Guid|Record id of the [file](ImageStoreFile.md) related|Yes|
|IsIgnored|bool|Whether this record is ignored or not|No|

*Note: Ignored record will not be selected by [Select-ImageStoreSameFile](SelectSameFile.md) automatically. If there is only one file left as not ignored in one same file group, the only file will not be selected also.*

# Cmdlets
  * [Get-ImageStoreSameFile](../cmdlet/SameFile/GetSameFile.md)
  * [Hide-ImageStoreSameFile](../cmdlet/SameFile/HideSameFile.md)
  * [Remove-ImageStoreSameFile](../cmdlet/SameFile/RemoveSameFile.md)
  * [Search-ImageStoreSameFile](../cmdlet/SameFile/SearchSameFile.md)
  * [Select-ImageStoreSameFile](../cmdlet/SameFile/SelectSameFile.md)
  * [Show-ImageStoreSameFile](../cmdlet/SameFile/ShowSameFile.md)
  * [Update-ImageStoreSameFile](../cmdlet/SameFile/UpdateSameFile.md)
