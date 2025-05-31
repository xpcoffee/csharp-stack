using Api.Data;
using Api.Dto;
using Api.Model;
using Microsoft.EntityFrameworkCore;

namespace Api.Service;


public class UserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<List<User>> ListUsers(Guid? cursor, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsQueryable()
          .OrderBy(u => u.Id)
          .Take(pageSize);

        if (cursor.HasValue)
        {
            query = query.Where(u => u.Id > cursor.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<User?> CreateUser(CreateUserDto options, CancellationToken cancellationToken)
    {

        var user = options.ToUser();
        _context.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateUser(Guid userId, UpdateUserDto options, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            return null;
        }

        if (options.Email is not null)
        {
            user.Email = options.Email;
        }

        if (options.Name is not null)
        {
            user.Name = options.Name;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}


public static class UserServiceLayerExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto()
        {
            Email = user.Email,
            Name = user.Name,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public static User ToUser(this CreateUserDto dto)
    {
        return new User()
        {
            Name = dto.Name,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
