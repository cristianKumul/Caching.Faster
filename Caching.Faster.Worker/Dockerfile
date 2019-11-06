FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Caching.Faster.Worker/Caching.Faster.Worker.csproj", "Caching.Faster.Worker/"]
RUN dotnet restore "Caching.Faster.Worker/Caching.Faster.Worker.csproj"
COPY . .
WORKDIR "/src/Caching.Faster.Worker"
RUN dotnet build "Caching.Faster.Worker.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Caching.Faster.Worker.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Caching.Faster.Worker.dll"]