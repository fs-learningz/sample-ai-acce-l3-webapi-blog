using System.Data;
using System.Globalization;
using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace BlogApi.Infrastructure.Persistence;

public class SqliteUserRepository(SqliteConnection connection) : IUserRepository
{
    private const string DefaultAdminUsername = "admin";
    private const string DefaultAdminPassword = "admin";

    private bool _initialized;

    public IReadOnlyList<User> GetAll()
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT u.id, u.username, u.email, u.password_hash, u.role_id, r.name,
                   u.is_active, u.is_default, u.created_at, u.updated_at
            FROM users u
            JOIN user_roles r ON r.id = u.role_id
            ORDER BY u.created_at ASC
            """;

        var users = new List<User>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(MapUser(reader));
        }

        return users;
    }

    public User? GetById(Guid id)
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT u.id, u.username, u.email, u.password_hash, u.role_id, r.name,
                   u.is_active, u.is_default, u.created_at, u.updated_at
            FROM users u
            JOIN user_roles r ON r.id = u.role_id
            WHERE u.id = @id
            """;
        command.Parameters.AddWithValue("@id", id.ToString());

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public User? GetByUsername(string username)
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT u.id, u.username, u.email, u.password_hash, u.role_id, r.name,
                   u.is_active, u.is_default, u.created_at, u.updated_at
            FROM users u
            JOIN user_roles r ON r.id = u.role_id
            WHERE u.username = @username COLLATE NOCASE
            """;
        command.Parameters.AddWithValue("@username", username);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public User Add(User user)
    {
        EnsureInitialized();

        ExecuteUpsert(user);
        return user;
    }

    public bool Update(User user)
    {
        EnsureInitialized();

        return ExecuteUpsert(user) == 1;
    }

    public IReadOnlyList<UserRole> GetRoles()
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM user_roles ORDER BY name ASC";

        var roles = new List<UserRole>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            roles.Add(new UserRole { Id = Guid.ParseExact(reader.GetString(0), "D"), Name = reader.GetString(1) });
        }

        return roles;
    }

    public UserRole? GetRoleByName(string name)
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM user_roles WHERE name = @name COLLATE NOCASE";
        command.Parameters.AddWithValue("@name", name);

        using var reader = command.ExecuteReader();
        return reader.Read()
            ? new UserRole { Id = Guid.ParseExact(reader.GetString(0), "D"), Name = reader.GetString(1) }
            : null;
    }

    public User GetDefaultAdmin()
    {
        EnsureInitialized();
        return GetByUsername(DefaultAdminUsername)!;
    }

    private int ExecuteUpsert(User user)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO users (id, username, email, password_hash, role_id, is_active, is_default, created_at, updated_at)
            VALUES (@id, @username, @email, @passwordHash, @roleId, @isActive, @isDefault, @createdAt, @updatedAt)
            ON CONFLICT(id) DO UPDATE SET
                email = @email,
                password_hash = @passwordHash,
                role_id = @roleId,
                is_active = @isActive,
                updated_at = @updatedAt
            """;
        command.Parameters.AddWithValue("@id", user.Id.ToString());
        command.Parameters.AddWithValue("@username", user.Username);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@roleId", user.RoleId.ToString());
        command.Parameters.AddWithValue("@isActive", user.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@isDefault", user.IsDefault ? 1 : 0);
        command.Parameters.AddWithValue("@createdAt", user.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@updatedAt", user.UpdatedAt.HasValue ? user.UpdatedAt.Value.ToString("O") : DBNull.Value);

        return command.ExecuteNonQuery();
    }

    private static User MapUser(SqliteDataReader reader) => new()
    {
        Id = Guid.ParseExact(reader.GetString(0), "D"),
        Username = reader.GetString(1),
        Email = reader.GetString(2),
        PasswordHash = reader.GetString(3),
        RoleId = Guid.ParseExact(reader.GetString(4), "D"),
        RoleName = reader.GetString(5),
        IsActive = reader.GetInt64(6) == 1,
        IsDefault = reader.GetInt64(7) == 1,
        CreatedAt = DateTime.Parse(reader.GetString(8), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        UpdatedAt = reader.IsDBNull(9)
            ? null
            : DateTime.Parse(reader.GetString(9), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
    };

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using (var createRolesCommand = connection.CreateCommand())
        {
            createRolesCommand.CommandText = """
                CREATE TABLE IF NOT EXISTS user_roles (
                    id TEXT PRIMARY KEY,
                    name TEXT NOT NULL UNIQUE
                )
                """;
            createRolesCommand.ExecuteNonQuery();
        }

        using (var createUsersCommand = connection.CreateCommand())
        {
            createUsersCommand.CommandText = """
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    username TEXT NOT NULL UNIQUE,
                    email TEXT NOT NULL,
                    password_hash TEXT NOT NULL,
                    role_id TEXT NOT NULL,
                    is_active INTEGER NOT NULL DEFAULT 0,
                    is_default INTEGER NOT NULL DEFAULT 0,
                    created_at TEXT NOT NULL,
                    updated_at TEXT
                )
                """;
            createUsersCommand.ExecuteNonQuery();
        }

        SeedRolesAndDefaultAdmin();

        _initialized = true;
    }

    private void SeedRolesAndDefaultAdmin()
    {
        foreach (var roleName in new[] { RoleNames.Admin, RoleNames.Author })
        {
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM user_roles WHERE name = @name";
            checkCommand.Parameters.AddWithValue("@name", roleName);
            if (Convert.ToInt64(checkCommand.ExecuteScalar()) > 0)
            {
                continue;
            }

            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO user_roles (id, name) VALUES (@id, @name)";
            insertCommand.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            insertCommand.Parameters.AddWithValue("@name", roleName);
            insertCommand.ExecuteNonQuery();
        }

        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM users";
        if (Convert.ToInt64(countCommand.ExecuteScalar()) > 0)
        {
            return;
        }

        using var roleIdCommand = connection.CreateCommand();
        roleIdCommand.CommandText = "SELECT id FROM user_roles WHERE name = @name";
        roleIdCommand.Parameters.AddWithValue("@name", RoleNames.Admin);
        var adminRoleId = (string)roleIdCommand.ExecuteScalar()!;

        var defaultAdmin = new User
        {
            Id = Guid.NewGuid(),
            Username = DefaultAdminUsername,
            Email = "admin@blog-api.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultAdminPassword),
            RoleId = Guid.ParseExact(adminRoleId, "D"),
            RoleName = RoleNames.Admin,
            IsActive = true,
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };

        ExecuteUpsert(defaultAdmin);
    }
}
