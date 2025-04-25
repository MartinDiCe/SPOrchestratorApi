# Etapa de compilación (build stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el .csproj y restaura dependencias
COPY ["SPOrchestratorAPI.csproj", "./"]
RUN dotnet restore "SPOrchestratorAPI.csproj"

# Copia el resto del código y publica en modo Release
COPY . .
RUN dotnet publish "SPOrchestratorAPI.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Etapa de ejecución (runtime stage)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instala dependencias para New Relic .NET agent, lo descarga e instala
RUN apt-get update && apt-get install -y wget unzip \
    && wget https://download.newrelic.com/dot_net_agent/latest/linux/newrelic-dotnet-agent-linux.tar.gz \
    && tar -xzf newrelic-dotnet-agent-linux.tar.gz -C /usr/local \
    && rm newrelic-dotnet-agent-linux.tar.gz \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Variables de entorno necesarias para el CLR profiler de New Relic
ENV CORECLR_ENABLE_PROFILING=true \
    COR_PROFILER={71DA0A04-7777-4EC6-9643-7D28B46A8A41} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore \
    ASPNETCORE_URLS=http://+:80

# Copia la publicación desde la etapa de compilación
COPY --from=build /app/publish .

# Expone el puerto de la aplicación
EXPOSE 80

# Punto de entrada
ENTRYPOINT ["dotnet", "SPOrchestratorAPI.dll"]
