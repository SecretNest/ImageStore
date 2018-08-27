# Show-ImageStoreSimilarFile
Marks the record of similar file relation as effective, not ignored.

Alias: ShowSimilarFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|SimilarFile|[ImageStoreSimilarFile](../../type/ImageStoreSimilarFile.md)|Entity to be marked as effective|No(*1)|
|Id|Guid|Id of the record to be marked as effective|No(*1)|

From Pipeline: SimilarFile, Id

*1: One and only one parameter within this group should be provided.

# Return
None.

# See also
  * [Concept: Similar File](../../concept/SimilarFile.md)
  * [Similar File Cmdlets](../cmdlets.md#similar-file)