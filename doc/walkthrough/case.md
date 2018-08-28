# Walkthrough
Here is a story how I use this product to manage my CG libs.

I have a huge collection of lots of CG libs from games and albums. And it keep growing. When I add some standalone image, not identified with the theme or source, to my collection, I want to check whether this image is stored already. When I get a new lib of CG, I need to check whether this lib is saved already.

# PowerShell related
After PowerShell started, enter these commands:

  * Loads ImageStore. You may need to change the path to the dll file.

    ```import-module .\ImageStore.dll```

  * Opens database connection. I placed the database file in D:\ImageStoreDatabase and want to use it with Sql Server 2017 LocalDB.

    ```open-ImageStoreDatabase "server=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\ImageStoreDatabase\DataStore.mdf;Integrated Security=True"``` 

  * Makes the outputs on.

    ```$InformationPreference="Continue"```
    
    ```$VerbosePreference="Continue"```

  * Enables cache. D:\ImageStoreDatabase\Cache is created for storing cache files.

    ```SetThumbprintCacheFolder "D:\ImageStoreDatabase\Cache"```

# Preparation
## Folders
I have 4 folders created for managing the collection.

|Name|Purpose|Path|
|---|---|---|
|Images|Stores CG libs.|D:\Images|
|Standalone Images|Stores images not be identified.|D:\Images\Standalone|
|New Libs|Store new collected CG libs temporarily.|D:\New Images|
|New Standalone Images|Stores new collected standalone images.|D:\New Standalone Images|

Of course, the first and second folder should be empty at beginning.

Here are the command to create these folders:

```
$ImagesFolder = Add-ImageStoreFolder -Name "Images" -Path "D:\Images" -IsSealed $true
$StandaloneImagesFolder = Add-ImageStoreFolder -Name "Standalone Images" -Path "D:\Images\Standalone" -CompareImageWith FilesInOtherFolders -IsSealed $true
$NewLibsFolder = Add-ImageStoreFolder -Name "New Libs" -Path "D:\New Images"
$NewStandaloneImagesFolder = Add-ImageStoreFolder -Name "New Standalone Images" -Path "D:\New Standalone Images" -CompareImageWith FilesInOtherDirectories
```

Because the 2nd folder is located in the 1st one, I need to ignore the directory from the 1st folder.

```Add-ImageStoreIgnoredDirectory -FolderId $ImagesFolder.Id -Directory "Standalone" -IsSubDirectoryIncluded $false```

## Extensions
Here are all extensions in my collections.

```
Add-ImageStoreExtension -Extension "jpg"
Add-ImageStoreExtension -Extension "jpeg"
Add-ImageStoreExtension -Extension "png"
Add-ImageStoreExtension -Extension "mkv" -IsImage $false
Add-ImageStoreExtension -Extension "webm" -IsImage $false
Add-ImageStoreExtension -Extension "gif"
Add-ImageStoreExtension -Extension "mp4" -IsImage $false
```

# Folders syncing
Now I can put new collected images into New Libs or New Standalone Images. After that, I need to run ```Sync-ImageStoreFolder``` to sync. If I make some change in the first 2 folders, run ```Sync-ImageStoreFolder -OverrideSealedFolder``` to sync them all.

# File measuring
After sync, measure the new added files by using ```Measure-ImageStoreFiles```.

# Dealing with same files
It's a good idea to remove same files before comparing similar files.

First, use ```Compare-ImageStoreSameFiles``` to find all same files.

I want to select all same files located in the New Standalone Images folder and remove them:

```
$target = Find-ImageStoreFolder -Name "New Standalone Images"
$files = Select-ImageStoreSameFile -Folder $target
foreach($i in $files){Remove-ImageStoreFile -Id $i.id}
```

And do the same thing on the folder New Libs:

```
$target = Find-ImageStoreFolder -Name "New Libs"
$files = Select-ImageStoreSameFile -Folder $target
foreach($i in $files){Remove-ImageStoreFile -Id $i.id}
```

Finally, remove the obsoleted records:

```Clear-ImageStoreSameFileObsoletedGroups```

# Dealing with similar files
First to find the similar file relations by running ```Compare-ImageStoreSimilarFiles```.

Then run [Resolve-ImageStoreSimilarFiles](../cmdlet/SimilarFile/ResolveSimilarFiles.md) to deal the similar files.

Because too many records may be generated and some of them are by mistake, I strongly recommend you run it with DifferenceDegree parameter increasingly, and disconnect all wrong relations.

For example:
```
$files = Resolve-ImageStoreSimilarFiles -DifferenceDegree 0.01
foreach($i in $files){Remove-ImageStoreFile -File $i -OverrideSealedFolder}
```

# Remove directory
When you realize there is a full directory should be removed, you don't need to remove all files one by one.

You can choose:
  * Remove this directory from file system and then rerun sync command. Or,
  * Use [Remove-ImageStoreDirectory](../cmdlet/File/RemoveDirectory.md) to remove it.

# Move files
Finally, move all files from the 3rd and 4th folder into the 1st and 2nd one.

```
$source = Find-ImageStoreFolder -Name "New Libs"
$target = Find-ImageStoreFolder -Name "Standalone Images"
$files = Search-ImageStoreFolder -FolderId $source.Id
foreach($i in $files){Move-ImageStoreFile -File $i -NewFolder $target -OverrideSealedFolder}
```

```
$source = Find-ImageStoreFolder -Name "New Standalone Images"
$target = Find-ImageStoreFolder -Name "Images"
$files = Search-ImageStoreFolder -FolderId $source.Id
foreach($i in $files){Move-ImageStoreFile -File $i -NewFolder $target -OverrideSealedFolder}
```
# Remove empty directories
After removing and moving files, there may be some directories are empty.

To remove them, use [Clear-ImageStoreEmptyFolders](../cmdlet/Folder/ClearEmptyFolders.md), or remove them directly from file system.

# Tips
Actually, you dont need to enter the names of parameters in all commands above, starting with hyphen, except switch like ```-OverrideSealedFolder```. Because those parameters are provided in the right sequence, cmdlet can detect the parameter of them correctly.
