using System;

#nullable enable

namespace OwlCore.Storage.Ntfs;

/// <summary>
/// Last-modified timestamp property for NTFS storage items.
/// </summary>
internal sealed class NtfsLastModifiedAtProperty(IStorable owner, Func<DateTime?> getValue)
    : SimpleStorageProperty<DateTime?>(
        id: owner.Id + "/" + nameof(ILastModifiedAt.LastModifiedAt),
        name: nameof(ILastModifiedAt.LastModifiedAt),
        getter: getValue
    ), ILastModifiedAtProperty;
