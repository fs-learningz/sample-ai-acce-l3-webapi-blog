var builder = DistributedApplication.CreateBuilder(args);

// SQLite database resource. The database file lives in a stable location
// (AppHostFolder/data) so it persists across restarts and can be browsed by
// the sqlite-web container, which mounts the same directory.
var databasePath = Path.GetFullPath(
    Path.Combine(builder.AppHostDirectory ?? Directory.GetCurrentDirectory(), "data"));

var sqlite = builder.AddSqlite("sqlite", databasePath, "blog.db");

// sqlite-web: web-based SQLite browser, exposed on static port 10001.
sqlite.WithSqliteWeb(
    configureContainer: web => web.WithEndpoint("http", endpoint => endpoint.Port = 10001),
    containerName: "sqlite-web");

builder.AddProject<Projects.Blog_Api>("blog-api")
    .WithHttpEndpoint(port: 10010, name: "http", isProxied: false)
    .WithHttpsEndpoint(port: 10011, name: "https", isProxied: false)
    .WithReference(sqlite)
    .WaitFor(sqlite);

builder.Build().Run();
