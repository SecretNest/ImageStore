# ImageStore
Image deduplication tool, optimized for large amount of pictures and CG libraries.

This tool is built as PowerShell cmdlets, providing user a flexible way to deal with large amount of image files (~1 million).
Specially, it is optimized for CG libraries storing by allowing user to suppress the comparing among files in the same folder.

# Image Hashing Arithmetic
This tool use a [forked version](https://github.com/scegg/phash) of [priHash](https://github.com/pgrho/phash), which added methods optimized for ImageStore calling.
priHash is a C# Implementation of pHash (http://phash.org). Based on phash-0.9.4 for Windows.
In this tool, [difference degree](doc/concept/DifferenceDegree.md) is based on the calculation of priHash.

# Use module in PowerShell

# Cmdlet

# Workthough

# Concept