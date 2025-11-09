# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution and restore
COPY chalenge-moto-connect.sln ./
COPY src/Api/Api.csproj src/Api/Api.csproj
COPY src/Application/Application.csproj src/Application/Application.csproj
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/Infrastructure.csproj
COPY src/Domain/Domain.csproj src/Domain/Domain.csproj
RUN dotnet restore ./src/Api/Api.csproj -r linux-x64

# copy full source and publish
COPY . .
RUN dotnet publish src/Api/Api.csproj -c Release -r linux-x64 -o /app/publish --no-restore

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]