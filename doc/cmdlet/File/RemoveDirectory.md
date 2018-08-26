# Remove-ImageStoreDirectory
Removes the directory specified, including all files and sub directories, from database and file system.

Alias: RemoveDirectory

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Folder|[ImageStoreFolder](../../type/ImageStoreFolder.md)|Folder of the directory located|No|
|Path|string|Relevant path to the directory to be removed. Set to empty string to remove the whole folder.|No|
|SkipFile|*switch*|Skips the file moving operating in file system.|-|
|OverrideSealedFolder|*switch*|Overrides the IsSealed mark of the folder.|-|

From Pipeline: Path

# Return
None.

# Thumbprint Cache
To remove the related cache files, call [Set-ImageStoreThumbprintCacheFolder](../SimilarFile/SetThumbprintCacheFolder.md) before this operating.

# See also
  * [Concept: File](../../concept/File.md)
  * [Concept: ThumbprintCache](../../concept/ThumbprintCache.md)
  * [File Cmdlets](../cmdlets.md#file)
