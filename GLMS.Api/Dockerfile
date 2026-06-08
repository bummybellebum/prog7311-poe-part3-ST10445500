# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
ARG APP_UID=1000
ARG APP_GID=1000
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Ensure uploads folder exists and is writable. Set permissive mode and adjust ownership
# if build/run supplies APP_UID/APP_GID. This makes the /app/uploads mount usable by the
# non-root runtime user and prevents permission denied errors when seeding files.
RUN mkdir -p /app/uploads/signed-agreements \
	&& chmod -R 0775 /app/uploads || true
RUN if [ -n "${APP_UID}" ]; then chown -R ${APP_UID}:${APP_GID} /app/uploads || true; fi

# Switch to non-root user if provided (Visual Studio/dev setups may pass APP_UID at build)
USER ${APP_UID}


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GLMS.Api/GLMS.Api.csproj", "GLMS.Api/"]
RUN dotnet restore "./GLMS.Api/GLMS.Api.csproj"
COPY . .
WORKDIR "/src/GLMS.Api"
RUN dotnet build "./GLMS.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GLMS.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GLMS.Api.dll"]
