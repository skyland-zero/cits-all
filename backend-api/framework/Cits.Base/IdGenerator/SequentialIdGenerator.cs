namespace Cits.IdGenerator;

public class SequentialIdGenerator : IIdGenerator
{
    public Guid Create()
    {
        return Guid.CreateVersion7();
    }
}