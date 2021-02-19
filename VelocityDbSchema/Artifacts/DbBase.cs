using VelocityDb;

namespace RelSandbox
{
    public interface IDbBase
    {
    }

    class DbBase : OptimizedPersistable, IDbBase
    {
        //protected new bool Update()
        //{
        //    return base.Update();
        //}
    }
}
