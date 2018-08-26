# Search-ImageStoreFolder
Searches all records of folders matched with the conditions provided.

Alias: SearchFolder

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Name|string|Filters records by name.|Yes|
|NamePropertyComparingModes|[StringPropertyComparingModes](../../type/StringPropertyComparingModes.md)|The ways to use Name in condition. Default value is ```Contains```.|Yes|
|Path|string|Filters records by the path of the root directory.|Yes|
|PathPropertyComparingModes|[StringPropertyComparingModes](../../type/StringPropertyComparingModes.md)|The ways to use Path in condition. Default value is ```Contains```.|Yes|
|CompareImageWith|[CompareImageWith](../../type/ImageStoreFolder.md#compare-image-with)|Filters records by the scope of the target files to be compared with for each file located in this folder.|Yes|
|IsSealed|bool?|Filters records by whether the folder is set to sealed.|Yes|

From Pipeline: Name

Conditions will be ignored if not provided or set as null, unless a default value is specified above.

# Return
The list of the records which matches the conditions provided.

Type: List<[ImageStoreFolder](../../type/ImageStoreFolder.md)>

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Folder Cmdlets](../cmdlets.md#folder)