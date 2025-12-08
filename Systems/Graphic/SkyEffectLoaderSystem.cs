using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Skies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    [Autoload(Side = ModSide.Client)]
    internal sealed class SkyEffectLoaderSystem : ModSystem
    {
        public override void Load()
        {
            Filters.Scene["CalamityMod:DevourerofGodsHead"] = new Filter(new DoGScreenShaderData("FilterMiniTower").UseOpacity(0.025f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:DevourerofGodsHead"] = new DoGSky();

            Filters.Scene["CalamityMod:CalamitasRun3"] = new Filter(new CalScreenShaderData("FilterMiniTower").UseColor(1.1f, 0.3f, 0.3f).UseOpacity(0.2f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:CalamitasRun3"] = new CalSky();

            Filters.Scene["CalamityMod:PlaguebringerGoliath"] = new Filter(new PbGScreenShaderData("FilterMiniTower").UseColor(0.2f, 0.6f, 0.2f).UseOpacity(0.35f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:PlaguebringerGoliath"] = new PbGSky();

            Filters.Scene["CalamityMod:Yharon"] = new Filter(new YScreenShaderData("FilterMiniTower").UseColor(1f, 0.4f, 0f).UseOpacity(0.75f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:Yharon"] = new YSky();

            Filters.Scene["CalamityMod:Leviathan"] = new Filter(new LevScreenShaderData("FilterMiniTower").UseColor(0f, 0f, 0.5f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:Leviathan"] = new LevSky();

            Filters.Scene["CalamityMod:SupremeCalamitas"] = new Filter(new SCalScreenShaderData("FilterMiniTower").UseColor(Color.Transparent).UseOpacity(0f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:SupremeCalamitas"] = new SCalSky();

            Filters.Scene["CalamityMod:Signus"] = new Filter(new SignusScreenShaderData("FilterMiniTower").UseColor(0.35f, 0.1f, 0.55f).UseOpacity(0.35f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:Signus"] = new SignusSky();

            SkyManager.Instance["CalamityMod:BossRush"] = new BossRushSky();

            Filters.Scene["CalamityMod:ExoMechs"] = new Filter(new ExoMechsScreenShaderData("FilterMiniTower").UseColor(ExoMechsSky.DrawColor).UseOpacity(0.25f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:ExoMechs"] = new ExoMechsSky();

            Filters.Scene["CalamityMod:MonolithAccursed"] = new Filter(new MonolithScreenShaderData("FilterMiniTower").UseColor(1.1f, 0.3f, 0.3f).UseOpacity(0.65f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:MonolithAccursed"] = new MonolithSky();

            // Normal intensity is 4f
            Texture2D DistortionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/BlobbyNoise", AssetRequestMode.ImmediateLoad).Value;
            Filters.Scene["CalamityMod:DrunkCrabulon"] = new Filter(new DrunkCrabScreenShaderData("FilterHeatDistortion").UseImage(DistortionTexture, 0, null).UseIntensity(20f), EffectPriority.VeryHigh);

            Filters.Scene["CalamityMod:BrimstoneCrag"] = new Filter(new MonolithScreenShaderData("FilterMiniTower").UseColor(0f, 0f, 0f).UseOpacity(0f), EffectPriority.VeryHigh);
            SkyManager.Instance["CalamityMod:BrimstoneCrag"] = new BrimstoneCragSky();

            SkyManager.Instance["CalamityMod:Astral"] = new AstralSky();
            SkyManager.Instance["CalamityMod:SulphurSea"] = new SulphurSeaSky();
            SkyManager.Instance["CalamityMod:Cryogen"] = new CryogenSky();
            SkyManager.Instance["CalamityMod:StormWeaverFlash"] = new StormWeaverFlashSky();
        }
    }
}
