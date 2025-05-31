using Api.Dto;
using Api.Service;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(UserService userService) : ControllerBase
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
    public async Task<ActionResult<PaginatedResponse<UserDto>>> ListUsers(PaginationOptionsDto? paginationOptions)
    {
        Guid? cursor = null;
        if (paginationOptions?.Cursor is not null)
        {
            if (!Guid.TryParse(paginationOptions.Cursor, out var c))
            {
                cursor = c;
                return BadRequest("The given cursor is not valid.");
            }

        }

        var cancellationToken = CancellationToken.None;
        var pageSize = paginationOptions?.PageSize ?? 50;
        var users = await userService.ListUsers(cursor, pageSize, cancellationToken);

        var hasMoreResults = users.Count == pageSize;

        var page = new PaginatedResponse<UserDto>()
        {
            Items = users.Select(u => u.ToDto()),
            NextCursor = hasMoreResults ? users.Last().Id.ToString() : null
        };

        return page;
    }

    [HttpPut]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto options)
    {
        var cancellationToken = CancellationToken.None;
        var user = await userService.CreateUser(options, cancellationToken);
        return Created();
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid userId, UpdateUserDto options)
    {
        var cancellationToken = CancellationToken.None;
        var user = await userService.UpdateUser(userId, options, cancellationToken);
        if (user is null)
        {
            return NotFound($"User with ID {userId} was not found");
        }

        return user.ToDto();
    }
}
