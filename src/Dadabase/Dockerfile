#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY ["DadABase.Tests/", "DadABase.Tests/"]
COPY ["DadABase.Web/", "DadABase.Web/"]

WORKDIR /src/DadABase.Web
RUN dotnet restore

FROM build AS publish
RUN dotnet publish -o /app

FROM base AS final
WORKDIR /app
EXPOSE 8080
#ENV ASPNETCORE_URLS=http://localhost:8080
#EXPOSE 8080
#EXPOSE 443
#EXPOSE 7273
#EXPOSE 5178
#ENV ASPNETCORE_URLS=http://localhost:8080
ENV ASPNETCORE_URLS="http://+"
#EXPOSE 5000
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DadABase.Web.dll"]