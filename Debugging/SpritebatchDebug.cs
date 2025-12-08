using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.ModLoader;

namespace CalamityMod.Debugging
{
    public static class SpritebatchDebug
    {
        public static string Trace { get; set; }
    }

    public class SpritebatchDebugInitializer : ILoadable
    {
        private static Hook _beginHook;
        private static Hook _endHook;
        
        public void Load(Mod mod)
        {
            MethodInfo begin = typeof(SpriteBatch).GetMethod(nameof(SpriteBatch.CheckBegin), BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo end = typeof(SpriteBatch).GetMethod(nameof(SpriteBatch.End), BindingFlags.Public | BindingFlags.Instance);
            
            Debug.Assert(begin != null);
            Debug.Assert(end != null);
            
            _beginHook = new Hook(begin, CheckBegin_Impl);
            _endHook = new Hook(end, End_Impl);
        }

        private static void CheckBegin_Impl(Action<SpriteBatch, string> orig, SpriteBatch self, string method)
        {
            if (self.beginCalled) ModLoader.GetMod(nameof(CalamityMod)).Logger.Debug(SpritebatchDebug.Trace); 
            SpritebatchDebug.Trace = Environment.StackTrace;
            
            orig(self, method);
        }

        private static void End_Impl(Action<SpriteBatch> orig, SpriteBatch self)
        {
            if (!self.beginCalled) ModLoader.GetMod(nameof(CalamityMod)).Logger.Debug(SpritebatchDebug.Trace); 
            SpritebatchDebug.Trace = Environment.StackTrace;
            
            orig(self);
        }
        
        public void Unload()
        {
            _beginHook.Dispose();
            _endHook.Dispose();
        }
    }
}
