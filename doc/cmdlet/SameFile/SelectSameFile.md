# Select-ImageStoreSameFile
Deals with found same files and selects records for further operating, like removal.

A window may be shown while running this cmdlet.

Alias: SelectSameFile

# Algorithm
User need to provide a folder as a main folder.

In each same file group: 
  * If at least one file is not in the main folder, all records in the main folder will be selected automatically.
  * If no file is in the main folder, no record will be selected automatically.
  * If all records are in the main folder, no record will be selected automatically and this group need to be dealt by user interaction.

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Folder|[ImageStoreFolder](../../type/ImageStoreFolder.md)|Main folder|No|
|Sha1Hash|byte[]|Processes only one same file group specified by file hashing result. Processes all groups when absent.|Yes|
|UserInteraction|[UserInteraction](#user-interation)|Whether user interaction UI should be displayed. Default is ```Auto```.|Yes|

From Pipeline: Folder

# Parameters to Modify UI Color
UI window will use different back colors for odd and even groups, as well as different fore colors for selected, ignored and effective records by default. To change these colors, or turn off this function to use the system default colors, use these parameters below.

|Name|Type|Description|Optional|
|---|---|---|---|
|OddGroupLinesBackColor|System.Drawing.Color|Defines the back color of the odd groups.|Yes|
|EvenGroupLinesBackColor|System.Drawing.Color|Defines the back color of the even groups.|Yes|
|NormalBackColor|System.Drawing.Color|Defines the back color if records are not grouped by same file group.|Yes|
|NormalForeColor|System.Drawing.Color|Defines the fore color of the normal items.|Yes|
|SelectedForeColor|System.Drawing.Color|Defines the fore color of the items selected for returning.|Yes|
|IgnoredForeColor|System.Drawing.Color|Defines the fore color of the items set as ignored.|Yes|
|UseSystemColor|*switch*|Uses system defined colors instead. Will suppress all others color parameters above.|-|

# User Interaction
The condition of user interaction UI showing up.

Enum: SecretNest.ImageStore.SameFile.UserInteraction

|Element|Value|Description|
|---|---|---|
|Auto|0|Displays when required.|
|Enforced|1|Always displays.|
|Suppressed|2|Never displays.|

# UI
A window for user dealing selection that cannot be dealt automatically.

## View
All records found will be listed in window.

  * When ordered by group (as default), different colors will be applied.
  * Double click any item will use system default viewer to open the file related.
  * User need to check all items which are intended to return.

## Functions
  * Button - Mark as Ignored: Marks the selected items as ignored.
  * Button - Mark as Not Ignored: Marks the selected item as normal, not ignored.
  * Button - Auto Select: Checks all but one items in each same file group as manual selection.
  * Checkbox - Prevent Non-reserved Selection: Prevents checking all items in one same file group. Will uncheck one item automatically when user attempt to check all items in one same file group.
  * Checkbox - Hide Auto Dealt: Hides all groups dealt automatically already.
  * Button - OK: Returns the checked items.
  * Close window directly: Returns nothing.

# Return
The list of the records which checked for the further operating.

Type: List<[ImageStoreSameFile](../../type/ImageStoreSameFile.md)>

# See also
  * [Concept: Same File](../../concept/SameFile.md)
  * [Same File Cmdlets](../cmdlets.md#same-file)