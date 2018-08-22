# File
In ImageStore, one File record represents one file in folders. It records the file path, name, size and hashing information.

You can manually add or remove record by using [File Cmdlets](../cmdlet/cmdlets.md#file). But the best way to maintain consistency between database records and file system is using [Sync-ImageStoreFolder](../cmdlet/Folder/SyncFolder.md) cmdlet.

To preserve data linked with files, when you need to move, rename or delete file synced to database, you may want to use related [File Cmdlets](../cmdlet/cmdlets.md#file) instead of operating in OS directly. Otherwise, though [Sync-ImageStoreFolder](../cmdlet/Folder/SyncFolder.md) can keep the consistence, the record id of changed files will be different than before, and all related data like hashing will not be kept.

# Cmdlets
  * To modify Files, you need to call [File Cmdlets](../cmdlet/cmdlets.md#file).
  * To make sure the database is consistent with file system, use [Sync-ImageStoreFolder](../cmdlet/Folder/SyncFolder.md) cmdlet.