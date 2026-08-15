# Database
ImageStore keeps every record and setting in a single SQLite file. There is no server to install and no service to run.

Each project has its own file. Switching project means opening a different one.

# Creating and opening
[New-ImageStoreDatabase](../cmdlet/Database/NewDatabase.md) creates the file and opens it in one step, so it stands in for a following [Open-ImageStoreDatabase](../cmdlet/Database/OpenDatabase.md):
```
New-ImageStoreDatabase D:\Library\library.db
```
Later sessions open the file that already exists:
```
Open-ImageStoreDatabase D:\Library\library.db
```
Opening a path that does not exist is an error rather than a silent creation, so a typo cannot leave you working in an empty database.

# Where to put the file
**On a local disk.** SQLite coordinates access through file locking, and that is unreliable on SMB and other network filesystems; write-ahead logging is unavailable there entirely. A database on a network share risks corruption.

This does not restrict where the images live. ImageStore records paths, not image content, so a library on a NAS with its database on a local disk is a normal and safe arrangement.

# Closing
[Close-ImageStoreDatabase](../cmdlet/Database/CloseDatabase.md) closes the file. The module also closes it when the module is removed and when PowerShell exits, so the ```-wal``` and ```-shm``` files beside the database are cleaned up rather than left behind. If the process is killed outright neither runs, but SQLite recovers on the next open.

# Maintenance
[Compress-ImageStoreDatabase](../cmdlet/Database/CompressDatabase.md) runs ```VACUUM```, rebuilding the file to reclaim space left by deleted records. It needs free disk space roughly equal to the size of the database while it runs.

# Migrating from Sql Server
Releases up to and including [v2.0](https://github.com/SecretNest/ImageStore/releases/tag/v2.0) used Sql Server. The ```ImageStore-Migrator``` tool, published as a separate asset with each release, copies an existing library across:
```
ImageStore.Migrator --source "<sql server connection string>" --target D:\Library\library.db
```
It reads the old database without modifying it.

# Cmdlets
  * [Database Cmdlets](../cmdlet/cmdlets.md#database).
