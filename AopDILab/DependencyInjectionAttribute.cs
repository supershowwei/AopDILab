using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectInjector.Broker;
using Autofac;

namespace AopDILab
{
    [Aspect(Scope.PerInstance)]
    [Injection(typeof(DependencyInjectionAspectAttribute))]
    public class DependencyInjectionAspectAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<string, Action<object>> InjectMethods = new ConcurrentDictionary<string, Action<object>>();
        private static readonly ConcurrentDictionary<Type, Dictionary<Type, FieldInfo>> InstanceFields = new ConcurrentDictionary<Type, Dictionary<Type, FieldInfo>>();
        private static readonly MethodInfo ResolveMethod = typeof(ResolutionExtensions).GetMethod("Resolve", new[] { typeof(IComponentContext) });

        [Advice(Kind.Before, Targets = Target.Method)]
        public void Before([Argument(Source.Metadata)] MethodBase method, [Argument(Source.Instance)] object instance)
        {
            var instanceType = instance.GetType();

            var privateFields = InstanceFields.GetOrAdd(
                instanceType,
                type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(fi => fi.FieldType, fi => fi));

            var injectedTypes = method.GetCustomAttribute<DependencyInjectAttribute>()?.Types;

            //if (injectedTypes == null) throw new ArgumentNullException("injectedTypes");
            //if (injectedTypes.Length == 0) throw new ArgumentException("Empty injected types", "injectedTypes");

            foreach (var injectedType in injectedTypes)
            {
                var field = privateFields[injectedType];

                var injectMethodName = $"Inject_{field.Name}_On_{instanceType.Name}";

                var inject = InjectMethods.GetOrAdd(
                    injectMethodName,
                    methodName =>
                        {
                            var lifetimeScopeField = privateFields[typeof(ILifetimeScope)];

                            //if (lifetimeScopeField == null) throw new ArgumentNullException("lifetimeScope");

                            //if (ResolveMethod == null) throw new MissingMethodException("Autofac.ResolutionExtensions", "Resolve");

                            var genericResolveMethod = ResolveMethod.MakeGenericMethod(injectedType);

                            var injectMethod = new DynamicMethod(injectMethodName, null, new[] { typeof(object) }, true);
                            var ilGenerator = injectMethod.GetILGenerator();

                            var lifetimeScopeLocalVariable = ilGenerator.DeclareLocal(lifetimeScopeField.FieldType);

                            ilGenerator.Emit(OpCodes.Ldarg_0);
                            ilGenerator.Emit(OpCodes.Ldfld, lifetimeScopeField);
                            ilGenerator.Emit(OpCodes.Stloc, lifetimeScopeLocalVariable);
                            ilGenerator.Emit(OpCodes.Ldarg_0);
                            ilGenerator.Emit(OpCodes.Ldloc, lifetimeScopeLocalVariable);
                            ilGenerator.Emit(OpCodes.Call, genericResolveMethod);
                            ilGenerator.Emit(OpCodes.Stfld, field);
                            ilGenerator.Emit(OpCodes.Ret);
                            
                            return injectMethod.CreateDelegate(typeof(Action<object>)) as Action<object>;
                        });

                inject(instance);
            }
        }
    }

    [Aspect(Scope.PerInstance)]
    [Injection(typeof(DependencyInjectionDynamicInvokeAspectAttribute))]
    public class DependencyInjectionDynamicInvokeAspectAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<string, Delegate> InjectMethods = new ConcurrentDictionary<string, Delegate>();
        private static readonly ConcurrentDictionary<Type, Dictionary<Type, FieldInfo>> InstanceFields = new ConcurrentDictionary<Type, Dictionary<Type, FieldInfo>>();
        private static readonly MethodInfo ResolveMethod = typeof(ResolutionExtensions).GetMethod("Resolve", new[] { typeof(IComponentContext) });

        [Advice(Kind.Before, Targets = Target.Method)]
        public void Before([Argument(Source.Metadata)] MethodBase method, [Argument(Source.Instance)] object instance)
        {
            var instanceType = instance.GetType();

            var privateFields = InstanceFields.GetOrAdd(
                instanceType,
                type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(fi => fi.FieldType, fi => fi));

            var injectedTypes = method.GetCustomAttribute<DependencyInjectAttribute>()?.Types;

            //if (injectedTypes == null) throw new ArgumentNullException("injectedTypes");
            //if (injectedTypes.Length == 0) throw new ArgumentException("Empty injected types", "injectedTypes");

            foreach (var injectedType in injectedTypes)
            {
                var field = privateFields[injectedType];

                var injectMethodName = $"Inject_{field.Name}_DynamicInvoke_On_{instanceType.Name}";

                var inject = InjectMethods.GetOrAdd(
                    injectMethodName,
                    methodName =>
                        {
                            var lifetimeScopeField = privateFields[typeof(ILifetimeScope)];

                            //if (lifetimeScopeField == null) throw new ArgumentNullException("lifetimeScope");

                            //if (ResolveMethod == null) throw new MissingMethodException("Autofac.ResolutionExtensions", "Resolve");

                            var genericResolveMethod = ResolveMethod.MakeGenericMethod(injectedType);

                            var injectMethod = new DynamicMethod(injectMethodName, null, new[] { instanceType }, true);
                            var ilGenerator = injectMethod.GetILGenerator();

                            var lifetimeScopeLocalVariable = ilGenerator.DeclareLocal(lifetimeScopeField.FieldType);

                            ilGenerator.Emit(OpCodes.Ldarg_0);
                            ilGenerator.Emit(OpCodes.Ldfld, lifetimeScopeField);
                            ilGenerator.Emit(OpCodes.Stloc, lifetimeScopeLocalVariable);
                            ilGenerator.Emit(OpCodes.Ldarg_0);
                            ilGenerator.Emit(OpCodes.Ldloc, lifetimeScopeLocalVariable);
                            ilGenerator.Emit(OpCodes.Call, genericResolveMethod);
                            ilGenerator.Emit(OpCodes.Stfld, field);
                            ilGenerator.Emit(OpCodes.Ret);

                            return injectMethod.CreateDelegate(typeof(Action<>).MakeGenericType(instanceType));
                        });

                inject.DynamicInvoke(instance);
            }
        }
    }

    [Aspect(Scope.PerInstance)]
    [Injection(typeof(DependencyInjectionReflectionAspectAttribute))]
    public class DependencyInjectionReflectionAspectAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<Type, FieldInfo>> InstanceFields = new ConcurrentDictionary<Type, Dictionary<Type, FieldInfo>>();

        [Advice(Kind.Before, Targets = Target.Method)]
        public void Before([Argument(Source.Metadata)] MethodBase method, [Argument(Source.Instance)] object instance)
        {
            var instanceType = instance.GetType();

            var privateFields = InstanceFields.GetOrAdd(
                instanceType,
                type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToDictionary(fi => fi.FieldType, fi => fi));

            var lifetimeScope = privateFields[typeof(ILifetimeScope)].GetValue(instance) as ILifetimeScope;

            var injectedTypes = method.GetCustomAttribute<DependencyInjectAttribute>()?.Types;

            foreach (var injectedType in injectedTypes)
            {
                var field = privateFields[injectedType];

                field.SetValue(instance, lifetimeScope.Resolve(injectedType));
            }
        }
    }

    public class DependencyInjectAttribute : Attribute
    {
        public DependencyInjectAttribute(params Type[] types)
        {
            this.Types = types;
        }

        public Type[] Types { get; }
    }

    public class AutofacConfig
    {
        public static IContainer Container { get; private set; }

        public static void Register()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MyDataAccess>().As<IMyDataAccess>().InstancePerLifetimeScope();
            builder.RegisterType<MyIdDataAccess>().As<IMyIdDataAccess>().InstancePerLifetimeScope();
            builder.RegisterType<MyService>().As<IMyService>();

            Container = builder.Build();
        }
    }

    public class ServiceCollection
    {
        private readonly ConcurrentDictionary<Type, object> services;

        public ServiceCollection()
        {
            this.services = new ConcurrentDictionary<Type, object>();
        }

        public void Add(Type type, object instance)
        {
            this.services.TryAdd(type, instance);
        }

        public object Get(Type type)
        {
            return this.services.TryGetValue(type, out var instance) ? instance : default;
        }
    }
}