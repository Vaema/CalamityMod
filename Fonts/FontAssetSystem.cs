using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Fonts
{
    public sealed class FontAssetSystem : ModSystem
    {
        public static Asset<DynamicSpriteFont> MouseText => FontAssets.MouseText;

        public static Asset<DynamicSpriteFont> ItemStack => FontAssets.ItemStack;

        public static Asset<DynamicSpriteFont> DeathText => FontAssets.DeathText;

        public static Asset<DynamicSpriteFont> CombatText => FontAssets.CombatText[0];

        public static Asset<DynamicSpriteFont> CombatTextCrit => FontAssets.CombatText[1];

        public static Asset<DynamicSpriteFont> Wingdings => field ??= GetFont("Fonts/Wingdings");

        public static Asset<DynamicSpriteFont> CodebreakerDialog => field ??= GetFont("Fonts/CodebreakerDialog");

        public static Asset<DynamicSpriteFont> Impact => field ??= GetFont("Fonts/Impact");

        public static Asset<DynamicSpriteFont> Flexure => field ??= GetFont("Fonts/Flexure");

        private static Asset<DynamicSpriteFont> GetFont(string path)
        {
            return ModContent.GetInstance<CalamityMod>().Assets.Request<DynamicSpriteFont>(path, AssetRequestMode.ImmediateLoad);
        }
    }
}
