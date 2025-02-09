# Etapa de compilación (build stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el archivo .csproj y restaura las dependencias
COPY ["SPOrchestratorAPI.csproj", "./"]
RUN dotnet restore "SPOrchestratorAPI.csproj"

# Copia el resto del código fuente y compila la aplicación
COPY . .
RUN dotnet publish "SPOrchestratorAPI.csproj" -c Release -o /app/publish --no-restore

# Etapa de ejecución (runtime stage)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia la publicación desde la etapa de compilación
COPY --from=build /app/publish .

# Exponer el puerto en el que se ejecutará la aplicación (por ejemplo, 80)
EXPOSE 80

# Define el comando de entrada para iniciar la aplicación
ENTRYPOINT ["dotnet", "SPOrchestratorAPI.dll"]
