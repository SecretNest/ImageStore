# Walkthrough
Here is a story how I use this product to manage my CG libs.

I have a huge collection of lots of CG libs from games and albums. And it keep growing. When I add some standalone image, not identified with the theme or source, to my collection, I want to check whether this image is stored already. When I get a new lib of CG, I need to check whether this lib is saved already.

# PowerShell related
After PowerShell started, enter these commands:

  * Loads ImageStore. You may need to change the path to the dll file.

   ```import-module .\ImageStore.dll```

```open-ImageStoreDatabase "server=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\ImageStoreDatabase\DataStore.mdf;Integrated Security=True"``` I placed the database file in D:\ImageStoreDatabase and want to use it with Sql Server 2017 LocalDB.


```
$InformationPreference="Continue"
$VerbosePreference="Continue"
```
SetThumbprintCacheFolder "Cache"

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

# Sync folders
Now I can put new collected images into New Libs or New Standalone Images. After that, I need to run ```Sync-ImageStoreFolder``` to sync. If I make some change in the first 2 folders, run ```Sync-ImageStoreFolder -OverrideSealedFolder``` to sync them all.

# Measure files
After sync, measure the new added files by using ```Measure-ImageStoreFiles```.

# Compare same files
It's a good idea to remove same files before comparing similar files.

First, I want to select all same files located in the New Standalone Images folder.

```

```