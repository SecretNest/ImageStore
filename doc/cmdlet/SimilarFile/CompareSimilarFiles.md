# Compare-ImageStoreSimilarFiles
Compares image hashing result and generates similar file relation records.

This cmdlet compares files to each other, generates relation records if the different degree is less than the threshold specified, and updates ImageComparedThreshold property of file records.

*Note: This cmdlet may cost **SEVERAL HOURS EVEN DAYS** to run. You may want to [Enable Information and Verbose Output](../../../README.md#enable-information-and-verbose-output) to view progress reporting.*

*Note: There will be a warning message about time cost shown while starting this cmdlet. If you don't want to see it, use SuppressTimeWarning switch.*

Alias: CompareSimilarFiles

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|ImageComparedThreshold|float|The maximum difference degree should be recorded. Default value is 0.05.|Yes|
|ComparingThreadLimit|int|The count of thread will be created for comparing image hashing. Default value will be set as the count of the logical processors installed in this computer. Set to ```-1``` to remove this limit.|Yes|
|SuppressTimeWarning|*switch*|Disables the warning about time cost.|Yes|

From Pipeline: ImageComparedThreshold

# Return
None.

# See also
  * [Concept: Similar File](../../concept/SimilarFile.md)
  * [Similar File Cmdlets](../cmdlets.md#similar-file)