FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ETLService/ETLService.csproj ETLService/
COPY ETLCommon/ETLCommon.csproj ETLCommon/
RUN dotnet restore ETLService/ETLService.csproj
COPY . .
WORKDIR /src/ETLService
RUN dotnet build ETLService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ETLService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ETLService.dll"]
