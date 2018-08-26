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
|[Search-ImageStoreFolder](Folder/SearchFolder.md)|SearchFolder|Searches all folders records matched with the conditions provided.|
|[Sync-ImageStoreFolder](Folder/SyncFolder.md)|SyncFolder|Syncs the database to make records consistent with file system.|
|[Update-ImageStoreFolder](Folder/UpdateFolder.md)|UpdateFolder|Updates the folder record by the entity provided.|

# Extension
Manages [extensions](../concept/Extension.md).

|Command|Alias|Description|
|---|---|---|
|[Add-ImageStoreExtension](Extension/AddExtension.md)|AddExtension|Creates and adds an extension to database.|
|[Find-ImageStoreExtension](Extension/FindExtension.md)|FindExtension|Finds the record related to the extension by name.|
|[Get-ImageStoreExtension](Extension/GetExtension.md)|GetExtension|Gets the record by id.|
|[Remove-ImageStoreExtension](Extension/RemoveExtension.md)|RemoveExtension|Removes the extension record specified from database.|
|[Search-ImageStoreExtension](Extension/SearchExtension.md)|SearchExtension|Searches all extensions records matched with the conditions provided.|
|[Update-ImageStoreExtension](Extension/UpdateExtension.md)|UpdateExtension|Updates the extension record by the entity provided.|

# Ignored Directory
Manages the directories excluded from folders.

|Command|Alias|Description|
|---|---|---|
|[Add-ImageStoreIgnoredDirectory](IgnoredDirectory/AddIgnoredDirectory.md)|AddIgnoredDirectory|Creates and adds an ignored directory record to database.|
|[Find-ImageStoreIgnoredDirectory](IgnoredDirectory/FindIgnoredDirectory.md)|FindIgnoredDirectory|Finds the record related to the ignored directory by all properties.|
|[Get-ImageStoreIgnoredDirectory](IgnoredDirectory/GetIgnoredDirectory.md)|GetIgnoredDirectory|Gets the record of ignored directory by id.|
|[Remove-ImageStoreIgnoredDirectory](IgnoredDirectory/RemoveIgnoredDirectory.md)|RemoveIgnoredDirectory|Removes the ignored directory record specified from database.|
|[Search-ImageStoreIgnoredDirectory](IgnoredDirectory/SearchIgnoredDirectory.md)|SearchIgnoredDirectory|Searches all ignored directory records matched with the conditions provided.|
|[Update-ImageStoreIgnoredDirectory](IgnoredDirectory/UpdateIgnoredDirectory.md)|UpdateIgnoredDirectory|Updates the ignored directory record by the entity provided.|

# File
Manages [files](../concept/File.md) and computes hashing results.

|Command|Alias|Description|
|---|---|---|
|[Add-ImageStoreFile](File/AddFile.md)|AddFile|Creates and adds a file record to database.|
|[Find-ImageStoreFile](File/FindFile.md)|FindFolder|Finds the record related to the file by folder, path and file name.|
|[Get-ImageStoreFile](File/GetFile.md)|GetFile|Gets the record related to the file by id.|
|[Measure-ImageStoreFile](File/MeasureFile.md)|MeasureFile|Computes hashing result of the file specified.|
|[Measure-ImageStoreFiles](File/MeasureFiles.md)|MeasureFiles|Computes hashing result of the files within the folder specified.|
|[Move-ImageStoreFile](File/MoveFile.md)|MoveFile|Moves the file record specified and the pointed file to the target folder and directory.|
|[Remove-ImageStoreDirectory](File/RemoveDirectory.md)|RemoveDirectory|Removes the directory specified, including all files and sub directories, from database and file system.|
|[Remove-ImageStoreFile](File/RemoveFile.md)|RemoveFile|Removes the file record specified and the pointed file.|
|[Rename-ImageStoreFile](File/RenameFile.md)|RenameFile|Renames the file record specified and the pointed file.|
|[Resolve-ImageStoreFile](File/ResolveFile.md)|ResolveFile|Gets the full path of the file specified.|
|[Search-ImageStoreFile](File/SearchFile.md)|SearchFile|Searches all files records matched with the conditions provided.|
|[Update-ImageStoreFile](file/UpdateFile.md)|UpdateFile|Updates the file record by the entity provided.|

# Same File

# Similar File
