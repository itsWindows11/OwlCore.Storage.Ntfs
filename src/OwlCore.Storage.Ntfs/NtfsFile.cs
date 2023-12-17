using System;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace OwlCore.Storage.Ntfs;

public class NtfsFile(NtfsReader reader, INode node) : IChildFile, IFastGetRoot
{
    /// <summary>
    /// The <see cref="NtfsReader"/> that this file belongs to.
    /// </summary>
    public NtfsReader Reader => reader;

    public string Path => node.FullName;

    /// <inheritdoc/>
    public string Id => node.FullName;

    /// <inheritdoc/>
    public string Name => node.Name;

    public Attributes Attributes => node.Attributes;

    public DateTime CreationTime => node.CreationTime;

    public DateTime LastChangeTime => node.LastChangeTime;

    public DateTime LastAccessTime => node.LastAccessTime;

    /// <inheritdoc/>
    public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
    {
        // TODO: instead of creating a new DirectoryInfo, we could do string manipulation in the path.
        DirectoryInfo parent = Directory.GetParent(Id);
        return Task.FromResult<IFolder?>(parent is { } ? new NtfsFolder(reader, parent.FullName) : null);
    }

    /// <inheritdoc/>
    public Task<IFolder?> GetRootAsync()
    {
        DirectoryInfo root = new DirectoryInfo(Id).Root;
        return Task.FromResult<IFolder?>(new NtfsFolder(reader, root.FullName));
    }

    /// <inheritdoc/>
    public Task<Stream> OpenStreamAsync(FileAccess accessMode = FileAccess.Read, CancellationToken cancellationToken = default)
        => Task.FromResult<Stream>(new FileStream(Id, FileMode.Open, accessMode));
}