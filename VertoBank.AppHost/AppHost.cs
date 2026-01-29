using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres(nameof(VertoBank) + "Postgres")
    .WithDataVolume()
    .WithPgAdmin();

IResourceBuilder<PostgresDatabaseResource> moduleDb = postgres.AddDatabase("ModuleDb");

builder.AddProject<VertoBank>(nameof(VertoBank))
    .WithReference(moduleDb, "ModuleConnectionString")
    .WaitFor(postgres);

builder.Build().Run();
