# Clear-ImageStoreSameFileObsoletedGroups
ClearSameFileObsoletedGroups|Clears same file groups which have only one record left.

When generating same file records, record will only be created while at least 2 files are the same. But after file or same file deleted, there may be only 1 same file record left in the group. These obsoleted records will be ignored while running [Select-ImageStoreSameFile](SelectSameFile.md) but you can still remove them automatically by running this cmdlet. And it's recommended for removing useless records like these.

Alias: ClearSameFileObsoletedGroups

# Parameters
None.

# Return
None.

# See also
  * [Concept: Same File](../../concept/SameFile.md)
  * [Same File Cmdlets](../cmdlets.md#same-file)