# Open-ImageStoreDatabase

Opens an existing SQLite database file.

*Note: A database has to be opened before other operations.*

*Note: To create a new one, use [New-ImageStoreDatabase](NewDatabase.md), which opens it as well.*

Alias: OpenDatabase

# Parameters
## Parameter set: Path (default)
|Name|Type|Description|Optional|
|---|---|---|---|
|Path|string|Path of the database file. Relative paths are resolved against the current PowerShell location.|No|

Opening a path that does not exist is an error. SQLite would otherwise create an empty file, which looks like it worked until the first query fails.

## Parameter set: ConnectionString
|Name|Type|Description|Optional|
|---|---|---|---|
|ConnectionString|string|A SQLite connection string, for settings a path alone cannot express.|No|

For example, to open a library without being able to change it:
```
Open-ImageStoreDatabase -ConnectionString "Data Source=D:\Library\library.db;Mode=ReadOnly"
```

FromPipeline: Path, ConnectionString

# Return
None.

# See also
  * [Concept: Database](../../concept/Database.md)
  * [New-ImageStoreDatabase](NewDatabase.md)
  * [Database Cmdlets](../cmdlets.md#database)
  * [Database Preparation](../../../README.md#database)
