# Folder
In ImageStore, Folder is not the same as your folders stored in hard drive, but a place for storing your images. For example, if you save all your photos in D:\MyPhotos, no matter how many sub directories are in it, you need to create D:\MyPhotos as a folder in ImageStore.

There is an important property for each folder named CompareImageWithCode. It can be set as:
  * All: The files in this folder will be compared with all files.
  * FilesInOtherDirectories: The files in this folder will be compared with all files other than those from same directory (sub directories are not included).
  * FilesInOtherFolders: The files in this folder will be compared with all files other than those in this folder.
  
For example, you may want to set it as FilesInOtherDirectories when lots of CG files from Game are stored in this folder, separated in dedicated directory for each game. This will ignore comparing among these files from the same game. If the folder is working as a permanent storage, you may want to set it as FilesInOtherFolders to avoid comparing within this folder. This will speed up the comparing processing a lot.

If the folder has the flag IsSealed on, all writing attempt like moving, renaming, adding and removing files belong to it will be terminated unless switch OverrideSealedFolder is provided while you calling the cmdlets.

# Multiple Folders
Multiple folders are supported and recommended. You should create folders separately for different use in one project. For example, for Game CG files storing, you could create 4 folders:
  * Permanent for files organized by game. Set CompareImageWithCode to FilesInOtherFolders with IsSealed on.
  * Permanent for files not recognized to the name of game. Set CompareImageWithCode to FilesInOtherFolders with IsSealed on.
  * For new added files organized by game. Set CompareImageWithCode to FilesInOtherDirectories. and,
  * For new added files not recognized to the name of game. Set CompareImageWithCode to All.

No directory should be shared among folders. In another word, there should not be any folder which contains another one. If you have to use a sub directory from a folder as another one, you could use Ignored Directory functions.

# Ignored Directory
Ignore directory is a function to exclude one directory from a folder by specifying the path. You could choose to include the sub directories or not. All files in the directories pointed from Ignored Directory records will be ignored while processing.

# Cmdlets
  * To modifying Folders, you need to call [Folder Cmdlets](../cmdlet/cmdlets.md#folder).
  * To modifying Ignored Directories, you need to call [Ignore Directory Cmdlets](../cmdlet/cmdlets.md#ignored-directory).
  * After folder created, you should call [Sync-ImageStoreFolder](../cmdlet/Folder/SyncFolder.md) to make sure the file system is consistent with database.
