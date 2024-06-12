using System;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace OwlCore.Storage.Ntfs;

public class NtfsFile(NtfsReader reader, INode node) : IChildFile, IGetRoot
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
        => Task.FromResult<IFolder?>(new NtfsFolder(reader, global::System.IO.Path.GetDirectoryName(Id)));

    /// <inheritdoc/>
    public Task<IFolder?> GetRootAsync(CancellationToken cancellationToken = default)
    {
        DirectoryInfo root = new DirectoryInfo(Id).Root;
        return Task.FromResult<IFolder?>(new NtfsFolder(reader, root.FullName));
    }

    /// <inheritdoc/>
    public Task<Stream> OpenStreamAsync(FileAccess accessMode = FileAccess.Read, CancellationToken cancellationToken = default)
        => Task.FromResult<Stream>(new FileStream(Id, FileMode.Open, accessMode));
}