using Terraria;
using Terraria.ModLoader;
using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using CalamityMod.Systems;

namespace CalamityMod.BiomeManagers
{
    //this is just a global sunken sea biome to check if you are in any of the existing biomes
    public class SunkenSeaBiome : ModBiome
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override void Load()
        {
            //apply the drawblack edits here since all sunken sea biomes will have custom backgrounds
            IL_Main.DrawBlack += ChangeBlackThreshold;
            On_Main.DrawBlack += ForceDrawBlack;
        }

        private void ForceDrawBlack(On_Main.orig_DrawBlack orig, Main self, bool force)
        {
            if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.SunkenSeaBiome>()) && Main.BackgroundEnabled)
            {
                orig(self, true);
            }
            else
            {
                orig(self, force);
            }
        }

        private float NewThreshold(float orig)
        {
            if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.SunkenSeaBiome>()) && Main.BackgroundEnabled)
            {
                return 0.1f;
            }
            else
            {
                return orig;
            }
        }

        private void ChangeBlackThreshold(ILContext il)
        {
            if (Main.BackgroundEnabled)
            {
                var c = new ILCursor(il);
                c.TryGotoNext(n => n.MatchLdloc(6), n => n.MatchStloc(13)); //beginning of the loop, local 11 is a looping variable
                c.Index++; //this is kinda goofy since I dont think you could actually ever write c# to compile to the resulting IL from emitting here.
                c.Emit(OpCodes.Ldloc, 3); //pass the original value so we can set that instead if we dont want to change the threshold
                c.EmitDelegate<Func<float, float>>(NewThreshold); //check if were in the biome to set, else set the original value
                c.Emit(OpCodes.Stloc, 3); //num2 in vanilla, controls minimum threshold to turn a tile black
            }
        }
        
        public override string BestiaryIcon => "CalamityMod/BiomeManagers/SunkenSeaIcon";
        // Placeholder until we get a dedicated Sunken Sea background
        public override string BackgroundPath => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer1";
        public override string MapBackground => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer1";

        public override bool IsBiomeActive(Player player)
        {
            return BiomeTileCounterSystem.SunkenSeaBurrowsTiles > 1000 || BiomeTileCounterSystem.SunkenSeaPolypTiles > 1000 ||
            BiomeTileCounterSystem.SunkenSeaReefsTiles > 1000 || BiomeTileCounterSystem.SunkenSeaShoresTiles > 1000;
        }
    }
}
