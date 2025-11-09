# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar tudo de uma vez
COPY . .

# Restaurar dependências do projeto Api
WORKDIR /src
RUN dotnet restore "src/Api/Api.csproj"

# Build do projeto
RUN dotnet build "src/Api/Api.csproj" -c Release -o /app/build --no-restore

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/challenge-moto-connect
RUN dotnet publish "src/Api/Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copiar os arquivos publicados
COPY --from=publish /app/publish .

# Definir o entrypoint
ENTRYPOINT ["dotnet", "Api.dll"]