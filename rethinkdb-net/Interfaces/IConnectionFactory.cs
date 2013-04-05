namespace RethinkDb
{
    public interface IConnectionFactory
    {
        IConnection Get(string name);
    }
}
