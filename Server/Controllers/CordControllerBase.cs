using Cord.Server.Application.Users;
using Cord.Server.Domain.Users;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cord.Server.Controllers;

public class CordControllerBase : ControllerBase {
    protected readonly UserProvider userProvider;

    protected ID SenderId {
        get {
            if (!ID.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)) {
                throw new UnauthorizedException();
            }

            return id;
        }
    }

    public CordControllerBase(UserProvider userProvider) {
        this.userProvider = userProvider;
    }

    protected Task<User> GetSender() => userProvider.GetUser(SenderId);

    protected ID ParseId(string id) => id == "@me" ? SenderId : ID.Parse(id);

    protected async Task<User> ParseUserId(string id, bool onlyMe = false) {
        if (id == "@me" || ID.Parse(id) == SenderId) {
            return await GetSender();
        }

        if (onlyMe && !(await GetSender()).HasProperties(UserProperties.Staff)) {
            throw new StaffOnlyException();
        }

        return await userProvider.GetUser(ID.Parse(id));
    }
}
