using System;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AopDILab
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //AutofacConfig.Register();

            //var myService = AutofacConfig.Container.Resolve<IMyService>();

            //var myData = myService.GetMyData_Delegate();

            //myData = myService.GetMyData_DynamicInvoke();

            //Console.WriteLine($"Id: {myData.Id}, Name: {myData.Name}");
            //Console.Read();

            var summary = BenchmarkRunner.Run<MyServiceBenchmark>();
            //var summary = BenchmarkRunner.Run<SetterLabBenchmark>();
        }
    }

    public class MyServiceBenchmark
    {
        private readonly IMyService myService;

        public MyServiceBenchmark()
        {
            AutofacConfig.Register();

            this.myService = AutofacConfig.Container.Resolve<IMyService>();
        }

        //[Benchmark]
        //public void InjectDirectly()
        //{
        //    var myData = this.myService.GetMyData_InjectDirectly();
        //}

        [Benchmark]
        public void InjectInnerDelegate()
        {
            var myData = this.myService.GetMyData_InnerDelegate();
        }

        [Benchmark]
        public void Delegate()
        {
            var myData = this.myService.GetMyData_Delegate();
        }

        //[Benchmark]
        //public void Reflection()
        //{
        //    var myData = this.myService.GetMyData_Reflection();
        //}

        //[Benchmark]
        //public void DynamicInvoke()
        //{
        //    var myData = this.myService.GetMyData_DynamicInvoke();
        //}
    }

    public class SetterLabBenchmark
    {
        private readonly SetterLab setterLafb;

        public SetterLabBenchmark()
        {
            this.setterLafb = new SetterLab();
        }

        [Benchmark]
        public void SetMyDataDirectly()
        {
            this.setterLafb.SetMyDataDirectly();
        }

        [Benchmark]
        public void SetMyDataDynamic()
        {
            this.setterLafb.SetMyDataDynamic();
        }
    }
}