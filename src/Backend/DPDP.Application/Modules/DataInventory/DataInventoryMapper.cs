using System.Text.Json;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataInventory;

namespace DPDP.Application.Modules.DataInventory;

internal static class DataInventoryMapper
{
    public static string ItemNumber(DataInventoryItem item) => $"DI-{item.SequenceNumber:D5}";
    public static string ActivityNumber(ProcessingActivity activity) => $"PA-{activity.SequenceNumber:D5}";
    public static string FlowNumber(DataFlow flow) => $"DF-{flow.SequenceNumber:D5}";

    public static IReadOnlyList<string> ParseJsonArray(string? json) =>
        string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<string>>(json) ?? [];

    public static string SerializeJsonArray(IReadOnlyList<string> values) => JsonSerializer.Serialize(values);

    public static DataCategoryDto ToDto(DataCategory c) => new(c.Id, c.Name, c.Description, c.ClassificationCategory?.ToString(), c.IsActive, c.CreatedAt);

    public static ItSystemDto ToDto(ItSystem s) => new(s.Id, s.Name, s.Description, s.SystemType.ToString(), s.OwnerUserId, s.Owner?.FullName, s.IsActive, s.CreatedAt);

    public static DataCollectionSourceDto ToDto(DataCollectionSource s) => new(s.Id, s.Name, s.Description, s.SourceType.ToString(), s.IsActive, s.CreatedAt);

    public static ProcessorDto ToDto(Processor p) => new(p.Id, p.Name, p.Description, p.ContactEmail, p.Country, p.IsActive, p.CreatedAt);

    public static RecipientDto ToDto(Recipient r) => new(r.Id, r.Name, r.Description, r.RecipientType.ToString(), r.IsActive, r.CreatedAt);

    public static RetentionPolicyDto ToDto(RetentionPolicy p) => new(p.Id, p.Name, p.Description, p.RetentionPeriodValue, p.RetentionPeriodUnit.ToString(), p.TriggerEvent, p.IsActive, p.CreatedAt);

    public static DataInventoryItemDto ToDto(DataInventoryItem i) => new(
        i.Id, ItemNumber(i), i.DataCategoryId, i.DataCategory?.Name ?? string.Empty, i.DataElementName,
        i.DiscoveredDataElementId, i.Classification?.ToString(),
        i.DataCollectionSourceId, i.DataCollectionSource?.Name,
        i.ItSystemId, i.ItSystem?.Name,
        i.OwnerUserId, i.Owner?.FullName, i.Purpose,
        i.RetentionPolicyId, i.RetentionPolicy?.Name, i.SharingDescription,
        i.ProcessorId, i.Processor?.Name, i.RiskLevel?.ToString(),
        i.CreatedAt, i.UpdatedAt);

    public static ProcessingActivitySummaryDto ToSummaryDto(ProcessingActivity a) => new(
        a.Id, ActivityNumber(a), a.Name, a.Status.ToString(), a.OwnerUserId, a.Owner?.FullName,
        a.ReviewDate, a.DataCategories.Count, a.CreatedAt);

    public static ProcessingActivityDetailDto ToDetailDto(ProcessingActivity a) => new(
        a.Id, ActivityNumber(a), a.Name, a.Purpose, ParseJsonArray(a.DataSubjectCategoriesJson), ParseJsonArray(a.SecurityControlsJson),
        a.RetentionPolicyId, a.RetentionPolicy?.Name,
        a.OwnerUserId, a.Owner?.FullName, a.Status.ToString(), a.ReviewDate,
        a.SubmittedForReviewAt, a.ReviewedAt, a.ReviewComments,
        a.ApprovedAt, a.ArchivedAt,
        a.CreatedAt, a.UpdatedAt,
        a.DataCategories.Select(ToDto).ToList(), a.ItSystems.Select(ToDto).ToList(),
        a.DataCollectionSources.Select(ToDto).ToList(), a.Recipients.Select(ToDto).ToList(),
        a.Processors.Select(ToDto).ToList());

    public static DataFlowDto ToDto(DataFlow f) => new(
        f.Id, FlowNumber(f), f.Name, f.Description,
        f.ProcessingActivityId, f.ProcessingActivity?.Name,
        f.DataCategoryId, f.DataCategory?.Name,
        f.FromItSystemId, f.FromItSystem?.Name,
        f.FromDataCollectionSourceId, f.FromDataCollectionSource?.Name, f.FromDescription,
        f.ToItSystemId, f.ToItSystem?.Name,
        f.ToProcessorId, f.ToProcessor?.Name,
        f.ToRecipientId, f.ToRecipient?.Name, f.ToDescription,
        f.TransferMechanism, f.IsCrossBorder, f.CrossBorderCountry,
        f.CreatedAt, f.UpdatedAt);
}
