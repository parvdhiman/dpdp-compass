namespace DPDP.Application.Modules.DataDiscovery.Connectors;

/// <summary>
/// SampleMaskedValue is the ONLY sample-related field here on purpose —
/// there is deliberately no raw-value property anywhere on this type, so a
/// connector cannot pass an unmasked value up to the Application layer
/// even by accident. Every connector must call
/// DPDP.Domain.Modules.DataDiscovery.SampleMasker.Mask itself before
/// constructing this record.
/// </summary>
public sealed record DiscoveredColumn(
    string ColumnName,
    string DataType,
    bool IsNullable,
    int OrdinalPosition,
    string? SampleMaskedValue);
