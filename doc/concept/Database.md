# Database
ImageStore need a database hosted in Sql Server 2017 to store data generated from file and settings. See [Database section](../../README.md#database) of readme file for details.

Each project should have a dedicated database. For switching project, you need to change the connection string pointed to other database.

You should prepare a database by copying [DataStore.mdf](../../ImageStore/DataStore.mdf) and [DataStore_log.ldf](../../ImageStore/DataStore_log.ldf) to your Sql Server data folder or any folder for storing database; attach it to your database instance and get the connection string, or leave it for working as attached file mode which is recommended for LocalDb and Express.

# Cmdlets
  * After database and connection string prepared, you could use it by [Database Cmdlets](../cmdlet/cmdlets.md#database).
