using BlogApi.Domain.Entities;

namespace BlogApi.Application.Interfaces;

public interface IUserRepository
{
    IReadOnlyList<User> GetAll();
    User? GetById(Guid id);
    User? GetByUsername(string username);
    bool ExistsByUsername(string username);
    User Add(User user);
    bool Update(User user);
    bool Delete(Guid id);
}
