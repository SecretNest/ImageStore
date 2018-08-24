# Sync-ImageStoreFolder
Syncs the database to make records consistent with file system.

Running this cmdlet will add or remove related file records within the specified folder, or all folders when no folder is specified as parameter.

This cmdlet will only sync file system to database. No file in file system will be affected.

*Note: This cmdlet will cost several minutes to run. You may want to [Enable Information and Verbose Output](../../../README.md#enable-information-and-verbose-output) to view progress reporting.*

Alias: SyncFolder

# Parameters
|Name|Type|Description|Optional
|---|---|---|---|
|Folder|[ImageStoreFolder](../../type/ImageStoreFolder.md)|Folder to sync. All folders will be synced if this parameter is not specified.|Yes|
|OverrideSealedFolder|*switch*|Overrides the IsSealed mark of the folder.|-|

If Folder is not specified and OverrideSealedFolder is missing, all folders without IsSealed marked will be synced.

# Return
None.

# See also
  * [Concept: Folder](../../concept/Folder.md)
  * [Folder Cmdlets](../cmdlets.md#folder)