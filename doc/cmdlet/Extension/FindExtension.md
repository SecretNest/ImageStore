# Find-ImageStoreExtension
Finds the record related to the extension by name.

Alias: FindExtension

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Extension|string|Extension name of this kind of file. Empty string is acceptable for the kind of file without extension.|No|

# Return
The record of the extension.

Type: [ImageStoreExtension](../../type/ImageStoreExtension.md)

Or, null, when no related record can be found.

# See also
  * [Concept: Extension](../../concept/Extension.md)
  * [Extension Cmdlets](../cmdlets.md#extension)