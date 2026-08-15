# New-ImageStoreDatabase
Creates a SQLite database file, builds the schema in it, and opens it.

Alias: NewDatabase

The new database is left open, so this stands in for a following [Open-ImageStoreDatabase](OpenDatabase.md).

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Path|string|Path of the database file to create. Relative paths are resolved against the current PowerShell location.|No|
|Force|switch|Replaces the file if it exists already, along with its ```-wal```, ```-shm``` and ```-journal``` companions. Without this, an existing file is an error.|Yes|

FromPipeline: Path

# Return
None.

# Remarks
The database file should be on a local disk. See [Database](../../concept/Database.md) for why.

The directory has to exist; it is not created.

*Note: ```-Force``` deletes the existing file. If it is a library rather than a leftover, there is nothing to undo.*

# See also
  * [Concept: Database](../../concept/Database.md)
  * [Open-ImageStoreDatabase](OpenDatabase.md)
  * [Database Cmdlets](../cmdlets.md#database)
