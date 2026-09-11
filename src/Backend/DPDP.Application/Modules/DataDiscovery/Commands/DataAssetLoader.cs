using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataDiscovery.Commands;

internal static class DataAssetLoader
{
    public static async Task<DataAsset> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.DataAssets
            .Include(a => a.DataSource)
            .Include(a => a.Elements).ThenInclude(e => e.CorrectedByUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(DataAsset), id);
}
