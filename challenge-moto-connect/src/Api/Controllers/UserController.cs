using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;
using challenge_moto_connect.Application.DTOs;
using challenge_moto_connect.Application.Services;
using challenge_moto_connect.Application.DTOs.Pagination;
using challenge_moto_connect.Application.DTOs.HATEOAS;
using System.Text.Json;

namespace challenge_moto_connect.Api.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    [Produces(MediaTypeNames.Application.Json)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers([FromQuery] PaginationParams paginationParams)
        {
            var pagedUsers = await _userService.GetPagedUsersAsync(paginationParams);

            var metadata = new
            {
                pagedUsers.TotalCount,
                pagedUsers.PageSize,
                pagedUsers.CurrentPage,
                pagedUsers.TotalPages,
                pagedUsers.HasNext,
                pagedUsers.HasPrevious
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));

            foreach (var user in pagedUsers.Items)
            {
                user.Links.Add(new LinkDto(Url.Link(nameof(GetUser), new { id = user.UserID }), "self", "GET"));
                user.Links.Add(new LinkDto(Url.Link(nameof(PutUser), new { id = user.UserID }), "update_user", "PUT"));
                user.Links.Add(new LinkDto(Url.Link(nameof(DeleteUser), new { id = user.UserID }), "delete_user", "DELETE"));
            }

            return Ok(pagedUsers.Items);
        }

        [HttpGet("{id:guid}", Name = nameof(GetUser))]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDTO>> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Links.Add(new LinkDto(Url.Link(nameof(GetUser), new { id = user.UserID }), "self", "GET"));
            user.Links.Add(new LinkDto(Url.Link(nameof(PutUser), new { id = user.UserID }), "update_user", "PUT"));
            user.Links.Add(new LinkDto(Url.Link(nameof(DeleteUser), new { id = user.UserID }), "delete_user", "DELETE"));

            return Ok(user);
        }

        [HttpPut("{id:guid}", Name = nameof(PutUser))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutUser(Guid id, UserDTO userDto)
        {
            if (id != userDto.UserID)
            {
                return BadRequest();
            }

            try
            {
                await _userService.UpdateUserAsync(id, userDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost(Name = nameof(PostUser))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDTO>> PostUser(UserDTO userDto)
        {
            var createdUser = await _userService.CreateUserAsync(userDto);

            createdUser.Links.Add(new LinkDto(Url.Link(nameof(GetUser), new { id = createdUser.UserID }), "self", "GET"));
            createdUser.Links.Add(new LinkDto(Url.Link(nameof(PutUser), new { id = createdUser.UserID }), "update_user", "PUT"));
            createdUser.Links.Add(new LinkDto(Url.Link(nameof(DeleteUser), new { id = createdUser.UserID }), "delete_user", "DELETE"));

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.UserID }, createdUser);
        }

        [HttpDelete("{id:guid}", Name = nameof(DeleteUser))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}

