# Estapa 1: Compilación y restauración
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

# Copiar el resto del código fuente
COPY ["src/", "src/"]

# Compilar y publicar la aplicación en modo Release
WORKDIR "/src/src/HorreumStack.MiddleEnd"
RUN dotnet publish "HorreumStack.MiddleEnd.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer los puertos por defecto de .NET (8080 es el estándar en las imágenes modernas de .NET)
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "HorreumStack.MiddleEnd.dll"]
