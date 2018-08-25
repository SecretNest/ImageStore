# StringPropertyComparingModes
The way to use the string provided as search condition.

*Note: If the string related is provided with empty string (not null), comparing mode will always use equal no matter how comparing mode is set.*

*Note: This is a flag enum. Use ```Or``` to link multiple comparing modes you need. With multiple modes chosen, records matched with any mode provided will be included.*

Namespace SecretNest.ImageStore

# Elements
|Element|Value|Description|
|---|---|---|
|Equals|1|The property of the record should match the string.|
|Contains|2|The property of the record should contain the string.|
|StartsWith|4|The property of the record should be started with the string.|
|EndsWith|8|The property of the record should be ended with the string.|

Upper and lower case is ignored in condition matching.

# Cmdlets
  * [Search-ImageStoreFolder](../cmdlet/Folder/SearchFolder.md)
  * [Search-ImageStoreExtension](../cmdlet/Extension/SearchExtension.md)
  * [Search-ImageStoreIgnoredDirectory](../cmdlet/IgnoredDirectory/SearchIgnoredDirectory.md)
  * [Search-ImageStoreFile](../cmdlet/File/SearchFile.md)