FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Second.slnx", "./"]
COPY ["src/Presentation/Second.API/Second.API.csproj", "src/Presentation/Second.API/"]
COPY ["src/Infrastructure/Second.Persistence/Second.Persistence.csproj", "src/Infrastructure/Second.Persistence/"]
COPY ["src/Infrastructure/Second.Infrastructure/Second.Infrastructure.csproj", "src/Infrastructure/Second.Infrastructure/"]
COPY ["src/Core/Second.Application/Second.Application.csproj", "src/Core/Second.Application/"]
COPY ["src/Core/Second.Domain/Second.Domain.csproj", "src/Core/Second.Domain/"]
RUN dotnet restore "src/Presentation/Second.API/Second.API.csproj"

COPY . .
RUN dotnet publish "src/Presentation/Second.API/Second.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Second.API.dll"]
