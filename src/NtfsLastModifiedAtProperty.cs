using System;
using System.IO.Filesystem.Ntfs;

#nullable enable

namespace OwlCore.Storage.Ntfs;

/// <summary>
/// Last-modified timestamp property for NTFS storage items backed by <see cref="INode"/>.
/// </summary>
internal sealed class NtfsLastModifiedAtProperty(IStorable owner, INode node)
    : SimpleStorageProperty<DateTime?>(
        id: owner.Id + "/" + nameof(ILastModifiedAt.LastModifiedAt),
        name: nameof(ILastModifiedAt.LastModifiedAt),
        getter: () => node.LastChangeTime == DateTime.MinValue ? null : node.LastChangeTime
    ), ILastModifiedAtProperty;
