# ImageStoreSimilarFile
Represents a [similar relationship](../concept/SimilarFile.md) between two image files.

Namespace: SecretNest.ImageStore.SimilarFile

# Properties
|Name|Type|Description|ReadOnly|
|---|---|---|---|
|Id|Guid|Record id|Yes|
|File1Id|Guid|Record id of one [file](ImageStoreFile.md)|Yes|
|File2Id|Guid|Record id of another [file](ImageStoreFile.md)|Yes|
|DifferenceDegree|float|[Difference degree](../concept/DifferenceDegree.md)|Yes|
|IgnoredMode|[IgnoredMode](#Ignored-Mode)|State of this relationship|No|

# Ignored Mode
Enum: SecretNest.ImageStore.SimilarFile.IgnoredMode

|Member|Value|Description|
|---|---|---|
|Normal|0|Normal status, not be set as ignored.|
|HiddenButConnected|1|Hidden but still connects the files as one group. Usually used when files are similar but want to be kept both.|
|HiddenAndDisconnected|2|Hidden and disconnects those two from one group. Usually used when files are not similar at all.|

# Cmdlets
  * [Get-ImageStoreSimilarFile](../cmdlet/SimilarFile/GetSimilarFile.md)
  * [Hide-ImageStoreSimilarFile](../cmdlet/SimilarFile/HideSimilarFile.md)
  * [Remove-ImageStoreSimilarFile](../cmdlet/SimilarFile/RemoveSimilarFile.md)
  * [Search-ImageStoreSimilarFile](../cmdlet/SimilarFile/SearchSimilarFile.md)
  * [Show-ImageStoreSimilarFile](../cmdlet/SimilarFile/ShowSimilarFile.md)
  * [Update-ImageStoreSimilarFile](../cmdlet/SimilarFile/UpdateSimilarFile.md)