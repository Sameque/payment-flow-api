FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGET_PROJECT
WORKDIR /src
COPY . .
RUN dotnet restore "${TARGET_PROJECT}"
RUN dotnet publish "${TARGET_PROJECT}" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet"]
