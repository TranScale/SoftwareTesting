FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Online Sales Management System.csproj", "./"]
RUN dotnet restore "Online Sales Management System.csproj"

COPY . .
RUN dotnet publish "Online Sales Management System.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} dotnet \"Online Sales Management System.dll\""]
