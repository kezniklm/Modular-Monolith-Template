using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("VertoBankPostgres").WithDataVolume();

var moduleDb = postgres.AddDatabase("ModuleDb");

builder.AddProject<VertoBank>(nameof(VertoBank)).WithReference(moduleDb, "ModuleConnectionString");

builder.Build().Run();
