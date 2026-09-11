var builder = DistributedApplication.CreateBuilder(args);

var dataPath = Path.Combine(builder.AppHostDirectory, "data");
var sqlite = builder.AddSqlite("sqlite", dataPath, "blog.db")
    .WithSqliteWeb(static x => x.WithHttpEndpoint(port: 10001));

builder.AddProject<Projects.BlogApi_Api>("blogapi")
    .WithReference(sqlite)
    .WithExternalHttpEndpoints()
    .WithHttpEndpoint(10010)
    .WithHttpsEndpoint(10011);

builder.Build().Run();
