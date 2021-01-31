using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AopDILab
{
    public class SetterLab
    {
        private static readonly FieldInfo MyDataField = typeof(SetterLab).GetField("myData", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly Action<MyData, object> MyDataSetter = BuildMyDataSetter();
        private MyData myData;

        public void SetMyDataDirectly()
        {
            this.myData = new MyData();
        }

        public void SetMyDataByILCode()
        {
            MyDataSetter(new MyData(), this);
        }

        public void SetMyDataByReflection()
        {
            MyDataField.SetValue(this, new MyData());
        }

        private static Action<MyData, object> BuildMyDataSetter()
        {
            var setterMethod = new DynamicMethod("Set_MyData_By_ILCode", null, new[] { typeof(MyData), typeof(object) }, typeof(SetterLab), true);
            var ilGenerator = setterMethod.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Stfld, MyDataField);
            ilGenerator.Emit(OpCodes.Ret);

            return setterMethod.CreateDelegate(typeof(Action<MyData, object>)) as Action<MyData, object>;
        }
    }
}