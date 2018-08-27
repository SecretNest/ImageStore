# Similar File
ImageStore use pHash to find the similar image files.

To detect similar files, first add all files being checked to database, second call [Measure-ImageStoreFile](../cmdlet/File/MeasureFile.md) or [Measure-ImageStoreFiles](../cmdlet/File/MeasureFiles.md) to calculate Sha1 hash, then call [Compare-ImageStoreSimilarFiles](../cmdlet/SimilarFile/CompareSimilarFiles.md) to find all similar relationships.

[Resolve-ImageStoreSimilarFiles](../cmdlet/SimilarFile/ResolveSimilarFiles.md) will group the similar relationships and display them with a UI.

**Note**: [Thumbprint cache](ThumbprintCache.md) may be useful while you calling Resolve-ImageStoreSimilarFiles repeatedly.

# Similar File Group
While running [Resolve-ImageStoreSimilarFiles](SimilarFile/ResolveSimilarFiles.md), all relations which have a [difference degree](DifferenceDegree.md) not larger than the parameter specified will be used to group files into similar file groups.

For example, there are 6 files: A, B, C, D, E and F. The difference degree between A and B is 0.01, between B and C is 0.02, between A and C is 0.1, between C and D is 0.5, between E and F is 0.2.
While running Resolve-ImageStoreSimilarFiles with parameter DifferenceDegree set as 0.02, there will be only one group founded, which contains A, B and C. Though the relation between A and C are not considered, but the relations of B connected them; if the parameter DifferenceDegree set as 0.2, there will be one more group found also, which contains E and F.
 
# Ignored
There are 2 types of the ignored states:
  * If the similar relation is correct, but you want to hide this relation from further processing, choose: Hidden but Connected.
  * If the similar relation is not correct, choose: Hidden and Disconnected.

The different between this 2 modes, is whether the connection is still valid.

For example, there are 4 files A, B, C and D are connected by effective similar file relations A-B, B-C, C-D. If the record of B-C is set to Hidden but Connected or not ignored, these 4 files are still will be grouped into one similar file group next time. But if it's set to Hidden and Disconnected, in next time grouping, there will be 2 groups contains A and B, C and D, instead of one.

# Remove Similar Files
All files checked in Resolve-ImageStoreSimilarFiles](SimilarFile/ResolveSimilarFiles.md) will be returned. Combines the result with [Remove-ImageStoreFile](../cmdlet/File/RemoveFile.md) to delete the files chosen.

# Cmdlets
  * To check or modify Similar Files, you need to call [Similar File Cmdlets](../cmdlet/cmdlets.md#similar-file).