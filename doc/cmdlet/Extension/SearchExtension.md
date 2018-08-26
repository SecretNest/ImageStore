# Search-ImageStoreExtension
Searches all records of extensions matched with the conditions provided.

Alias: SearchExtension

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Extension|string|Filters records by the name of the extension. Empty string is acceptable for the kind of file without extension.|Yes|
|IsImage|bool?|Filters records by whether the extension is marked as image.|Yes|
|Ignored|bool?|Filters records by whether the extension is marked as ignored.|Yes|

From Pipeline: Extension

Conditions will be ignored if not provided or set as null.

# Return
The list of the records which matches the conditions provided.

Type: List<[ImageStoreExtension](../../type/ImageStoreExtension.md)>

# See also
  * [Concept: Extension](../../concept/Extension.md)
  * [Extension Cmdlets](../cmdlets.md#extension)