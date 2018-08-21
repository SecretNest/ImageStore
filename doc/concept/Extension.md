# Extension
ImageStore will deal with different type of files separately. Files will be grouped by using extension name.

You need to create records for all extensions need to be processed, one for each.

Property IsImage is used to decide whether the image hashing and comparing need to be done for this kind of files. It should be set to true for all image extensions. If it set to false, image hashing will be skipped but [data based comparation for checking exactly same files](SameFile.md) will still be processed.

Property Ignored is used to set this kind of file ignored for all comparation.

The extension names found in files but not having a related extension setting will be treated as ignored.

# Cmdlets
To modifying Extensions, you need to call [Extension Cmdlets](../cmdlet/cmdlets.md#extension).
