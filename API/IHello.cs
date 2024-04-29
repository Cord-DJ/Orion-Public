namespace Cord;

public interface IHello {
    int HelloInterval { get; }
}

sealed record HelloImpl(int HelloInterval) : IHello;
