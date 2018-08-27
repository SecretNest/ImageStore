# Hide-ImageStoreSimilarFile
Marks the record of similar file relation as ignored.

Alias: HideSimilarFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|SimilarFile|[ImageStoreSimilarFile](../../type/ImageStoreSimilarFile.md)|Entity to be marked as ignored|No(*1)|
|Id|Guid|Id of the record to be marked as ignored|No(*1)|
|MakeDisconnected|bool|Whether the relation should be marked as disconnected. See [Ignored section](../../concept/SimilarFile.md#Ignored) for details.|No|

From Pipeline: SimilarFile, Id

*1: One and only one parameter within this group should be provided.

# Return
None.

# See also
  * [Concept: Similar File](../../concept/SimilarFile.md)
  * [Similar File Cmdlets](../cmdlets.md#similar-file)