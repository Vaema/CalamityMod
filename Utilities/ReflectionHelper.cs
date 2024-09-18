using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ModLoader.Core;
using Terraria.ModLoader;
using System.Collections;
using CalamityMod.Particles;

namespace CalamityMod
{
    public class FastField<T, ValueType>(FieldInfo fieldInfo)
    {
        private Action<T, ValueType> _Setter = ReflectionHelper.CreateFieldSetter<T, ValueType>(fieldInfo);
        private Func<T, ValueType> _Getter = ReflectionHelper.CreateFieldGetter<T, ValueType>(fieldInfo);

        public FastField(string fieldName, BindingFlags flags) : this(typeof(T).GetField(fieldName, flags))
        {

        }

        public ValueType Get(T instance)
        {
            return _Getter(instance);
        }

        public void Set(T instance, ValueType newValue)
        {
            _Setter(instance, newValue);
        }
    }

    public static class ReflectionHelper
    {
        public static Action<T, ValueType> CreateFieldSetter<T, ValueType>(FieldInfo fieldInfo)
        {
            string methodName = fieldInfo.ReflectedType.FullName + ".set_" + fieldInfo.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, null, [typeof(T), typeof(ValueType)], true);
            ILGenerator gen = setterMethod.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, fieldInfo);
            gen.Emit(OpCodes.Ret);

            return (Action<T, ValueType>)setterMethod.CreateDelegate(typeof(Action<T, ValueType>));
        }

        public static Func<T, ValueType> CreateFieldGetter<T, ValueType>(FieldInfo fieldInfo)
        {
            string methodName = fieldInfo.ReflectedType.FullName + ".get_" + fieldInfo.Name;
            DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(ValueType), [typeof(T)], true);
            ILGenerator gen = getterMethod.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, fieldInfo);
            gen.Emit(OpCodes.Ret);

            return (Func<T, ValueType>)getterMethod.CreateDelegate(typeof(Func<T, ValueType>));
        }

        public static IEnumerable<Type> GetEveryModsTypes()
        {
            return ModLoader.Mods.SelectMany(mod => AssemblyManager.GetLoadableTypes(mod.Code));
        }

        public static void IterateEveryModsTypes<T>(bool includeBaseType = false, Action<Type> action = null)
        {
            // WHY????
            if (action is null)
                return;

            Type baseType = typeof(T);
            var types = GetEveryModsTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(baseType) && !type.IsAbstract && (!includeBaseType && type != baseType))
                {
                    action.Invoke(type);
                }
            }
        }

        public static IEnumerable<Type> GetCalamityTypes()
        {
            return AssemblyManager.GetLoadableTypes(CalamityMod.Instance.Code);
        }

        public static void IterateCalamityTypes<T>(bool includeBaseType, Action<Type> action)
        {
            // WHY????
            if (action is null)
                return;

            Type baseType = typeof(T);
            var types = GetCalamityTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(baseType) && !type.IsAbstract && (!includeBaseType && type != baseType))
                {
                    action.Invoke(type);
                }
            }
        }
    }
}
