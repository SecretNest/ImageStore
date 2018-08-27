# Search-ImageStoreFile
Searches all files records matched with the conditions provided.

Alias: SearchFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|FolderId|Guid?|Filters records by the folder where it's located.|Yes|
|Path|string|Filters records by the path to the directory where it's located. Filename is not included. Set to empty string to locate the files in the root directory of folders.|Yes|
|PathPropertyComparingModes|[StringPropertyComparingModes](../../type/StringPropertyComparingModes.md)|The ways to use Path in condition. Default value is ```Contains```.|Yes|
|FileName|string|Filters records by the file name. Extension is not included. Set to empty string to return the file without file name.|Yes|
|FileNamePropertyComparingModes|[StringPropertyComparingModes](../../type/StringPropertyComparingModes.md)|The ways to use FileName in condition. Default value is ```Contains```.|Yes|
|ExtensionId|Guid?|Filters by the id of the extension.|Yes|
|ImageHashIsNull|*switch*|Returns files which have no image hashing result.|Yes|
|ImageHash|byte[]|Filters records by image hashing result. Will be ignored when ImageHashIsNull is present.|Yes|
|Sha1HashIsNull|*switch*|Returns files which have no file hashing result.|Yes|
|Sha1Hash|byte[]|Filters records by file hashing result. Will be ignored when Sha1HashIsNull is present.|Yes|
|FileSize|int?|Filters records by the file size.|Yes|
|FileSizeGreaterOrEqual|int?|Returns files which are equal or larger than the size. Will be ignored when FileSize is present.|Yes|
|FileSizeLessOrEqual|int?|Returns files which are equal or smaller than the size. Will be ignored when FileSize is present.|Yes|
|FileState|[FileState](../../type/ImageStoreFile.md#file-state)?|Filters records by state.|Yes|
|ImageComparedThreshold|float?|Filters records by the maximum ImageComparedThreshold used in [Compare-ImageStoreSimilarFiles](../SimilarFile/CompareSimilarFiles.md) on this file.|Yes|
|ImageComparedThresholdGreaterOrEqual|float?|Returns files which ImageComparedThreshold are equal or greater than this value. Will be ignored when ImageComparedThreshold is present.|Yes|
|ImageComparedThresholdLessOrEqual|float?|Returns files which ImageComparedThreshold are equal or less than this value. Will be ignored when ImageComparedThreshold is present.|Yes|
|Top|int?|Limits the count of the results.|Yes|

From Pipeline: Path

Conditions will be ignored if not provided or set as null.

# Return
The list of the records which matches the conditions provided.

Type: List<[ImageStoreFile](../../type/ImageStoreFile.md)>

# See also
  * [Concept: File](../../concept/File.md)
  * [File Cmdlets](../cmdlets.md#file)