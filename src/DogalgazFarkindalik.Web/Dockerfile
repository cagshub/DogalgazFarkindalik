FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/DogalgazFarkindalik.Domain/DogalgazFarkindalik.Domain.csproj", "DogalgazFarkindalik.Domain/"]
COPY ["src/DogalgazFarkindalik.Application/DogalgazFarkindalik.Application.csproj", "DogalgazFarkindalik.Application/"]
COPY ["src/DogalgazFarkindalik.Web/DogalgazFarkindalik.Web.csproj", "DogalgazFarkindalik.Web/"]
RUN dotnet restore "DogalgazFarkindalik.Web/DogalgazFarkindalik.Web.csproj"

COPY src/ .
WORKDIR "/src/DogalgazFarkindalik.Web"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DogalgazFarkindalik.Web.dll"]
