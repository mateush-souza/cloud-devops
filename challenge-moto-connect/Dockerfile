# Etapa 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solução e projetos
COPY challenge-moto-connect.sln ./
COPY src/Api/Api.csproj src/Api/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Domain/Domain.csproj src/Domain/

# Restaura dependências
RUN dotnet restore src/Api/Api.csproj -r linux-x64

# Copia tudo e publica
COPY . .
RUN dotnet publish src/Api/Api.csproj -c Release -r linux-x64 -o /out --no-restore

# Etapa 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Define a porta e URLs
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copia os artefatos publicados da etapa anterior
COPY --from=build /out .

# Nome correto da DLL (baseado no seu .csproj)
ENTRYPOINT ["dotnet", "Api.dll"]

