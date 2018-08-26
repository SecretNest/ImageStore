# Hide-ImageStoreSameFile
Marks the record of same file as ignored.

*Note: Ignored record will not be selected by [Select-ImageStoreSameFile](SelectSameFile.md) automatically. If there is only one file left as not ignored in one same file group, the only file will not be selected also.*

Alias: HideSameFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|SameFile|[ImageStoreSameFile](../../type/ImageStoreSameFile.md)|Entity to be marked as ignored|No(*1)|
|Id|Guid|Id of the record to be marked as ignored|No(*1)|

From Pipeline: SameFile, Id

*1: One and only one parameter within this group should be provided.

# Return
None.

# See also
  * [Concept: Same File](../../concept/SameFile.md)
  * [Same File Cmdlets](../cmdlets.md#same-file)