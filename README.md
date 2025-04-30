# OpenTelemetry Tester

Repository with a docker container ready to get deployed in Azure
for testing OpenTelemetry capabilities.

# Development

To build the container type the following:
```bash
docker build -t oteltester -f Dockerfile .
```

To run the API locally without Docker, install the .NET 8 SDK and:
```bash
dotnet run
```
