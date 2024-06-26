﻿using System.Collections.Generic;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.Storage.Ntfs;

public class NtfsFolder(NtfsReader reader, string path) : IChildFolder, IGetRoot, IGetItem, IGetItemRecursive, IGetFirstByName
{
    /// <summary>
    /// The <see cref="NtfsReader"/> that this folder belongs to.
    /// </summary>
    public NtfsReader Reader => reader;

    /// <summary>
    /// The folder path.
    /// </summary>
    public string Path { get; } = path.TrimEnd(global::System.IO.Path.PathSeparator, global::System.IO.Path.DirectorySeparatorChar, global::System.IO.Path.AltDirectorySeparatorChar);

    /// <inheritdoc/>
    public string Id => Path;

    /// <inheritdoc/>
    public string Name { get; } = global::System.IO.Path.GetFileName(path.TrimEnd(global::System.IO.Path.PathSeparator, global::System.IO.Path.DirectorySeparatorChar, global::System.IO.Path.AltDirectorySeparatorChar));

    /// <inheritdoc/>
    public Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        => GetItemRecursiveAsync(name, cancellationToken);

    /// <inheritdoc/>
    public Task<IStorableChild> GetItemRecursiveAsync(string id, CancellationToken cancellationToken = default)
        => GetItemAsync(id, cancellationToken);

    /// <inheritdoc/>
    public async Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await foreach (var item in GetItemsAsync(cancellationToken: cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            if (file.Attributes.HasFlag(Attributes.Directory) &&
                (type.HasFlag(StorableType.Folder) || type.HasFlag(StorableType.All))
            )
                yield return new NtfsFolder(reader, file.FullName);

            yield return new NtfsFile(reader, file);
        }
    }

    /// <inheritdoc/>
    public Task<IFolder> GetParentAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IFolder>(new NtfsFolder(reader, global::System.IO.Path.GetDirectoryName(Path)));
    }

    /// <inheritdoc/>
    public Task<IFolder> GetRootAsync(CancellationToken cancellationToken = default)
    {
        DirectoryInfo root = new DirectoryInfo(Id).Root;
        return Task.FromResult<IFolder>(new NtfsFolder(reader, root.FullName));
    }
}