using System;

#nullable enable

namespace OwlCore.Storage.Ntfs;

/// <summary>
/// Creation timestamp property for NTFS storage items.
/// </summary>
internal sealed class NtfsCreatedAtProperty(IStorable owner, Func<DateTime?> getValue)
    : SimpleStorageProperty<DateTime?>(
        id: owner.Id + "/" + nameof(ICreatedAt.CreatedAt),
        name: nameof(ICreatedAt.CreatedAt),
        getter: getValue
    ), ICreatedAtProperty;
