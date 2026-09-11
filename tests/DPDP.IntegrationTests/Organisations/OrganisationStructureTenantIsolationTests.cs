using DPDP.Domain.Modules.Organisations;
using DPDP.Infrastructure.Persistence;
using DPDP.IntegrationTests.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DPDP.IntegrationTests.Organisations;

/// <summary>
/// Proves the generic ITenantScoped query filter (DpdpDbContext.ApplyTenantScopedFilter)
/// correctly isolates BusinessUnit and Department across tenants — the
/// same guarantee as DPDP.IntegrationTests.Identity.TenantIsolationTests,
/// exercised for Module 3's entities. See docs/ARCHITECTURE.md section 13.
/// </summary>
public sealed class OrganisationStructureTenantIsolationTests : IAsyncLifetime
{
    private readonly Guid _organisationAId = Guid.NewGuid();
    private readonly Guid _organisationBId = Guid.NewGuid();
    private readonly Guid _businessUnitAId = Guid.NewGuid();
    private readonly Guid _businessUnitBId = Guid.NewGuid();
    private readonly Guid _departmentAId = Guid.NewGuid();
    private readonly Guid _departmentBId = Guid.NewGuid();

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ConnectionStrings__Default")
        ?? throw new InvalidOperationException("Set ConnectionStrings__Default before running integration tests.");

    private static DpdpDbContext CreateContext(DPDP.Application.Common.Interfaces.ICurrentUserContext currentUser)
    {
        var options = new DbContextOptionsBuilder<DpdpDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DpdpDbContext(options, currentUser);
    }

    public async Task InitializeAsync()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(null, isSuperAdministrator: true));

        var now = DateTimeOffset.UtcNow;

        db.Organisations.AddRange(
            new Organisation { Id = _organisationAId, Name = $"OrgStructure-Test-A-{_organisationAId:N}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Organisation { Id = _organisationBId, Name = $"OrgStructure-Test-B-{_organisationBId:N}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now });

        db.BusinessUnits.AddRange(
            new BusinessUnit { Id = _businessUnitAId, OrganisationId = _organisationAId, Name = "BU-A", CreatedAt = now, UpdatedAt = now },
            new BusinessUnit { Id = _businessUnitBId, OrganisationId = _organisationBId, Name = "BU-B", CreatedAt = now, UpdatedAt = now });

        db.Departments.AddRange(
            new Department { Id = _departmentAId, OrganisationId = _organisationAId, BusinessUnitId = _businessUnitAId, Name = "Dept-A", CreatedAt = now, UpdatedAt = now },
            new Department { Id = _departmentBId, OrganisationId = _organisationBId, BusinessUnitId = _businessUnitBId, Name = "Dept-B", CreatedAt = now, UpdatedAt = now });

        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(null, isSuperAdministrator: true));

        await db.Departments.IgnoreQueryFilters().Where(d => d.Id == _departmentAId || d.Id == _departmentBId).ExecuteDeleteAsync();
        await db.BusinessUnits.IgnoreQueryFilters().Where(b => b.Id == _businessUnitAId || b.Id == _businessUnitBId).ExecuteDeleteAsync();
        await db.Organisations.IgnoreQueryFilters().Where(o => o.Id == _organisationAId || o.Id == _organisationBId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task A_caller_in_organisation_A_sees_only_organisation_As_business_units()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(_organisationAId));

        var visibleIds = await db.BusinessUnits.Select(b => b.Id).ToListAsync();

        Assert.Contains(_businessUnitAId, visibleIds);
        Assert.DoesNotContain(_businessUnitBId, visibleIds);
    }

    [Fact]
    public async Task A_caller_in_organisation_A_sees_only_organisation_As_departments()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(_organisationAId));

        var visibleIds = await db.Departments.Select(d => d.Id).ToListAsync();

        Assert.Contains(_departmentAId, visibleIds);
        Assert.DoesNotContain(_departmentBId, visibleIds);
    }

    [Fact]
    public async Task A_caller_in_organisation_A_cannot_fetch_organisation_Bs_business_unit_by_id()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(_organisationAId));

        var businessUnit = await db.BusinessUnits.FirstOrDefaultAsync(b => b.Id == _businessUnitBId);

        Assert.Null(businessUnit);
    }

    [Fact]
    public async Task A_super_administrator_sees_business_units_and_departments_from_every_organisation()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(null, isSuperAdministrator: true));

        var businessUnitIds = await db.BusinessUnits.Select(b => b.Id).ToListAsync();
        var departmentIds = await db.Departments.Select(d => d.Id).ToListAsync();

        Assert.Contains(_businessUnitAId, businessUnitIds);
        Assert.Contains(_businessUnitBId, businessUnitIds);
        Assert.Contains(_departmentAId, departmentIds);
        Assert.Contains(_departmentBId, departmentIds);
    }
}
