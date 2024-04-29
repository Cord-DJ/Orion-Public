namespace Cord.Server.Domain.Hub;

public interface IGatewaySender {
    Task Send(IUser user, Func<IGatewayAPI, Task> callback);
    Task Send(IRoom room, Func<IGatewayAPI, Task> callback);
    Task Send(IRoom room, Func<IGatewayAPI, IUser, Task> callback);

    Task SendToAllVisibleUsersOfUser(IUser user, Func<IGatewayAPI, Task> callback);
    Task SendToAllVisibleUsersOfUser(IUser user, Func<IGatewayAPI, IUser, Task> callback);
}
