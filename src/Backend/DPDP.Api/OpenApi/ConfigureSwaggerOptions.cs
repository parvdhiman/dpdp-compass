using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DPDP.Api.OpenApi;

/// <summary>
/// Generates one Swagger document per discovered API version, so adding a
/// new /api/v{N} surface never requires touching this file — see
/// docs/API.md section 1 (versioning strategy).
/// </summary>
public sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "DPDP-COMPASS API",
                Version = description.ApiVersion.ToString(),
                Description = "DPDP Compliance Management & Continuous Assessment Platform API."
                    + (description.IsDeprecated ? " This version is deprecated." : string.Empty),
            });
        }
    }
}
