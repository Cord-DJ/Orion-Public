using Cord.Server.Application.Equipment;
using Cord.Server.Application.Users;
using Cord.Server.Domain;
using Cord.Server.Domain.Relationships;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

[Route("api/users")]
[ApiController]
public partial class UsersController : CordControllerBase {
    readonly EquipmentService equipmentService;
    readonly IRelationshipRepository relationshipRepository;
    readonly IMediator mediator;
    readonly ICaptcha captcha;

    public UsersController(
        UserProvider userProvider,
        EquipmentService equipmentService,
        IRelationshipRepository relationshipRepository,
        IMediator mediator,
        ICaptcha captcha
    ) : base(userProvider) {
        this.equipmentService = equipmentService;
        this.relationshipRepository = relationshipRepository;
        this.mediator = mediator;
        this.captcha = captcha;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IUser> Get(string id) {
        if (id == "@me") {
            return await GetSender();
        }

        throw new StaffOnlyException();
    }


    [HttpGet("{id}/rooms")]
    [Authorize]
    public async IAsyncEnumerable<IRoom> GetJoinedRooms(string id) {
        var user = await ParseUserId(id, true);

        await foreach (var room in userProvider.GetJoinedRooms(user)) {
            yield return room;
        }
    }

    [HttpPost]
    public async Task<IUser> Create(
        [FromBody]
        CreateUser model,
        [FromServices]
        IWebHostEnvironment webHostEnvironment
    ) {
        if (webHostEnvironment.IsProduction() && !await captcha.Verify(model.Captcha)) {
            throw new BadRequestException("captcha");
        }

        return await mediator.Send(new CreateUserCommand(model.Email, model.Password, model.Name));
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IUser> Update(string id, [FromBody] UpdateUser model) {
        var user = await ParseUserId(id, true);
        return await mediator.Send(new UpdateUserCommand(user, model));
    }

    [Authorize]
    [HttpPost("{id}/update-password")]
    public async Task<IActionResult> UpdatePassword(string id, [FromBody] UpdatePassword model) {
        var user = await ParseUserId(id, true);

        await mediator.Send(new UpdatePasswordCommand(user, model.CurrentPassword, model.NewPassword));
        return NoContent();
    }

    [Authorize]
    [HttpPost("update-email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailCommand command) {
        await mediator.Send(command);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id}/update-email-request")]
    public async Task<IActionResult> ChangeEmailRequest(string id) {
        var user = await ParseUserId(id, true);
        var (code, sendEmail) = await mediator.Send(new UpdateEmailRequestCommand(user));

        return sendEmail ? NoContent() : Ok(code);
    }

    [Authorize]
    [HttpPost("reset-password-request")]
    public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequest model) {
        if (!await captcha.Verify(model.Captcha)) {
            throw new BadRequestException("captcha");
        }

        await mediator.Send(new ResetPasswordRequestCommand(model.Email));
        return NoContent();
    }

    [Authorize]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command) {
        await mediator.Send(command);
        return NoContent();
    }
}

public record CreateUser(string Email, string Password, string Name, string Captcha);

public record UpdatePassword(string CurrentPassword, string NewPassword);

public record ResetPasswordRequest(string Email, string Captcha);
