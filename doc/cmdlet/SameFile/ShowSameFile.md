# Show-ImageStoreSameFile
Marks the record of same file as effective, not ignored.

Alias: ShowSameFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|SameFile|[ImageStoreSameFile](../../type/ImageStoreSameFile.md)|Entity to be marked as effective|No(*1)|
|Id|Guid|Id of the record to be marked as effective|No(*1)|

From Pipeline: SameFile, Id

*1: One and only one parameter within this group should be provided.

# Return
None.

# See also
  * [Concept: Same File](../../concept/SameFile.md)
  * [Same File Cmdlets](../cmdlets.md#same-file)