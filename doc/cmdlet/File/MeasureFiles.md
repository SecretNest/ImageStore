# Measure-ImageStoreFiles
Computes hashing result of the files within the folder specified.

*Note: This cmdlet will cost several minutes even hours to run. You may want to [Enable Information and Verbose Output](../../../README.md#enable-information-and-verbose-output) to view progress reporting.*

Alias: MeasureFiles

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Folder|[ImageStoreFolder](../../type/ImageStoreFolder.md)|The folder which contains the files to be computed. All files within all folders will be computed when this parameter absent.|Yes|
|RemeasuringFileStates|[RemeasuringFileStates](#remeasuring)|The kind of files to be recomputed. Default value is ```None```.|Yes|
|FileCountLimit|int?|Limits the count of files to be computed. Default value is ```null```, which means no limit of count will be applied.|Yes|
|ComputingThreadLimit|int|The count of thread will be created for computing hashing result. Default value will be the count of the logical processors installed in this computer. Set to -1 to remove this limit.|Yes|

# Remeasuring
Enum: SecretNest.ImageStore.File.RemeasuringFileStates

|Element|Value|Description|
|---|---|---|
|None|0|No file will be recomputed.|
|NotImage|1|The files which state marked as NotImage will be recomputed.|
|NotReadable|2|The files which state marked as NotReadable will be recomputed.|
|Computed|4|The files which state marked as Computed will be recomputed.|
|SizeZero|8|The files which size recorded as 0 will be recomputed.|
|AllFailed|3|The files which state marked as NotImage or NotReadable will be recomputed.|
|All|15|All files will be recomputed.|

*Note: This is a flag enum. Use ```Or``` to link multiple file states you need. With multiple states chosen, files matched with any state provided will be recomputed.*

*Note: After recomputed, the records related hashing result of this file will be deleted.*

# Return
None.

# See also
  * [Concept: File](../../concept/File.md)
  * [File Cmdlets](../cmdlets.md#file)