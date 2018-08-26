# Search-ImageStoreSameFile
Searches all same file records matched with the conditions provided.

Alias: SearchSameFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Sha1Hash|byte[]|Filters records by using file hashing result.|Yes|
|IncludeIgnored|*switch*|Includes records marked as ignored.|-|
|OnlyIgnored|*switch*|Returns only ignored records.|-|
|IncludeObsoleted|*switch*|Includes records from the same file groups which have only one record.|-|

From Pipeline: Sha1Hash

Conditions will be ignored if not provided or set as null.

# Return
The list of the records which matches the conditions provided.

Type: List<[ImageStoreSameFile](../../type/ImageStoreSameFile.md)>

# See also
  * [Concept: Same File](../../concept/SameFile.md)
  * [Same File Cmdlets](../cmdlets.md#same-file)