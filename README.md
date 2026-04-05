# OwlCore.Storage.Ntfs

[![NuGet](https://img.shields.io/nuget/v/OwlCore.Storage.Ntfs.svg)](https://www.nuget.org/packages/OwlCore.Storage.Ntfs)
[![Build CI](https://github.com/itsWindows11/OwlCore.Storage.Ntfs/actions/workflows/build.yml/badge.svg)](https://github.com/itsWindows11/OwlCore.Storage.Ntfs/actions/workflows/build.yml)

`OwlCore.Storage.Ntfs` is an [`OwlCore.Storage`](https://github.com/Arlodotexe/OwlCore.Storage)-based library for enumerating NTFS drives and exposing files/folders through OwlCore storage abstractions.

## Features

- Enumerate NTFS folders and files with `NtfsReader`
- Access files as `NtfsFile` and folders as `NtfsFolder`
- Supports `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`

## Remarks

- Folder/file objects are bound to the `NtfsReader` snapshot they were created with.
- Folder enumeration does not support live change tracking. Recreate `NtfsReader` (and `NtfsFolder`/`NtfsFile`) to observe new filesystem changes.

## Install

```bash
dotnet add package OwlCore.Storage.Ntfs
```

## Sample

```csharp
using OwlCore.Storage;
using OwlCore.Storage.Ntfs;
using System.IO;
using System.IO.Filesystem.Ntfs;

var reader = await NtfsReader.CreateAsync(new DriveInfo("C:\\"), RetrieveMode.StandardInformations);
var rootFolder = new NtfsFolder(reader, @"C:\");

await foreach (var item in rootFolder.GetItemsAsync(StorableType.All))
{
    Console.WriteLine($"{item.Name} ({item.Id})");
}

reader.Dispose();
```

## NuGet

- Package: https://www.nuget.org/packages/OwlCore.Storage.Ntfs
