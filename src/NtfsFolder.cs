using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace OwlCore.Storage.Ntfs;

/// <summary>
/// Represents a folder in an NTFS volume.
/// </summary>
/// <remarks>
/// Instances are bound to the <see cref="NtfsReader"/> passed to the constructor. If the reader is disposed
/// and recreated, create a new <see cref="NtfsFolder"/> instance to observe the updated view.
/// <para>
/// <see cref="GetItemsAsync(StorableType, CancellationToken)"/> returns direct children only, excludes the
/// current folder node, and honors <see cref="StorableType"/> filtering for files and folders.
/// </para>
/// </remarks>
public class NtfsFolder(NtfsReader reader, string path) : IChildFolder, IGetRoot, IGetItem, IGetItemRecursive, IGetFirstByName
{
    private INode? _node;
    private ICreatedAtProperty? _createdAt;
    private ILastAccessedAtProperty? _lastAccessedAt;
    private ILastModifiedAtProperty? _lastModifiedAt;

    /// <summary>
    /// Initializes a new instance of <see cref="NtfsFolder"/> from a known <see cref="INode"/>.
    /// </summary>
    /// <param name="reader">The <see cref="NtfsReader"/> that owns the node.</param>
    /// <param name="node">The NTFS directory node.</param>
    public NtfsFolder(NtfsReader reader, INode node) : this(reader, node.FullName ?? throw new ArgumentNullException(nameof(node), "INode.FullName must not be null."))
    {
        _node = node;
    }

    /// <summary>
    /// The <see cref="NtfsReader"/> that this folder belongs to.
    /// </summary>
    /// <remarks>
    /// This reference is not updated automatically if callers replace or dispose the original reader instance.
    /// </remarks>
    public NtfsReader Reader => reader;

    /// <summary>
    /// The folder path.
    /// </summary>
    public string Path { get; } = path.TrimEnd(
        global::System.IO.Path.PathSeparator,
        global::System.IO.Path.DirectorySeparatorChar,
        global::System.IO.Path.AltDirectorySeparatorChar
    );

    /// <inheritdoc/>
    public string Id => Path;

    /// <inheritdoc/>
    public string Name { get; } = global::System.IO.Path.GetFileName(
        path.TrimEnd(
            global::System.IO.Path.PathSeparator,
            global::System.IO.Path.DirectorySeparatorChar,
            global::System.IO.Path.AltDirectorySeparatorChar
        )
    );

    /// <summary>
    /// Gets the creation timestamp property, or <c>null</c> if the reader was not initialized with
    /// <see cref="RetrieveMode.StandardInformations"/> (indicated by an unset timestamp), or if this
    /// instance was not created from an <see cref="INode"/> (e.g. constructed by path only).
    /// </summary>
    public ICreatedAtProperty? CreatedAt => _node is not null && _node.CreationTime != DateTime.MinValue
        ? _createdAt ??= new NtfsCreatedAtProperty(this, _node)
        : null;

    /// <summary>
    /// Gets the last-accessed timestamp property, or <c>null</c> if the reader was not initialized with
    /// <see cref="RetrieveMode.StandardInformations"/> (indicated by an unset timestamp), or if this
    /// instance was not created from an <see cref="INode"/> (e.g. constructed by path only).
    /// </summary>
    public ILastAccessedAtProperty? LastAccessedAt => _node is not null && _node.LastAccessTime != DateTime.MinValue
        ? _lastAccessedAt ??= new NtfsLastAccessedAtProperty(this, _node)
        : null;

    /// <summary>
    /// Gets the last-modified timestamp property, or <c>null</c> if the reader was not initialized with
    /// <see cref="RetrieveMode.StandardInformations"/> (indicated by an unset timestamp), or if this
    /// instance was not created from an <see cref="INode"/> (e.g. constructed by path only).
    /// </summary>
    public ILastModifiedAtProperty? LastModifiedAt => _node is not null && _node.LastChangeTime != DateTime.MinValue
        ? _lastModifiedAt ??= new NtfsLastModifiedAtProperty(this, _node)
        : null;

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
        cancellationToken.ThrowIfCancellationRequested();

        if (type == StorableType.None)
            throw new ArgumentOutOfRangeException(nameof(type));

        cancellationToken.ThrowIfCancellationRequested();

        var includeFolders = type.HasFlag(StorableType.Folder);
        var includeFiles = type.HasFlag(StorableType.File);

        var nodes = await Task.Run(() => Reader.GetNodes(path), cancellationToken).ConfigureAwait(false);
        foreach (var node in nodes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedNodePath = node.FullName.TrimEnd(
                global::System.IO.Path.PathSeparator,
                global::System.IO.Path.DirectorySeparatorChar,
                global::System.IO.Path.AltDirectorySeparatorChar
            );

            if (string.Equals(normalizedNodePath, Path, StringComparison.OrdinalIgnoreCase))
                continue;

            var isDirectory = node.Attributes.HasFlag(Attributes.Directory);

            if (isDirectory)
            {
                if (includeFolders)
                    yield return new NtfsFolder(reader, node);

                continue;
            }

            if (includeFiles)
                yield return new NtfsFile(reader, node);
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