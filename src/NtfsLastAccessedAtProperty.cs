using System;
using System.IO.Filesystem.Ntfs;

#nullable enable

namespace OwlCore.Storage.Ntfs;

/// <summary>
/// Last-accessed timestamp property for NTFS storage items backed by <see cref="INode"/>.
/// </summary>
internal sealed class NtfsLastAccessedAtProperty(IStorable owner, INode node)
    : SimpleStorageProperty<DateTime?>(
        id: owner.Id + "/" + nameof(ILastAccessedAt.LastAccessedAt),
        name: nameof(ILastAccessedAt.LastAccessedAt),
        getter: () => node.LastAccessTime == DateTime.MinValue ? null : node.LastAccessTime
    ), ILastAccessedAtProperty;
