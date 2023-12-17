using OwlCore.Storage.CommonTests;
using System.IO.Filesystem.Ntfs;

namespace OwlCore.Storage.Ntfs.Tests;

[TestClass]
public class IFolderTests : CommonIFolderTests
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

    // For some reason this test is not included on my end in the base class in the test explorer, so I put it here and it works. - itsWindows11
    [TestMethod]
    [DataRow(StorableType.None, 0, 0)]
    [DataRow(StorableType.None, 2, 2)]

    [DataRow(StorableType.File, 2, 0)]

    [DataRow(StorableType.Folder, 0, 2)]

    [DataRow(StorableType.Folder | StorableType.File, 2, 0),
     DataRow(StorableType.Folder | StorableType.File, 0, 2)]

    [DataRow(StorableType.All, 2, 0),
     DataRow(StorableType.All, 0, 2)]
    public new async Task GetItemsAsync_AllCombinations_TokenCancellationDuringEnumeration(StorableType type, int fileCount, int folderCount)
    {
        // No enumeration should take place if set to "None". Tests for this covered elsewhere.
        if (type == StorableType.None)
            return;

        var cancellationTokenSource = new CancellationTokenSource();
        var folder = await CreateFolderWithItems(fileCount, folderCount);

        await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
        {
            var index = 0;
            await foreach (var item in folder.GetItemsAsync(type, cancellationTokenSource.Token))
            {
                Assert.IsNotNull(item);

                index++;
                if (index > fileCount || index > folderCount)
                {
                    cancellationTokenSource.Cancel();
                }
            }
        });
    }
}