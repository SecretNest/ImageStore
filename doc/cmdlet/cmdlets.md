# Cmdlets

Here lists all cmdlets provided from ImageStore.

# Database
Operates on [database](../concept/database.md) connection or shrinks database.

|Command|Alias|Description|
|---|---|---|
|[Open-ImageStoreDatabase](Database/OpenDatabase.md)|OpenDatabase|Sets connection string and opens connection to the target database.|
|[Compress-ImageStoreDatabase](Database/CompressDatabase.md)|ShrinkDatabase, CompressDatabase, Shrink-ImageStoreDatabase|Runs ShrinkDatabase command in connected database to shrink the size of the data and log files.|
|[Close-ImageStoreDatabase](Database/CloseDatabase.md)|CloseDatabase|Closes the connection to the database.|

# Folder
Manages [folders](../concept/folder.md).

|Command|Alias|Description|
|---|---|---|
|[Add-ImageStoreFolder](Folder/AddFolder.md)|AddFolder|Creates and adds a folder record to database.|
|[Clear-ImageStoreEmptyFolders](Folder/ClearEmptyFolders.md)|ClearEmptyFolder|Clears empty directories from file system within the folder specified.|
|[Find-ImageStoreFolder](Folder/FindFolder.md)|FindFolder|Finds the record related to the folder by name.|
|[Get-ImageStoreFolder](Folder/GetFolder.md)|GetFolder|Gets the record related to the folder by id.|
|[Remove-ImageStoreFolder](Folder/RemoveFolder.md)|RemoveFolder|Removes the folder record specified from database.|
|[Search-ImageStoreFolder](Folder/SearchFolder.md)|SearchFolder|Searches all folders records which matches the conditions provided.|
|[Sync-ImageStoreFolder](Folder/SyncFolder.md)|SyncFolder|Syncs the database to make records consistent with file system.|
|[Update-ImageStoreFolder](Folder/UpdateFolder.md)|UpdateFolder|Updates the folder record by the entity provided.|

# Extension
Manages [extensions](../concept/extension.md).

|Command|Alias|Description|
|---|---|---|
|[Add-ImageStoreExtension](Extension/AddExtension.md)|AddExtension|Creates and adds an extension to database.|
|[Find-ImageStoreExtension](Extension/FindExtension.md)|FindExtension|Finds the record related to the extension by name.|
|[Get-ImageStoreExtension](Extension/GetExtension.md)|GetExtension|Gets the record by id.|
|[Remove-ImageStoreExtension](Extension/RemoveExtension.md)|RemoveExtension|Removes the extension record specified from database.|
|[Search-ImageStoreExtension](Extension/SearchExtension.md)|SearchExtension|Searches all extensions records which matches the conditions provided.|
|[Update-ImageStoreExtension](Extension/UpdateExtension.md)|UpdateExtension|Updates the extension record by the entity provided.|

# Ignored Directory
Manages the directories excluded from folders.

|Command|Alias|Description|
|---|---|---|
|[Add-ImageStoreIgnoredDirectory](IgnoredDirectory/AddIgnoredDirectory.md)|AddIgnoredDirectory|Creates and adds an ignored directory record to database.|
|[Find-ImageStoreIgnoredDirectory](IgnoredDirectory/FindIgnoredDirectory.md)|FindIgnoredDirectory|Finds the record related to the ignored directory by all properties.|
|[Get-ImageStoreIgnoredDirectory](IgnoredDirectory/GetIgnoredDirectory.md)|GetIgnoredDirectory|Gets the record of ignored directory by id.|
|[Remove-ImageStoreIgnoredDirectory](IgnoredDirectory/RemoveIgnoredDirectory.md)|RemoveIgnoredDirectory|Removes the ignored directory record specified from database.|
|[Search-ImageStoreIgnoredDirectory](IgnoredDirectory/SearchIgnoredDirectory.md)|SearchIgnoredDirectory|Searches all ignore directory records which matches the conditions provided.|
|[Update-ImageStoreIgnoredDirectory](IgnoredDirectory/UpdateIgnoredDirectory.md)|UpdateIgnoredDirectory|Updates the ignored directory record by the entity provided.|

# File

# Same File

# Similar File
