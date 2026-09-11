using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Application.Modules.DataDiscovery.Classification;

/// <summary>
/// Bound from configuration section "DataClassification" — see
/// appsettings.json and docs/DATA_DISCOVERY.md section 4. Every keyword
/// list and confidence figure the classifier uses lives here, never as a
/// literal inside DefaultDataClassificationStrategy, so a deployment can
/// retune the rules (e.g. for a different naming convention, or a
/// non-English schema) without a code change.
/// </summary>
public sealed class ClassificationOptions
{
    public const string SectionName = "DataClassification";

    /// <summary>Column-name keyword lists, matched case-insensitively as whole "words" split on non-alphanumeric characters.</summary>
    public Dictionary<ClassificationCategory, string[]> ColumnNameKeywords { get; set; } = new()
    {
        [ClassificationCategory.IDENTIFIER] = ["ssn", "aadhaar", "aadhar", "pan", "passport", "national_id", "voter_id", "tax_id", "uid"],
        [ClassificationCategory.CONTACT] = ["email", "phone", "mobile", "telephone", "fax", "contact_number"],
        [ClassificationCategory.ADDRESS] = ["address", "street", "city", "state", "zip", "zipcode", "postal", "pincode", "pin_code", "country"],
        [ClassificationCategory.FINANCIAL] = ["account_number", "account_no", "iban", "ifsc", "card_number", "card_no", "cvv", "salary", "income", "bank", "upi"],
        [ClassificationCategory.IDENTITY] = ["first_name", "last_name", "full_name", "middle_name", "date_of_birth", "dob", "gender", "nationality", "photo"],
        [ClassificationCategory.EMPLOYEE] = ["employee_id", "emp_id", "employee_no", "designation", "department_id", "manager_id", "hire_date"],
        [ClassificationCategory.CUSTOMER] = ["customer_id", "cust_id", "client_id", "subscriber_id", "account_holder"],
        [ClassificationCategory.CHILD] = ["guardian", "parent_name", "child_dob", "minor"],
        [ClassificationCategory.HEALTH_RELATED] = ["diagnosis", "medical", "health", "disability", "blood_group", "allergy", "prescription", "patient"],
        [ClassificationCategory.LOCATION] = ["latitude", "longitude", "lat", "lng", "gps", "geo_location", "coordinates"],
    };

    /// <summary>A column name that exactly equals a keyword (after tokenizing).</summary>
    public decimal ExactMatchConfidence { get; set; } = 95m;

    /// <summary>A column name that merely contains a keyword as a substring.</summary>
    public decimal ContainsMatchConfidence { get; set; } = 75m;

    /// <summary>Added on top of a match's confidence when the masked sample's shape also looks like the category (e.g. an "@" for CONTACT) — capped at 99.</summary>
    public decimal SamplePatternBonus { get; set; } = 15m;
}
