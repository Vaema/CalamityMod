using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

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

        public static bool IsSubclass(Type baseType, Type type, bool includeBaseType)
        {
            return type.IsSubclassOf(baseType) && !type.IsAbstract && (!includeBaseType && type != baseType);
        }

        public static void IterateEveryModsTypesSorted<T>(bool includeBaseType = false, Action<Type> action = null)
        {
            // WHY????
            if (action is null)
                return;

            Type baseType = typeof(T);
            var types = GetEveryModsTypes().Where(t => IsSubclass(baseType, t, includeBaseType)).OrderBy(t => t.FullName);
            foreach (var type in types)
            {
                action.Invoke(type);
            }
        }

        public static void IterateEveryModsTypes<T>(bool includeBaseType = false, Action<Type> action)
        {
            // WHY????
            if (action is null)
                return;

            Type baseType = typeof(T);
            var types = GetEveryModsTypes().Where(t => IsSubclass(baseType, t, includeBaseType));
            foreach (var type in types)
            {
                action.Invoke(type);
            }
        }

        public static IEnumerable<Type> GetCalamityTypes()
        {
            return AssemblyManager.GetLoadableTypes(CalamityMod.Instance.Code);
        }

        public static void IterateCalamityTypesSorted<T>(bool includeBaseType = false, Action<Type> action = null)
        {
            // WHY????
            if (action is null)
                return;

            Type baseType = typeof(T);
            var types = GetCalamityTypes().Where(t => IsSubclass(baseType, t, includeBaseType)).OrderBy(t => t.FullName);
            foreach (var type in types)
            {
                action.Invoke(type);
            }
        }

        public static void IterateCalamityTypes<T>(bool includeBaseType = false, Action<Type> action = null)
        {
            // WHY????
            if (action is null)
                return;

            Type baseType = typeof(T);
            var types = GetCalamityTypes().Where(t => IsSubclass(baseType, t, includeBaseType));
            foreach (var type in types)
            {
                action.Invoke(type);
            }
        }
    }
}
