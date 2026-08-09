FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/BITask.Shared.csproj", "src/Shared/"]
COPY ["src/Services/ProductService/ProductService.csproj", "src/Services/ProductService/"]
RUN dotnet restore "src/Services/ProductService/ProductService.csproj"
COPY src/Shared/ src/Shared/
COPY src/Services/ProductService/ src/Services/ProductService/
WORKDIR "/src/src/Services/ProductService"
RUN dotnet publish "ProductService.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"]
