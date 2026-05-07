using System;

#nullable enable

namespace OwlCore.Storage.Ntfs;

/// <summary>
/// Last-accessed timestamp property for NTFS storage items.
/// </summary>
internal sealed class NtfsLastAccessedAtProperty(IStorable owner, Func<DateTime?> getValue)
    : SimpleStorageProperty<DateTime?>(
        id: owner.Id + "/" + nameof(ILastAccessedAt.LastAccessedAt),
        name: nameof(ILastAccessedAt.LastAccessedAt),
        getter: getValue
    ), ILastAccessedAtProperty;
