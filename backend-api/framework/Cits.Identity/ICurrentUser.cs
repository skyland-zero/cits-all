namespace Cits;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    public Guid Id { get; }
    public string UserName { get; }
    public string Surname { get; }
}