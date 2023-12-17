using System.Collections.Generic;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.Storage.Ntfs;

public class NtfsFolder(NtfsReader reader, string path) : IChildFolder, IFastGetRoot, IFastGetItem, IFastGetItemRecursive, IFastGetFirstByName
{
    /// <summary>
    /// The <see cref="NtfsReader"/> that this folder belongs to.
    /// </summary>
    public NtfsReader Reader => reader;

    /// <summary>
    /// The folder path.
    /// </summary>
    public string Path => path;

    /// <inheritdoc/>
    public string Id => path;

    /// <inheritdoc/>
    public string Name { get; } = System.IO.Path.GetDirectoryName(path);

    /// <inheritdoc/>
    public Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        => GetItemRecursiveAsync(name, cancellationToken);

    /// <inheritdoc/>
    public Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
        => GetItemRecursiveAsync(id, cancellationToken);

    /// <inheritdoc/>
    public async Task<IStorableChild> GetItemRecursiveAsync(string id, CancellationToken cancellationToken = default)
    {
        await foreach (var item in GetItemsAsync(cancellationToken: cancellationToken))
        {
            if (item.Id == id)
                return item;
        }

        throw new FileNotFoundException();
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<IStorableChild> GetItemsAsync(
        StorableType type = StorableType.All,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (type == StorableType.None)
            yield break;

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var file in await Task.Run(() => Reader.GetNodes(path), cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.Attributes.HasFlag(Attributes.Directory) && (type.HasFlag(StorableType.Folder) || type.HasFlag(StorableType.All)))
                yield return new NtfsFolder(reader, file.FullName);

            yield return new NtfsFile(reader, file);
        }
    }

    /// <inheritdoc/>
    public Task<IFolder> GetParentAsync(CancellationToken cancellationToken = default)
    {
        DirectoryInfo parent = Directory.GetParent(Path);
        return Task.FromResult<IFolder>(parent is { } ? new NtfsFolder(reader, parent.FullName) : null);
    }

    /// <inheritdoc/>
    public Task<IFolder> GetRootAsync()
    {
        DirectoryInfo root = new DirectoryInfo(Id).Root;
        return Task.FromResult<IFolder>(new NtfsFolder(reader, root.FullName));
    }
}