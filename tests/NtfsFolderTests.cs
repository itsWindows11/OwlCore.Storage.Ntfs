using OwlCore.Storage.CommonTests;
using System.IO.Filesystem.Ntfs;

namespace OwlCore.Storage.Ntfs.Tests;

[TestClass]
public class NtfsFolderTests : CommonIFolderTests
{
    private NtfsReader? reader;

    public override Task<IFolder> CreateFolderAsync()
    {
        reader ??= new NtfsReader(new DriveInfo("C:\\"), RetrieveMode.Minimal);

        // We cannot actually create a folder, so we just return any folder that exists.
        // We use the user's temp folder here.
        return Task.FromResult<IFolder>(new NtfsFolder(reader, Path.GetTempPath()));
    }

    public override Task<IFolder> CreateFolderWithItems(int fileCount, int folderCount)
    {
        reader ??= new NtfsReader(new DriveInfo("C:\\"), RetrieveMode.Minimal);

        // We cannot actually create a folder, so we just return any folder that exists.
        // We use the user's temp folder here.
        return Task.FromResult<IFolder>(new NtfsFolder(reader, Path.GetTempPath()));
    }
}