# Search-ImageStoreExtension
Searches all extensions records which matches the conditions provided.

Alias: SearchExtension

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Extension|string|Uses this extension name as an condition to search. Empty string is acceptable for the kind of file without extension.|Yes|
|IsImage|bool?|Uses whether this kind of file is image as an condition to search.|Yes|
|Ignored|bool?|Uses whether this kind of file should be ignored as an condition to search.|Yes|

Conditions will be ignored if not provided or set as null.

# Return
The list of the records which matches the conditions provided.

Type: List<[ImageStoreExtension](../../type/ImageStoreExtension.md)>

# See also
  * [Concept: Extension](../../concept/Extension.md)
  * [Extension Cmdlets](../cmdlets.md#extension)