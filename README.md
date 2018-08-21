# ImageStore
Image deduplication tool, optimized for large amount of pictures and CG libraries.

This tool is built as PowerShell cmdlets, providing user a flexible way to deal with large amount of image files (~1 million).
Specially, it is optimized for CG libraries storing by allowing user to suppress the comparing among files in the same folder.

# Image Hashing Arithmetic
This tool use a [forked version](https://github.com/scegg/phash) of [priHash](https://github.com/pgrho/phash), which added methods optimized for ImageStore calling.
priHash is a C# Implementation of pHash (http://phash.org). Based on phash-0.9.4 for Windows.
In this tool, [difference degree](doc/concept/DifferenceDegree.md) is based on the calculation of priHash.

# Use module in PowerShell
To use any module from dll in PowerShell, you just need 3 steps:
1. Start PowerShell. Usually it is placed as ```C:\windows\System32\windowspowershell\v1.0\powershell.exe``` in Windows.
2. Use command ```Import-Module``` to load module from dll file. The parameter of this command is the path of the dll file.  
```Import-Module C:\ImageStore.dll``` will load the module file named as ImageStore.dll and placed in the root folder of drive C.  
```Import-Module .\ImageStore.dll``` will load the module file named as ImageStore.dll from the current directory.
3. Use cmdlets.

Also, you can combine the step 1 and 2 as one, by passing the command as a parameter while starting PowerShell.  
```C:\windows\System32\windowspowershell\v1.0\powershell.exe -noexit -command "Import-Module .\ImageStore.dll"```

## Enable Information and Verbose Output
By default, the information and verbose level output will be silenced. ImageStore will report key information as informational output and progress updates as verbose one. Thus, enabling information output is highly recommended. If ImageStore is dealing with large amount of files, turning on verbose output is advised.

  * Turn on information output: ```$InformationPreference="Continue"```
  * Turn on verbose output: ```$VerbosePreference="Continue"```
  * Turn off information output: ```$InformationPreference="SilentlyContinue"```, or simply restart PowerShell.
  * Turn off verbose output: ```$VerbosePreference="SilentlyContinue"```, or simply restart PowerShell.

These setting will not be preserved among instances of PowerShell. Every time the PowerShell started, there are set as ```
"SilentlyContinue"```.

# Database
ImageStore need SqlServer 2017 for hosting database. Each project should have a dedicated database.

All editions of SqlServer 2017 on Windows are supported, including LocalDb, Express, Standard, Enterprise and Developer. Linux verions are not tested.

Attached database file mode is supported and recommended.

To install SqlServer 2017, access [Sql Server 2017 Homepage](https://www.microsoft.com/en-us/sql-server/sql-server-2017) and download the edition you desierd. LocalDb or Express edition will be a good choice IMHO.

# Concept

# Cmdlet

# Type

# Workthough
