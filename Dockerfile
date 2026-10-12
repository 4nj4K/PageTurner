# Byggfasen
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PageTurner.csproj", "./"]
RUN dotnet restore "PageTurner.csproj"
COPY . .
RUN dotnet publish PageTurner.csproj -c Release -o /app/publish

# Körfasen
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080
ENTRYPOINT ["dotnet", "PageTurner.dll"]