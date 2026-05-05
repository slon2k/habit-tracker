FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/HabitTracker.Api/HabitTracker.Api.csproj", "src/HabitTracker.Api/"]
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]
RUN dotnet restore "src/HabitTracker.Api/HabitTracker.Api.csproj"

COPY ["src/HabitTracker.Api/", "src/HabitTracker.Api/"]
WORKDIR "/src/src/HabitTracker.Api"
RUN dotnet publish "HabitTracker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS="http://+:8080;https://+:8443"
EXPOSE 8080 8443

ENTRYPOINT ["dotnet", "HabitTracker.Api.dll"]
