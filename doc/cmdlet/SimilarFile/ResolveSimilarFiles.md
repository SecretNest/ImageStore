# Resolve-ImageStoreSimilarFiles
Deals with found similar files and selects files for further operating, like removal.

A window will be shown while running this cmdlet.

Alias: ResolveSimilarFiles

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|DifferenceDegree|float?|The maximum difference degree of records to be viewed in this time.|Yes|
|BuildsDisconnectedGroup|*switch*|Collects all disconnected records into a special group.|-|

If DifferenceDegree is absent or larger than the minimum ImageComparedThreshold of file records, the latter data will be used in place.

From Pipeline: DifferenceDegree

# UI
A window for user dealing selection.

## Groups (Left Panel)
Each group generated will be displayed here. One icon for each group. The label will display the number of files in this group.

All disconnected relations will be grouped into a dedicated group displayed at bottom. Any group contains no effective records will be shown below the effective groups. By default, all hidden relations are not visible on screen.

## Relations (Upper Panel)
You can choose the way to deal the relations within the group selected in the left panel.
  * By Files: All files in this group will be displayed in File 1 list. After you click any file from the list, all related files and the difference degree (Rate) will be displayed in File 2 list.
  * By Relations: Display all relations directly.

User need to check all files which are intended to return.

### Operations
  * Button - Check None: Checks none file from this relation. Only the first selected record will be processed.
  * Button - Check Both: Checks both files from this relation. Only the first selected record will be processed.
  * Button - Check the 1st: Checks the 1st file from this relation. Only the first selected record will be processed. In "By Files" mode, the checking state of the 2nd one will not be changed; In "By Relations" mode, the 2nd one will be unchecked.
  * Button - Check the 2nd: Checks the 2nd file from this relation. Only the first selected record will be processed. In "By Files" mode, the checking state of the 1st one will not be changed; In "By Relations" mode, the 1st one will be unchecked.
  * Mark as Effective: Marks the selected relations effective.
  * Mark as Hidden but Connected: Marks the selected relations hidden but connected.
  * Mark as Hidden and Disconnected: Marks the selected relations hidden and disconnected.

## Pictures (Lower Panel)
Displays both files of the selected relation. Only the first selected record will be processed.

Double clicking the image will use system default viewer to open the file related.

## Functions (Bottom Zone)
  * Button - Refresh Groups: Refreshes groups considering the newly state changed relations.
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