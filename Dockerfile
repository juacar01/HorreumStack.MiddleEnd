# Etapa 1: Compilación y restauración
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto (.csproj) para restaurar dependencias de forma eficiente usando caché de Docker
COPY ["src/HorreumStack.MiddleEnd/HorreumStack.MiddleEnd.csproj", "src/HorreumStack.MiddleEnd/"]
COPY ["src/HorreumStack.Domain/HorreumStack.Domain.csproj", "src/HorreumStack.Domain/"]
COPY ["src/HorreumStack.Infrastructure/HorreumStack.Infrastructure.csproj", "src/HorreumStack.Infrastructure/"]
COPY ["src/HorreumStack.Utilities/HorreumStack.Utilities.csproj", "src/HorreumStack.Utilities/"]
COPY ["src/HorreumStack.Application/HorreumStack.Application.csproj", "src/HorreumStack.Application/"]

# Restaurar dependencias
RUN dotnet restore "src/HorreumStack.MiddleEnd/HorreumStack.MiddleEnd.csproj"

# Copiar solo el código fuente relevante (evitando archivos innecesarios de la raíz)
COPY ["src/", "src/"]

# Compilar y publicar en modo Release
WORKDIR "/src/src/HorreumStack.MiddleEnd"
RUN dotnet publish "HorreumStack.MiddleEnd.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime para ejecución (Imagen final limpia y de tamaño reducido sin código fuente)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# ── Variables de entorno requeridas en runtime ──────────────────────────────
# Estas variables deben ser provistas por la plataforma (Azure Container Apps)
# o por el archivo .env en desarrollo local. NO hardcodear valores aquí.

# Puerto de escucha
ENV ASPNETCORE_HTTP_PORTS=80

# URI del Azure Key Vault (Managed Identity lo usará para obtener los demás secrets)
# Valor inyectado desde: GitHub Secret → Azure Container Apps env var
ENV KeyVault__Uri=""

# Las siguientes variables son cargadas automáticamente desde Key Vault en producción.
# En desarrollo local deben definirse en .env o en appsettings.Development.json.
# Se declaran aquí solo como documentación de lo que el contenedor espera:
#   ConnectionStrings__DefaultConnection              → cadena de conexión SQL Server
#   AzureServices__CommunicationConnectionString      → Azure Communication Services
#   AzureServices__StorageConnectionString            → Azure Blob Storage
#   Jwt__SecretKey                                    → clave firma JWT
#   Jwt__Issuer                                       → emisor JWT
#   Jwt__Audience                                     → audiencia JWT

EXPOSE 80

ENTRYPOINT ["dotnet", "HorreumStack.MiddleEnd.dll"]

