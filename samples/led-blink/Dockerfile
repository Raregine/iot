#FROM microsoft/dotnet:2.1-sdk-stretch AS build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /app

# copy csproj and restore as distinct layers
# COPY *.config .
COPY *.csproj .
RUN dotnet restore

# copy and build app
COPY . .
RUN dotnet publish -c release -o out

#FROM microsoft/dotnet:2.1-runtime-stretch-slim-arm32v7 AS runtime
FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim-arm32v7
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "led-blink.dll"]
