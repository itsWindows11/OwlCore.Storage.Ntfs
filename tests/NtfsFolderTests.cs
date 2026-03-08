using OwlCore.Storage.CommonTests;
using System.IO.Filesystem.Ntfs;

namespace OwlCore.Storage.Ntfs.Tests;

[TestClass]
public class NtfsFolderTests : CommonIFolderTests
{
    private static NtfsReader? reader;
    private const string tempFolderName = "owlcorestoragentfstest";

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        reader = new NtfsReader(new DriveInfo("C:\\"), RetrieveMode.Minimal);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Directory.Delete(Path.Combine(Path.GetTempPath(), tempFolderName), true);
        reader?.Dispose();
    }

    public override Task<IFolder> CreateFolderAsync()
    {
        var ulid = Ulid.NewUlid().ToString();

        foreach (var character in Path.GetInvalidFileNameChars())
            ulid = ulid.Replace(character, '_');

        // Create a temporary folder for testing.
        var folder = Path.Combine(Path.GetTempPath(), tempFolderName, ulid);
        _ = Directory.CreateDirectory(folder);

        return Task.FromResult<IFolder>(new NtfsFolder(reader, folder));
    }

    public override async Task<IFolder> CreateFolderWithItems(int fileCount, int folderCount)
    {
        var folder = await CreateFolderAsync();
        var tasks = new List<Task>();

        for (int i = 0; i < folderCount; i++)
        {
            var subFolderPath = Path.Combine(folder.Id, $"subfolder{i}");
            _ = Directory.CreateDirectory(subFolderPath);
        }

        for (int i = 0; i < fileCount; i++)
        {
            var filePath = Path.Combine(folder.Id, $"file{i}.txt");
            tasks.Add(File.WriteAllTextAsync(filePath, "Test content"));
        }

        await Task.WhenAll(tasks);

        return folder;
    }
}