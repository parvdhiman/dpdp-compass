# Build context is the repository root (needs the whole solution) —
# see deployment/docker-compose.yml, "full" profile.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./
COPY src/Backend/Directory.Build.props src/Backend/
COPY src/Backend/DPDP.Domain/DPDP.Domain.csproj src/Backend/DPDP.Domain/
COPY src/Backend/DPDP.Application/DPDP.Application.csproj src/Backend/DPDP.Application/
COPY src/Backend/DPDP.Infrastructure/DPDP.Infrastructure.csproj src/Backend/DPDP.Infrastructure/
COPY src/Backend/DPDP.Api/DPDP.Api.csproj src/Backend/DPDP.Api/
RUN dotnet restore src/Backend/DPDP.Api/DPDP.Api.csproj

COPY src/Backend/ src/Backend/
RUN dotnet publish src/Backend/DPDP.Api/DPDP.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN useradd --create-home dpdp
USER dpdp

COPY --from=build --chown=dpdp /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DPDP.Api.dll"]
