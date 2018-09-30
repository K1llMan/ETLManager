FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY Config/General.xml Config/
COPY ETLService/ETLService.csproj ETLService/
COPY ETLService/ETLSettings.docker.json ETLService/ETLSettings.json
COPY ETLCommon/ETLCommon.csproj ETLCommon/
RUN dotnet restore ETLService/ETLService.csproj
COPY . .
WORKDIR /src/ETLService
RUN dotnet build ETLService.csproj -c Release -o /app

WORKDIR /src
COPY ETLApp/ETLApp.csproj ETLApp/
COPY ETLProgram/ETLProgram.csproj ETLProgram/
COPY ETLProgramCommon/ETLProgramCommon.csproj ETLProgramCommon/
RUN dotnet restore ETLApp/ETLApp.csproj
COPY . .
WORKDIR /src/ETLApp
RUN dotnet build ETLApp.csproj -c Release -o /app

WORKDIR /src/ETLService
FROM build AS publish
RUN dotnet publish ETLService.csproj -c Release -o /app

#FROM microsoft/aspnetcore-build:2.0 AS publish
#COPY Release/ app/
#RUN dotnet restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ETLService.dll"]
