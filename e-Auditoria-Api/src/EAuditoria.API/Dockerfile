FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY EAuditoria.sln ./
COPY src/EAuditoria.Domain/EAuditoria.Domain.csproj              src/EAuditoria.Domain/
COPY src/EAuditoria.Application/EAuditoria.Application.csproj    src/EAuditoria.Application/
COPY src/EAuditoria.Infrastructure/EAuditoria.Infrastructure.csproj src/EAuditoria.Infrastructure/
COPY src/EAuditoria.API/EAuditoria.API.csproj                    src/EAuditoria.API/
COPY tests/EAuditoria.Tests/EAuditoria.Tests.csproj              tests/EAuditoria.Tests/

RUN dotnet restore

COPY . .
RUN dotnet publish src/EAuditoria.API/EAuditoria.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "EAuditoria.API.dll"]
