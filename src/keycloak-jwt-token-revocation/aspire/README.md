# Aspire development host

Start Keycloak, backend API, and Angular frontend together:

```powershell
dotnet run --project aspire/AppHost/AppHost.csproj
```

Requirements:

- .NET 10 SDK
- Docker Desktop running for the Keycloak container
- Node.js and npm for Angular

Endpoints:

- Aspire dashboard: URL printed by AppHost
- Frontend: http://localhost:3000
- Backend HTTP: http://localhost:5001
- Backend HTTPS: https://localhost:5000
- Keycloak: http://localhost:8080
