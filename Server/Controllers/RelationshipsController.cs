using Cord.Server.Domain.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

public partial class UsersController {
    [Authorize]
    [HttpGet("{userId}/relationships")]
    public async IAsyncEnumerable<Relationship> GetRoleMembers(string userId) {
        var id = ParseId(userId);

        if (id == SenderId) {
            await foreach (var relationship in relationshipRepository.GetUserRelationships(id)) {
                yield return relationship;
            }
        }
        // TODO: Get mutual
    }
}
