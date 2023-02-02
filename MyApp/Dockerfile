# STAGE I: Base ------------------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app

EXPOSE 80

# STAGE II: Build Environment ----------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# install nodejs
RUN apt-get update -yq \
 && apt-get install -yq curl gnupg \
 && curl -sL https://deb.nodesource.com/setup_18.x | bash \
 && apt-get install -yq nodejs \
 && corepack enable

WORKDIR /src

COPY MyApp/MyApp.csproj MyApp/
RUN dotnet restore MyApp/MyApp.csproj

COPY . .

WORKDIR /src/MyApp

RUN dotnet build "MyApp.csproj" -c Release -o /app/build

# STAGE III: Publishing Environment ----------------------------------------------------------------

FROM build AS publish

RUN dotnet publish "MyApp.csproj" -c Release -o /app/publish

# STAGE IV: Server Runtime -------------------------------------------------------------------------

FROM base AS final

ARG BUILD_DATE
ARG BUILD_HASH

WORKDIR /app

COPY --from=publish /app/publish .

# https://github.com/opencontainers/image-spec/blob/main/annotations.md#pre-defined-annotation-keys
LABEL org.opencontainers.image.created="${BUILD_DATE}" \
      org.opencontainers.image.revision="${BUILD_HASH}"

ENTRYPOINT ["dotnet", "MyApp.dll"]
