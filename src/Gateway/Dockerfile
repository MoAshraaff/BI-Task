FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/BITask.Shared.csproj", "src/Shared/"]
COPY ["src/Gateway/Gateway.csproj", "src/Gateway/"]
RUN dotnet restore "src/Gateway/Gateway.csproj"
COPY src/Shared/ src/Shared/
COPY src/Gateway/ src/Gateway/
WORKDIR "/src/src/Gateway"
RUN dotnet publish "Gateway.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gateway.dll"]
