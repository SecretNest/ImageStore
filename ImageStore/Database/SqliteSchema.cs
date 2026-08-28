using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.Database
{
    /// <summary>
    /// The database schema, translated from the Sql Server script this project used
    /// before SQLite. Single source of truth: the migration tool compiles this same
    /// file by link, so a new database and a migrated one cannot drift apart.
    /// </summary>
    /// <remarks>
    /// Type mapping from the original:
    /// uniqueidentifier -&gt; BLOB, holding the 16 bytes of Guid.ToByteArray().
    /// bit              -&gt; INTEGER, 0 or 1.
    /// real             -&gt; REAL. SQLite stores 8 bytes where Sql Server stored 4,
    ///                     which only widens the value.
    /// binary(n)        -&gt; BLOB.
    /// nvarchar(256)    -&gt; TEXT COLLATE NOCASE.
    ///
    /// The NOCASE collation keeps path and name comparison case-insensitive, matching
    /// both Sql Server's default collation and the Windows file system. Note that
    /// SQLite's NOCASE only folds ASCII A-Z, so accented characters compare exactly;
    /// this is a real difference from Sql Server for non-English file names.
    ///
    /// Cascades are reproduced exactly as they were, including the asymmetry:
    /// File, IgnoredDirectory and SameFile cascade from their parent, but SimilarFile
    /// does not. Code that deletes files therefore has to clear SimilarFile rows
    /// itself - see FileHelper - and that stays true here.
    /// </remarks>
    static class SqliteSchema
    {
        /// <summary>
        /// Statements to create an empty database, in dependency order.
        /// </summary>
        internal static IReadOnlyList<string> CreationStatements { get; } = new[]
        {
            @"CREATE TABLE [Folder](
    [Id] BLOB NOT NULL PRIMARY KEY,
    [Name] TEXT NOT NULL COLLATE NOCASE,
    [Path] TEXT NOT NULL COLLATE NOCASE,
    [CompareImageWith] INTEGER NOT NULL,
    [IsSealed] INTEGER NOT NULL)",

            @"CREATE UNIQUE INDEX [IX_Folder_Name] ON [Folder]([Name])",
            @"CREATE INDEX [IX_Folder_Path] ON [Folder]([Path])",

            @"CREATE TABLE [Extension](
    [Id] BLOB NOT NULL PRIMARY KEY,
    [Extension] TEXT NOT NULL COLLATE NOCASE,
    [IsImage] INTEGER NOT NULL,
    [Ignored] INTEGER NOT NULL)",

            @"CREATE UNIQUE INDEX [IX_Extension] ON [Extension]([Extension])",

            @"CREATE TABLE [File](
    [Id] BLOB NOT NULL PRIMARY KEY,
    [FolderId] BLOB NOT NULL REFERENCES [Folder]([Id]) ON DELETE CASCADE,
    [Path] TEXT NOT NULL COLLATE NOCASE,
    [FileName] TEXT NOT NULL COLLATE NOCASE,
    [ExtensionId] BLOB NOT NULL REFERENCES [Extension]([Id]) ON DELETE CASCADE,
    [ImageHash] BLOB NULL,
    [Sha1Hash] BLOB NULL,
    [FileSize] INTEGER NOT NULL DEFAULT (-1),
    [FileState] INTEGER NOT NULL,
    [ImageComparedThreshold] REAL NOT NULL DEFAULT (0))",

            @"CREATE INDEX [IX_File_FolderId] ON [File]([FolderId])",
            @"CREATE INDEX [IX_File_FolderIdPath] ON [File]([FolderId],[Path],[FileName],[ExtensionId],[FileState])",
            @"CREATE INDEX [IX_File_ForComparing] ON [File]([ImageComparedThreshold],[FileState])",

            @"CREATE TABLE [IgnoredDirectory](
    [Id] BLOB NOT NULL PRIMARY KEY,
    [FolderId] BLOB NOT NULL REFERENCES [Folder]([Id]) ON DELETE CASCADE,
    [Directory] TEXT NOT NULL COLLATE NOCASE,
    [IsSubDirectoryIncluded] INTEGER NOT NULL)",

            @"CREATE INDEX [IX_IgnoredDirectory_FolderId] ON [IgnoredDirectory]([FolderId])",
            @"CREATE UNIQUE INDEX [IX_IgnoredDirectory_FolderIdDirectory] ON [IgnoredDirectory]([FolderId],[Directory])",

            @"CREATE TABLE [SameFile](
    [Id] BLOB NOT NULL PRIMARY KEY,
    [Sha1Hash] BLOB NOT NULL,
    [FileId] BLOB NOT NULL REFERENCES [File]([Id]) ON DELETE CASCADE,
    [IsIgnored] INTEGER NOT NULL)",

            @"CREATE UNIQUE INDEX [IX_SameFile_FileId] ON [SameFile]([FileId])",
            @"CREATE INDEX [IX_SameFile_Sha1Hash] ON [SameFile]([Sha1Hash])",

            //No cascade on either foreign key, matching the original schema.
            @"CREATE TABLE [SimilarFile](
    [Id] BLOB NOT NULL PRIMARY KEY,
    [File1Id] BLOB NOT NULL REFERENCES [File]([Id]),
    [File2Id] BLOB NOT NULL REFERENCES [File]([Id]),
    [DifferenceDegree] REAL NOT NULL,
    [IgnoredMode] INTEGER NOT NULL)",

            @"CREATE INDEX [IX_SimilarFile] ON [SimilarFile]([DifferenceDegree])",
            @"CREATE INDEX [IX_SimilarFile_File1] ON [SimilarFile]([File1Id])",
            @"CREATE INDEX [IX_SimilarFile_File2] ON [SimilarFile]([File2Id])",

            //IgnoredMode first, DifferenceDegree second. Every review query filters on
            //both, and IgnoredMode is the selective one once a library has been worked
            //through - hiding pairs leaves nearly every row at HiddenAndDisconnected.
            //IX_SimilarFile alone cannot serve those: it locates the DifferenceDegree
            //range, but the range is most of the table, so each entry costs a random
            //row fetch just to read IgnoredMode and discard it. Measured on 2.26M rows
            //with two rows not hidden, Search-ImageStoreSimilarFile went from minutes
            //to instant. Order matters and cannot be reversed; DifferenceDegree first
            //degenerates to the same scan.
            @"CREATE INDEX [IX_SimilarFile_IgnoredMode_DifferenceDegree] ON [SimilarFile]([IgnoredMode],[DifferenceDegree])"
        };

        /// <summary>
        /// Names of every table, in an order safe for inserting with foreign keys on.
        /// Used by the migration tool.
        /// </summary>
        internal static IReadOnlyList<string> TablesInDependencyOrder { get; } = new[]
        {
            "Folder", "Extension", "File", "IgnoredDirectory", "SameFile", "SimilarFile"
        };
    }
}
