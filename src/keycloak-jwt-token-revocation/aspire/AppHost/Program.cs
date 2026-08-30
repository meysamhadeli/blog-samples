var builder = DistributedApplication.CreateBuilder(args);

var postgresUser = builder.AddParameter("postgres-user", "postgres", secret: false);
var postgresPassword = builder.AddParameter("postgres-password", "postgres", secret: false);
var postgres = builder.AddPostgres("postgres", postgresUser, postgresPassword)
    .WithImageTag("17")
    .WithDataVolume("keycloak-postgres-data-v2");
var keycloakDatabase = postgres.AddDatabase("keycloakdb");
var realmImportPath = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath, "..", "..", "keycloak", "realm-export.json"));

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.7.2")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", "jdbc:postgresql://postgres:5432/keycloakdb")
    .WithEnvironment("KC_DB_USERNAME", "postgres")
    .WithEnvironment("KC_DB_PASSWORD", "postgres")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithBindMount(realmImportPath, "/opt/keycloak/data/import/realm-export.json", isReadOnly: true)
    .WithArgs("start-dev", "--import-realm")
    .WaitFor(keycloakDatabase);

var backend = builder.AddProject<Projects.Products_Api>("backend-api")
    .WithHttpEndpoint(port: 5001, name: "http")
    .WithHttpsEndpoint(port: 5000, name: "https")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Keycloak__Authority", "http://localhost:8080/realms/demo")
    .WithEnvironment("Keycloak__IntrospectionClientId", "api-introspection")
    .WithEnvironment("Keycloak__IntrospectionClientSecret", "api-introspection-demo-secret")
    .WaitFor(keycloak);

builder.AddExecutable("frontend", @"C:\Program Files\nodejs\node.exe", "../../frontend")
    .WithArgs(@"C:\Program Files\nodejs\node_modules\npm\bin\npm-cli.js", "start", "--", "--port", "4200")
    .WithHttpEndpoint(port: 3000, targetPort: 4200, name: "http")
    .WithEnvironment("NODE_ENV", "development")
    .WaitFor(backend);

builder.Build().Run();
