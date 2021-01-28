using Autofac;

namespace AopDILab
{
    public interface IMyService
    {
        MyData GetMyData_InjectDirectly();

        MyData GetMyData_DynamicInvoke();

        MyData GetMyData_Delegate();

        MyData GetMyData_Reflection();
    }

    public class MyService : IMyService
    {
        private readonly ILifetimeScope lifetimeScope = AutofacConfig.Container.BeginLifetimeScope();
        private IMyDataAccess myDataAccess;
        private IMyIdDataAccess myIdDataAccess;

        public MyData GetMyData_InjectDirectly()
        {
            this.Inject();

            var myId = this.myIdDataAccess.GetMyId();

            return this.myDataAccess.QuerySingle(myId);
        }

        [DependencyInjectionDynamicInvokeAspect]
        [DependencyInject(typeof(IMyDataAccess), typeof(IMyIdDataAccess))]
        public MyData GetMyData_DynamicInvoke()
        {
            var myId = this.myIdDataAccess.GetMyId();

            return this.myDataAccess.QuerySingle(myId);
        }

        [DependencyInjectionAspect]
        [DependencyInject(typeof(IMyDataAccess), typeof(IMyIdDataAccess))]
        public MyData GetMyData_Delegate()
        {
            var myId = this.myIdDataAccess.GetMyId();

            return this.myDataAccess.QuerySingle(myId);
        }

        [DependencyInjectionReflectionAspect]
        [DependencyInject(typeof(IMyDataAccess), typeof(IMyIdDataAccess))]
        public MyData GetMyData_Reflection()
        {
            var myId = this.myIdDataAccess.GetMyId();

            return this.myDataAccess.QuerySingle(myId);
        }

        private void Inject()
        {
            this.myDataAccess = this.lifetimeScope.Resolve<IMyDataAccess>();
            this.myIdDataAccess = this.lifetimeScope.Resolve<IMyIdDataAccess>();
        }
    }
}