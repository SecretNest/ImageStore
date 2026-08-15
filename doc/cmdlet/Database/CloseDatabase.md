# Close-ImageStoreDatabase

Closes the connection to the database.

*Note: After the connection closed, most cmdlets cannot be performed correctly.*

*Note: You should close the database before moving its file if you want to keep the PowerShell instance running.*

*Note: The database is also closed automatically when the module is removed and when PowerShell exits, so the ```-wal``` and ```-shm``` files are not left beside it.*

Alias: CloseDatabase

# Parameters
None

# Return
None

# See also
  * [Concept: Database](../../concept/Database.md)
  * [Database Cmdlets](../cmdlets.md#database)
  * [Database Preparation](../../../README.md#database)