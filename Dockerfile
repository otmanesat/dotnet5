FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["HelloDotNet5.csproj", "./"]
RUN dotnet restore "HelloDotNet5.csproj"
COPY . .
RUN dotnet build "HelloDotNet5.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HelloDotNet5.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HelloDotNet5.dll"]
