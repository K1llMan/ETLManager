FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app

FROM microsoft/aspnetcore-build:2.0 AS build
ARG Configuration=Release
WORKDIR /src
# Copy configs
COPY Config/*.xml Config/
# Copy projects
COPY ETLService/ETLService.csproj ETLService/
COPY ETLCommon/ETLCommon.csproj ETLCommon/

COPY ETLApp/ETLApp.csproj ETLApp/
COPY ETLProgram/ETLProgram.csproj ETLProgram/
COPY ETLProgramCommon/ETLProgramCommon.csproj ETLProgramCommon/

# Copy ETL config
COPY ETLService/ETLSettings.docker.json ETLService/ETLSettings.json

# Restore dependencies
RUN dotnet restore ETLService/ETLService.csproj
RUN dotnet restore ETLApp/ETLApp.csproj

# Build projects
COPY . .
WORKDIR /src/ETLService
RUN dotnet build ETLService.csproj -c $Configuration -o /app
WORKDIR /src/ETLApp
RUN dotnet build ETLApp.csproj -c $Configuration -o /app

WORKDIR /src
FROM build AS publish
ARG Configuration=Release
# Publish
RUN dotnet publish /ETLService/ETLService.csproj -c $Configuration -o /app
RUN dotnet publish /ETLApp/ETLApp.csproj -c $Configuration -o /app

#FROM microsoft/aspnetcore-build:2.0 AS publish
#COPY Release/ app/
#RUN dotnet restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ETLService.dll"]
