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

# Concept

# Cmdlet

# Type

# Workthough
