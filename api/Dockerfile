# Build the image using the dotnet 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:8ab06772f296ed5f541350334f15d9e2ce84ad4b3ce70c90f2e43db2752c30f6 AS build
WORKDIR /app

COPY . .

RUN dotnet restore
RUN dotnet publish -o out

# Create the container image using the runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:6159cf66274cf52730d7a0c7bb05cf0af94b79370176886ac58286ab6cbb7faf
WORKDIR /app

COPY --from=build /app/out .
# Do not rely on the host's dotnet installation
ENTRYPOINT ["dotnet", "OtelTester.Api.dll"]
