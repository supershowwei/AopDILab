namespace AopDILab
{
    public interface IMyDataAccess
    {
        MyData QuerySingle(int id);
    }

    public class MyDataAccess : IMyDataAccess
    {
        public MyData QuerySingle(int id)
        {
            return new MyData { Id = id, Name = "Johnny" };
        }
    }
}