using Api.Dto;
using Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController(UserService userService, AuditRecordService auditRecordService, ILogger<UserController> logger) : AuditRecordControllerBase(auditRecordService)
{

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid userId)
    {
        var cancellationToken = CancellationToken.None;
        var user = await userService.GetUserById(userId, cancellationToken);
        if (user is null)
        {
            return NotFound($"User with ID {userId} was not found");
        }

        return user.ToDto();
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<UserDto>>> ListUsers(ListUsersRequest? request)
    {
        Guid? cursor = null;
        if (request?.PaginationOptions?.Cursor is not null)
        {
            if (Guid.TryParse(request.PaginationOptions.Cursor, out var c))
            {
                cursor = c;
            }
            else
            {
                return BadRequest("The given cursor is not valid.");
            }

        }

        var cancellationToken = CancellationToken.None;
        var pageSize = request?.PaginationOptions?.PageSize ?? 50;
        var users = await userService.ListUsers(cursor, pageSize, cancellationToken);

        var hasMoreResults = users.Count == pageSize;

        var page = new PaginatedResponse<UserDto>()
        {
            Items = users.Select(u => u.ToDto()),
            NextCursor = hasMoreResults ? users.Last().Id.ToString() : null
        };

        return page;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest options)
    {
        var cancellationToken = CancellationToken.None;
        var user = await userService.CreateUser(options, cancellationToken);
        logger.LogInformation("User created. ID {}", user.Id);
        var routeValues = new { userId = user.Id };
        return CreatedAtAction(nameof(GetUser), routeValues, user.ToDto());
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid userId, UpdateUserRequest options)
    {
        var cancellationToken = CancellationToken.None;
        var user = await userService.UpdateUser(userId, options, cancellationToken);
        if (user is null)
        {
            return NotFound($"User with ID {userId} was not found");
        }

        logger.LogInformation("User updated. ID {}", user.Id);
        return user.ToDto();
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult<UserDto>> DeleteUser(Guid userId)
    {
        var cancellationToken = CancellationToken.None;
        var user = await userService.DeleteUser(userId, cancellationToken);
        if (user is null)
        {
            return NotFound($"User with ID {userId} was not found");
        }

        logger.LogInformation("User deleted. ID {}", user.Id);
        return Ok(user.ToDto());
    }

    [HttpPut("{userId}/undelete")]
    public async Task<ActionResult<UserDto>> UndeleteUser(Guid userId)
    {
        var cancellationToken = CancellationToken.None;
        var user = await userService.UnDeleteUser(userId, cancellationToken);
        if (user is null)
        {
            return NotFound($"Deleted user with ID {userId} was not found");
        }

        logger.LogInformation("User undeleted. ID {}", user.Id);
        var routeValues = new { userId = user.Id };
        return CreatedAtAction(nameof(GetUser), routeValues, user.ToDto());
    }
}
