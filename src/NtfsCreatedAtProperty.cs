using System;
using System.IO.Filesystem.Ntfs;

#nullable enable

namespace OwlCore.Storage.Ntfs;

/// <summary>
/// Creation timestamp property for NTFS storage items backed by <see cref="INode"/>.
/// </summary>
internal sealed class NtfsCreatedAtProperty(IStorable owner, INode node)
    : SimpleStorageProperty<DateTime?>(
        id: owner.Id + "/" + nameof(ICreatedAt.CreatedAt),
        name: nameof(ICreatedAt.CreatedAt),
        getter: () => node.CreationTime == DateTime.MinValue ? null : node.CreationTime
    ), ICreatedAtProperty;
