# Difference Degree

Difference degree is calculated based on result of GetCrossCorrelation method of priHash.
The value used in ImageStore is set to "1 - cross correlation". The value range is [0, 1]. The smaller the value, the more similar the two files are.

The value is not linear based, which means it's NOT the percentage of the difference between two images. Usually, if the value larger than 0.03, in most case these two files are not the similar for human being. But it still has a chance that they are.
This problem is brought from pHash algorithm, thus I cannot fix it -- if I could, you will not realize it exists.

You need to specify difference degree while calling [CompareSimilarFile](../cmdlet/SimilarFile/CompareSimilarFile.md) and [ResolveSimilarFile](../cmdlet/SimilarFile/ResolveSimilarFile.md), or a default value will be used. The default difference degree for CompareSimilarFile is 0.05, the one for ResolveSimilarFile is the smallest one among all files.
