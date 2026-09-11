using System.Reflection;
using DPDP.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace DPDP.Infrastructure.Services;

public sealed class ApplicationInfoProvider(IHostEnvironment hostEnvironment) : IApplicationInfo
{
    public string ApplicationName => "DPDP-COMPASS";

    public string Version =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
        ?? "0.0.0";

    public string EnvironmentName => hostEnvironment.EnvironmentName;
}
