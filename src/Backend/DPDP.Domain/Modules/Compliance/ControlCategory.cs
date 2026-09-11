using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Compliance;

public sealed class ControlCategory : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
