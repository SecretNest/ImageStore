# Remove-ImageStoreSameFile
Removes the same file record specified.

Alias: RemoveSameFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|SameFile|[ImageStoreSameFile](../../type/ImageStoreSameFile.md)|Entity to be removed|No(*1)|
|Id|Guid|Id of the record to be removed|No(*1)|

From Pipeline: SameFile, Id

*1: One and only one parameter within this group should be provided.

# Return
None.

# See also
  * [Concept: Same File](../../concept/SameFile.md)
  * [Same File Cmdlets](../cmdlets.md#same-file)