# OpenTelemetry .NET Tester

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

# Azure Deployments

Bicep files are provided to create the base resources with the `az`
or the `azd` CLI.

To create the resources, type in your terminal:
```bash
azd up
```

Make sure to choose an alphanumeric environment name to successfully
create all resources.

Clean up resources with:
```
azd down
```
