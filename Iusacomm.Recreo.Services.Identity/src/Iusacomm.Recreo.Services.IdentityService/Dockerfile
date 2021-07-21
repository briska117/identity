#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update
RUN apt-get install -y nano

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY NuGet.Config ./
COPY ["Iusacomm.Recreo.Services.IdentityService/Iusacomm.Recreo.Services.IdentityService.csproj", "Iusacomm.Recreo.Services.IdentityService/"]
RUN dotnet restore "Iusacomm.Recreo.Services.IdentityService/Iusacomm.Recreo.Services.IdentityService.csproj"
COPY . .
WORKDIR "/src/Iusacomm.Recreo.Services.IdentityService"
RUN dotnet build "Iusacomm.Recreo.Services.IdentityService.csproj" -c Release -o /app

COPY Iusacomm.Recreo.Services.IdentityService/Email /app/Email/
COPY Iusacomm.Recreo.Services.IdentityService/images /app/images/


FROM build AS publish
RUN dotnet publish "Iusacomm.Recreo.Services.IdentityService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Iusacomm.Recreo.Services.IdentityService.dll"]