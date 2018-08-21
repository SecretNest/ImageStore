# Folder
In ImageStore, Folder is not the same as your folders stored in hard drive, but a place for storing your images. For example, if you save all your photos in D:\MyPhotos, no matter how many sub directories are in it, you need to create D:\MyPhotos as a folder in ImageStore.

There is an important property for each folder named CompareImageWithCode. It can be set as:
  * All: The files in this folder will be compared with all files.
  * FilesInOtherDirectories: The files in this folder will be compared with all files other than those from same directory (sub directories are not included).
  * FilesInOtherFolders: The files in this folder will be compared with all files other than those in this folder.
  
For example, you may want to set it as FilesInOtherDirectories when lots of CG files from Game are stored in this folder, separated in dedicated directory for each game. This will ignore comparing among these files from the same game. If the folder is working as a permanent storage, you may want to set it as FilesInOtherFolders to avoid comparing within this folder. This will speed up the comparing processing a lot.

If the folder has the flag IsSealed on, all writing attempt like moving, renaming, adding and removing files belong to it will be terminated unless switch OverrideSealedFolder is provided while you calling the cmdlets.

To modifing folders, you need to call [Folder Cmdlets](../cmdlet/cmdlets.md#database).
