using System;
using System.Reflection.Emit;

namespace RfgTools.Helpers
{
    public static class SizeHelper
    {
        public static int GetTypeSize<T>()
        {
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, typeof(T));
            il.Emit(OpCodes.Ret);
            return (int)dm.Invoke(null, null);
        }
    }
}
