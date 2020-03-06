FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Caching.Faster.Proxy/Caching.Faster.Proxy.csproj" --configfile Nuget.config
RUN dotnet build "Caching.Faster.Proxy/Caching.Faster.Proxy.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Caching.Faster.Proxy/Caching.Faster.Proxy.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN dotnet dev-certs https
ENTRYPOINT ["dotnet", "Caching.Faster.Proxy.dll"]