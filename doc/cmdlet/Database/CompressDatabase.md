# Compress-ImageStoreDatabase

Runs VACUUM in the connected database, rebuilding the file to reclaim the space left by deleted records.

Alias: ShrinkDatabase, CompressDatabase, Shrink-ImageStoreDatabase

# Remarks
Requires free disk space roughly equal to the size of the database while it runs, since VACUUM rebuilds the file.

# Parameters
None

# Return
None

# See also
  * [Concept: Database](../../concept/Database.md)
  * [Database Cmdlets](../cmdlets.md#database)
  * [Database Preparation](../../../README.md#database)