using System;
using System.Reflection;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    internal delegate void ResizeArrayDelegate(bool unloading);

    internal sealed class ResizeArrayHook : ModSystem
    {
        public static event ResizeArrayDelegate OnResizeArrays;
        public static event ResizeArrayDelegate OnPostResizeArrays;

        public override void Load()
        {
            var resizeArrayMethod = typeof(ModContent).GetMethod("ResizeArrays", BindingFlags.NonPublic | BindingFlags.Static);
            MonoModHooks.Add(resizeArrayMethod, Delegate.CreateDelegate(typeof(Action<ResizeArrayDelegate, bool>), typeof(ResizeArrayHook).GetMethod(nameof(ResizeArrays), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        private static void ResizeArrays(ResizeArrayDelegate orig, bool unloading)
        {
            OnResizeArrays?.Invoke(unloading);
            orig(unloading);
            OnPostResizeArrays?.Invoke(unloading);
        }

        // MonoMod Hook Unloads before this being called
        // So we forcely call ResizeArray with Unload true
        public override void Unload()
        {
            OnResizeArrays?.Invoke(unloading: true);
            OnPostResizeArrays?.Invoke(unloading: true);

            OnResizeArrays = null;
            OnPostResizeArrays = null;
        }
    }
}
