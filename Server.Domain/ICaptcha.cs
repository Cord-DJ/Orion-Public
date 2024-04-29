namespace Cord.Server.Domain;

public interface ICaptcha {
    Task<bool> Verify(string token);
}
