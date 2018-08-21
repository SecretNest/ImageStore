# Difference Degree

Difference degree is calculated based on result of GetCrossCorrelation method of priHash.
The value used in ImageStore is set to "1 - cross correlation". The value range is [0, 1]. The smaller the value, the more similar the two files are.

The value is not linear based, which means it's NOT the percentage of the difference between two images. Usually, if the value larger than 0.03, in most case these two files are not the similar for human being. But it still has a chance that they are.
This problem is brought from pHash algorithm, thus I cannot fix it -- if I could, you will not realize it exists.
