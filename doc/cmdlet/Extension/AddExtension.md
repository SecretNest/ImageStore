# Add-ImageStoreExtension
Creates and adds an extension to database.

Alias: AddExtension

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Extension|string|Extension name of this kind of file. Empty string is acceptable for the kind of file without extension.|No|
|IsImage|bool|Whether this kind of file is image. Default value is ```true```.|Yes|
|Ignored|bool|Whether this kind of file should be ignored. Default value is ```false```.|Yes|

FromPipeline: Extension

# Return
The record of newly created extension.

Type: [ImageStoreExtension](../../type/ImageStoreExtension.md)

# See also
  * [Concept: Extension](../../concept/Extension.md)
  * [Extension Cmdlets](../cmdlets.md#extension)