using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Infrastructure.Persistence;

public class UserRepository(BlogDbContext dbContext) : IUserRepository
{
    public IReadOnlyList<User> GetAll() =>
        dbContext.Users.OrderBy(u => u.Username).ToList();

    public User? GetById(Guid id) => dbContext.Users.Find(id);

    public User? GetByUsername(string username) =>
        dbContext.Users.FirstOrDefault(u => u.Username == username);

    public bool ExistsByUsername(string username) =>
        dbContext.Users.Any(u => u.Username == username);

    public User Add(User user)
    {
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
        return user;
    }

    public bool Update(User user)
    {
        dbContext.Users.Update(user);
        return dbContext.SaveChanges() > 0;
    }

    public bool Delete(Guid id)
    {
        var user = dbContext.Users.Find(id);
        if (user is null)
        {
            return false;
        }

        dbContext.Users.Remove(user);
        return dbContext.SaveChanges() > 0;
    }
}
