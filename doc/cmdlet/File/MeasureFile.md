# Measure-ImageStoreFile
Computes hashing result of the file specified.

Alias: MeasureFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|File|[ImageStoreFile](../../type/ImageStoreFile.md)|The file to be computed.|No|
|Recompute|*switch*|Compute the file even the state of the file is not new created. (*1)|-|

From Pipeline: FileName

*1: Hashing result will be recomputed when this parameter specified. After recomputed, the records related hashing result of this file will be deleted.

# Return
The record of hashed file.

Type: [ImageStoreFile](../../type/ImageStoreFile.md)

# See also
  * [Concept: File](../../concept/File.md)
  * [File Cmdlets](../cmdlets.md#file)