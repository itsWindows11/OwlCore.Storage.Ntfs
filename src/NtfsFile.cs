using System;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace OwlCore.Storage.Ntfs;

/// <summary>
/// Represents a file in an NTFS volume.
/// </summary>
/// <remarks>
/// Instances are bound to the <see cref="NtfsReader"/> passed to the constructor. If the reader is disposed
/// and recreated, create a new <see cref="NtfsFile"/> instance to observe the updated view.
/// </remarks>
public class NtfsFile(NtfsReader reader, INode node) : IChildFile, IGetRoot
{
    private ICreatedAtProperty? _createdAt;
    private ILastAccessedAtProperty? _lastAccessedAt;
    private ILastModifiedAtProperty? _lastModifiedAt;

    /// <summary>
    /// The <see cref="NtfsReader"/> that this file belongs to.
    /// </summary>
    /// <remarks>
    /// This reference is not updated automatically if callers replace or dispose the original reader instance.
    /// </remarks>
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

    /// <summary>
    /// Gets the creation timestamp property, or <c>null</c> if the reader was not initialized with
    /// <see cref="RetrieveMode.StandardInformations"/> (indicated by an unset timestamp).
    /// </summary>
    public ICreatedAtProperty? CreatedAt => node.CreationTime != DateTime.MinValue
        ? _createdAt ??= new NtfsCreatedAtProperty(this, node)
        : null;

    /// <summary>
    /// Gets the last-accessed timestamp property, or <c>null</c> if the reader was not initialized with
    /// <see cref="RetrieveMode.StandardInformations"/> (indicated by an unset timestamp).
    /// </summary>
    public ILastAccessedAtProperty? LastAccessedAt => node.LastAccessTime != DateTime.MinValue
        ? _lastAccessedAt ??= new NtfsLastAccessedAtProperty(this, node)
        : null;

    /// <summary>
    /// Gets the last-modified timestamp property, or <c>null</c> if the reader was not initialized with
    /// <see cref="RetrieveMode.StandardInformations"/> (indicated by an unset timestamp).
    /// </summary>
    public ILastModifiedAtProperty? LastModifiedAt => node.LastChangeTime != DateTime.MinValue
        ? _lastModifiedAt ??= new NtfsLastModifiedAtProperty(this, node)
        : null;

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