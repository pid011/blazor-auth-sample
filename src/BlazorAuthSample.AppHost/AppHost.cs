var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", true);

var db = builder.AddPostgres("postgres", password: postgresPassword, port: 25544)
.WithContainerName("blazorauthsample-db")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("db");

builder.AddProject<Projects.BlazorAuthSample>("server")
    .WithReplicas(2)
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
