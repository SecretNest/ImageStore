# Resolve-ImageStoreSimilarFiles
Deals with found similar files and selects files for further operating, like removal.

A window will be shown while running this cmdlet.

Alias: ResolveSimilarFiles

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|DifferenceDegree|float?|The maximum difference degree of records to be viewed in this time.|Yes|
|IncludesDisconnected|*switch*|Includes all records marked as disconnected.|-|
|NoGrouping|*switch*|Lists similar files for each file instead of grouping by similar relationships.|-|
|File|[ImageStoreFile](../../type/ImageStoreFile.md)|Lists similar files for this file only.|Yes(*1)|
|FileId|Guid|Lists similar files for this file only, specified by id.|Yes(*1)|

If DifferenceDegree is absent or larger than the minimum ImageComparedThreshold of file records, the latter data will be used in place.

*1: No more than one parameter within this group should be provided. NoGrouping will be set if present.

From Pipeline: DifferenceDegree

# UI
A window for user dealing selection.

## Group Mode
Puts similar files into groups. All files linked with similar records will be put into the same group.

### Groups (Left Panel)
Each group generated will be displayed here. One icon for each group. The label will display the number of files in this group.

All disconnected relations will be grouped into a dedicated group displayed at bottom when IncludesDisconnected is specified. Any group contains no effective records will be shown below the effective groups. By default, all hidden relations are not visible on screen.

### Relations (Upper Panel)
You can choose the way to deal the relations within the group selected in the left panel.
  * By Files: All files in this group will be displayed in File 1 list. After you click any file from the list, all related files and the difference degree (Rate) will be displayed in File 2 list.
  * By Relations: Display all relations directly.

User need to check all files which are intended to return.

#### Operations
  * Button - Check None: Checks none file from this relation. Only the first selected record will be processed.
  * Button - Check Both: Checks both files from this relation. Only the first selected record will be processed.
  * Button - Check the 1st: Checks the 1st file from this relation. Only the first selected record will be processed. In "By Files" mode, the checking state of the 2nd one will not be changed; In "By Relations" mode, the 2nd one will be unchecked.
  * Button - Check the 2nd: Checks the 2nd file from this relation. Only the first selected record will be processed. In "By Files" mode, the checking state of the 1st one will not be changed; In "By Relations" mode, the 1st one will be unchecked.
  * Mark as Effective: Marks the selected relations effective.
  * Mark as Hidden but Connected: Marks the selected relations hidden but connected.
  * Mark as Hidden and Disconnected: Marks the selected relations hidden and disconnected.

## File Mode
Checks similar files for each file.

### Files (Left Panel)
Each file found will be displayed here. One icon for each file. The label will display the number of files similar to this file.

This panel is visible in file mode when File or FileId presents.

### Relations (Upper Panel)
The main file is labeled in the check box above.
All files related will be displayed in list below.

User need to check all files which are intended to return.

#### Operations
  * Button - Check / Uncheck Main: Inverts checking status of the main file.
  * Button - Check / Uncheck File: Inverts checking status of the files selected.
  * Mark as Effective: Marks the selected relations effective.
  * Mark as Hidden but Connected: Marks the selected relations hidden but connected.
  * Mark as Hidden and Disconnected: Marks the selected relations hidden and disconnected.
  * Go to selected file: Makes the selected file as the main file. Visible in file mode when File or FileId presents.
  * Order by Rate: Orders the list by IgnoredMode, then rate.
  * Order by Path: Orders the list by path, then file name.

## Pictures (Lower Panel)
Displays both files of the selected relation. Only the first selected record will be processed.

Double clicking the image will use system default viewer to open the file related.

## Functions (Bottom Zone)
  * Button - Refresh Groups: Refreshes groups considering the newly state changed relations. Visible in group mode.
  * Button - Refresh Files: Refreshes  files considering the newly state changed relations. Visible in file mode when File and FileId absent.
  * Checkbox - Show Hidden Records: Makes hidden relations visible as well.
  * Checkbox - Auto Move Next: Moves to the next relation after [operation](#operations) processed.
  * Checkbox - Auto Resize: Resizes images to fit the screen.
  * Button - OK: Returns all checked files.
  * Close window directly: Returns nothing. 

# Return
The list of the file records which checked for the further operating.

Type: List<[ImageStoreFile](../../type/ImageStoreFile.md)>

# See also
  * [Concept: Similar File](../../concept/SimilarFile.md)
  * [Similar File Cmdlets](../cmdlets.md#similar-file)
