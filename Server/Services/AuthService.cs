using Cord.Server.Application.Users;
using Grpc.Core;

namespace Cord.Server.Services;

public class AuthServiceImpl : AuthService.AuthServiceBase {
    readonly UserProvider userProvider;

    public AuthServiceImpl(UserProvider userProvider) {
        this.userProvider = userProvider;
    }

    public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context) {
        var user = await userProvider.GetByEmail(request.Email);

        if (user == null || !user.CheckPassword(request.Password)) {
            return new() { ErrorCode = ErrorCode.NotFound };
        }

        if (user.HasProperties(UserProperties.Bot)) {
            return new() { ErrorCode = ErrorCode.BadRequest };
        }

        return new() { ErrorCode = 0, Id = user.Id.Id };
    }

    public override async Task<LoginReply> LoginBot(LoginBotRequest request, ServerCallContext context) {
        var bot = await userProvider.GetBotByToken(request.Token);
        if (bot == null) {
            return new() { ErrorCode = ErrorCode.NotFound };
        }

        return new() { ErrorCode = 0, Id = bot.Id.Id };
    }
}
