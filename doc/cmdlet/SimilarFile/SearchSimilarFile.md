# Search-ImageStoreSimilarFile
Searches all records of similar file relations matched with the condition provided.

Alias: SearchSimilarFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|FileId|Guid?|Filters records by related file id.|Yes(*1)|
|AnotherFileId|Guid?|Filters records by another related file id.|Yes(*1)|
|DifferenceDegree|float?|Filters records by the difference degree exactly.|Yes|
|DifferenceDegreeGreaterOrEqual|float?|Returns relations which are equal or larger than this difference degree. Will be ignored when DifferenceDegree is present.|Yes|
|DifferenceDegreeLessOrEqual|float?|Returns relations which are equal or smaller than this difference degree. Will be ignored when DifferenceDegree is present.|Yes|
|IncludesEffective|*switch*|Returns records with [IgnoreMode](../../type/ImageStoreSimilarFile.md#Ignored-mode) is set as Effective.|Yes(*2)|
|IncludesHiddenButConnected|*switch*|Returns records with [IgnoreMode](../../type/ImageStoreSimilarFile.md#Ignored-mode) is set as HiddenButConnected.|Yes(*2)|
|IncludesHiddenAndDisconnected|*switch*|Returns records with [IgnoreMode](../../type/ImageStoreSimilarFile.md#Ignored-mode) is set as HiddenAndDisconnected.|Yes(*2)|
|Top|int?|Limits the count of the results.|Yes|
|OrdersByDifferenceDegree|*switch*|Orders results by difference degree.|Yes|

From Pipeline: FileId

*1: When both present, only returns the record related between these 2 files. AnotherFileId will be ignored when FileId absent.
*2: When these 3 switches are all not set, only returns records with [IgnoreMode](../../type/ImageStoreSimilarFile.md#Ignored-mode) is set as Effective.

Conditions will be ignored if not provided or set as null.

# Ignored Modes
State of this record of file.

Enum: SecretNest.ImageStore.SimilarFile.IgnoredModes

|Element|Value|Description|
|---|---|---|
|Effective|1|Marked as effective, not ignored.|
|HiddenButConnected|2|Marked as Hidden but Connected.|
|HiddenAndDisconnected|4|Marked as Hidden and Disconnected.|
|All|7|All states|
|AllHidden|6|All hidden|
|AllConnected|3|All connected, no matter hidden or effective|

*Note: This is a flag enum. Use ```Or``` to link multiple comparing modes you need. With multiple modes chosen, records matched with any mode provided will be included.*

# Return
The list of the records which matches the conditions provided.

Type: List<[ImageStoreSimilarFile](../../type/ImageStoreSimilarFile.md)>

# See also
  * [Concept: Similar File](../../concept/SimilarFile.md)
  * [Similar File Cmdlets](../cmdlets.md#similar-file)