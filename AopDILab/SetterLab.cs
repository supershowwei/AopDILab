using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AopDILab
{
    public class SetterLab
    {
        private static readonly Action<MyData, object> MyDataSetter = BuildMyDataSetter();
        private MyData myData;

        public void SetMyDataDirectly()
        {
            this.myData = new MyData();
        }

        public void SetMyDataDynamic()
        {
            MyDataSetter(new MyData(), this);
        }

        private static Action<MyData, object> BuildMyDataSetter()
        {
            var setterMethod = new DynamicMethod("Set_MyData_Dynamic", null, new[] { typeof(MyData), typeof(object) }, typeof(SetterLab), true);
            var ilGenerator = setterMethod.GetILGenerator();

            var instanceLocalVariable = ilGenerator.DeclareLocal(typeof(SetterLab));

            var myDataField = typeof(SetterLab).GetField("myData", BindingFlags.NonPublic | BindingFlags.Instance);

            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stloc_0, instanceLocalVariable);
            ilGenerator.Emit(OpCodes.Ldloc_0, instanceLocalVariable);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Stfld, myDataField);
            ilGenerator.Emit(OpCodes.Ret);

            return setterMethod.CreateDelegate(typeof(Action<MyData, object>)) as Action<MyData, object>;
        }
    }
}