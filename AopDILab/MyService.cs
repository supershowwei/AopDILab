using System;
using System.Reflection;
using System.Reflection.Emit;
using Autofac;

namespace AopDILab
{
    public interface IMyService
    {
        MyData GetMyData_InjectDirectly();

        MyData GetMyData_InnerDelegate();

        MyData GetMyData_DynamicInvoke();

        MyData GetMyData_Delegate();

        MyData GetMyData_Reflection();
    }

    public class MyService : IMyService
    {
        private static readonly Action<object> MyDataAccessSetter = BuilMyDataAccessSetter();
        private static readonly Action<object> MyIdDataAccessSetter = BuilMyIdDataAccessSetter();

        private readonly ILifetimeScope lifetimeScope = AutofacConfig.Container.BeginLifetimeScope();
        private IMyDataAccess myDataAccess;
        private IMyIdDataAccess myIdDataAccess;

        public MyData GetMyData_InjectDirectly()
        {
            this.Inject();

            var myId = this.myIdDataAccess.GetMyId();

            return this.myDataAccess.QuerySingle(myId);
        }

        public MyData GetMyData_InnerDelegate()
        {
            MyDataAccessSetter(this);
            MyIdDataAccessSetter(this);

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
            var a = this;
            var b = a.lifetimeScope;
            a.myDataAccess = b.Resolve<IMyDataAccess>();
            a.myIdDataAccess = b.Resolve<IMyIdDataAccess>();
        }

        private static Action<object> BuilMyDataAccessSetter()
        {
            var lifetimeScopeField = typeof(MyService).GetField("lifetimeScope", BindingFlags.NonPublic | BindingFlags.Instance);

            var genericResolveMethod = typeof(ResolutionExtensions).GetMethod("Resolve", new[] { typeof(IComponentContext) }).MakeGenericMethod(typeof(IMyDataAccess));

            var injectMethod = new DynamicMethod("_MyDataAccess_Setter_", null, new[] { typeof(object) }, typeof(MyService), true);
            var ilGenerator = injectMethod.GetILGenerator();

            var lifetimeScopeLocalVariable = ilGenerator.DeclareLocal(lifetimeScopeField.FieldType);

            var field = typeof(MyService).GetField("myDataAccess", BindingFlags.NonPublic | BindingFlags.Instance);

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, lifetimeScopeField);
            ilGenerator.Emit(OpCodes.Stloc_0, lifetimeScopeLocalVariable);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldloc_0, lifetimeScopeLocalVariable);
            ilGenerator.Emit(OpCodes.Call, genericResolveMethod);
            ilGenerator.Emit(OpCodes.Stfld, field);
            ilGenerator.Emit(OpCodes.Ret);

            return injectMethod.CreateDelegate(typeof(Action<object>)) as Action<object>;
        }

        private static Action<object> BuilMyIdDataAccessSetter()
        {
            var lifetimeScopeField = typeof(MyService).GetField("lifetimeScope", BindingFlags.NonPublic | BindingFlags.Instance);

            var genericResolveMethod = typeof(ResolutionExtensions).GetMethod("Resolve", new[] { typeof(IComponentContext) }).MakeGenericMethod(typeof(IMyIdDataAccess));

            var injectMethod = new DynamicMethod("_MyIdDataAccess_Setter_", null, new[] { typeof(object) }, typeof(MyService), true);
            var ilGenerator = injectMethod.GetILGenerator();

            var lifetimeScopeLocalVariable = ilGenerator.DeclareLocal(lifetimeScopeField.FieldType);

            var field = typeof(MyService).GetField("myIdDataAccess", BindingFlags.NonPublic | BindingFlags.Instance);

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, lifetimeScopeField);
            ilGenerator.Emit(OpCodes.Stloc_0, lifetimeScopeLocalVariable);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldloc_0, lifetimeScopeLocalVariable);
            ilGenerator.Emit(OpCodes.Call, genericResolveMethod);
            ilGenerator.Emit(OpCodes.Stfld, field);
            ilGenerator.Emit(OpCodes.Ret);

            return injectMethod.CreateDelegate(typeof(Action<object>)) as Action<object>;
        }
    }
}