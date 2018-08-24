# Search-ImageStoreFolder
Searches all records of folders which matches the conditions provided.

Alias: SearchFolder

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Name|string|Uses name property as an condition to search.|Yes|
|NamePropertyComparingModes|[StringPropertyComparingModes](../../type/StringPropertyComparingModes.md)|The ways to use Name in condition. Default value is ```Contains```.|Yes|
|Path|string|Uses path property as an condition to search.|Yes|
|PathPropertyComparingModes|[StringPropertyComparingModes](../../type/StringPropertyComparingModes.md)|The ways to use Path in condition. Default value is ```Contains```.|Yes|
|CompareImageWith|[CompareImageWith](../../type/ImageStoreFolder.md#compare-image-with)|Uses CompareImageWith property as an condition to search.|Yes|
|IsSealed|bool?|Uses IsSealed property as an condition to search.|Yes|

Conditions will be ignored if not provided or set as null, unless a default value is specified above.

# Return
The list of the records which matches the conditions provided.

Type: List<[ImageStoreFolder](../../type/ImageStoreFolder.md)>

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Folder Cmdlets](../cmdlets.md#folder)