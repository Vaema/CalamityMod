using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace CalamityMod
{
    public static class ReflectionHelper
    {
        public static IEnumerable<Type> GetEveryModsTypes()
        {
            return ModLoader.Mods.SelectMany(mod => AssemblyManager.GetLoadableTypes(mod.Code));
        }

        public static bool IsSubclass(Type baseType, Type type, bool includeBaseType)
        {
            return type.IsSubclassOf(baseType) && !type.IsAbstract && (!includeBaseType && type != baseType);
        }
        
        // TODO: Consider replacing with ModType?
        public static void IterateEveryModsTypes<T>(Action<Type> action, bool includeBaseType = false)
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
        
        [Obsolete]
        private static IEnumerable<Type> GetCalamityTypes()
        {
            return AssemblyManager.GetLoadableTypes(CalamityMod.Instance.Code);
        }
        
        [Obsolete("Please avoid using this; ModType and ILoadable could probably be used instead.")]
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
