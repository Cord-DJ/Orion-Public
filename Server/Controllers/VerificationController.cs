using Cord.Server.Application.Users;
using Cord.Server.Application.Verification;
using Cord.Server.Domain;
using Cord.Server.Domain.Verification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

[Route("api/verification")]
[ApiController]
public class VerificationController : CordControllerBase {
    readonly IMediator mediator;
    readonly ICaptcha captcha;
    readonly IWebHostEnvironment webHostEnvironment;

    public VerificationController(
        UserProvider userProvider,
        IMediator mediator,
        ICaptcha captcha,
        IWebHostEnvironment webHostEnvironment
    ) : base(userProvider) {
        this.mediator = mediator;
        this.captcha = captcha;
        this.webHostEnvironment = webHostEnvironment;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Get(string id) {
        if (id == "@me") {
            var user = await GetSender();
            return Ok(user.Verified);
        }

        throw new StaffOnlyException();
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> Create() {
        await mediator.Send(new CreateVerificationCommand(await GetSender(), VerificationType.EmailVerification));
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Verify([FromBody] EmailVerificationModel model) {
        if (webHostEnvironment.IsProduction() && !await captcha.Verify(model.Captcha)) {
            throw new BadRequestException("captcha");
        }

        await mediator.Send(new VerifyCommand(model.Code));
        return NoContent();
    }

    public record EmailVerificationModel(string Code, string Captcha);
}
