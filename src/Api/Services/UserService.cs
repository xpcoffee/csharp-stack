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
        var user = await _context.Users.FindAsync(userId);

        if (user?.DeletedAuditRecordId is not null)
        {
            return null;
        }

        return user;
    }

    public async Task<List<User>> ListUsers(Guid? cursor, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsQueryable();

        if (cursor.HasValue)
        {
            query = query.Where(u => u.Id < cursor.Value);
        }

        return await query
          .Where(u => u.DeletedAuditRecordId == null)
          .OrderByDescending(u => u.Id)
          .Take(pageSize)
          .ToListAsync(cancellationToken);
    }

    public async Task<User> CreateUser(CreateUserRequest options, CancellationToken cancellationToken)
    {
        var user = options.ToUser();
        _context.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> DeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null || user.DeletedAuditRecordId is not null)
        {
            return null;
        }

        user.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UnDeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null || user.DeletedAuditRecordId is null)
        {
            return null;
        }

        user.IsDeleted = false;
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> DropUser(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            return null;
        }
        _context.Remove(user);

        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateUser(Guid userId, UpdateUserRequest options, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null || user.DeletedAuditRecordId is not null)
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
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
        };
    }

    public static User ToUser(this CreateUserRequest dto)
    {
        return new User()
        {
            Name = dto.Name,
            Email = dto.Email,
        };
    }
}
