namespace AopDILab
{
    public interface IMyIdDataAccess
    {
        int GetMyId();
    }

    public class MyIdDataAccess : IMyIdDataAccess
    {
        public int GetMyId()
        {
            return 2;
        }
    }
}