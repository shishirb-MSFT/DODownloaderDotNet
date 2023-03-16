﻿- Sample code to showcase usage of Delivery Optimization client's COM API via C# Interop.
- COM API is documented here: https://learn.microsoft.com/en-us/windows/win32/delivery_optimization/do-reference
- The project currently shows the following use cases:
    - File output and streaming output.
    - Full-file downloads and partial-file downloads (byte range requests).
    - Download status callbacks.
    - Enumerate existing downloads. Filtering not implemented yet.
- Tested on Visual Studio 2022
- Minimum required Windows OS version: Windows 10, version 1809 (10.0.17763.1)
- More on Delivery Optimization: https://learn.microsoft.com/en-us/windows/deployment/do/
