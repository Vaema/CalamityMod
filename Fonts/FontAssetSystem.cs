using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Projectiles.Boss;
using CalamityMod.UI.DraedonSummoning;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Fonts
{
    public class FontAssetSystem : ModSystem
    {
        public static readonly Dictionary<string, DynamicSpriteFont> Fonts = new() {
            { "MouseText", FontAssets.MouseText.Value },
            { "ItemStack", FontAssets.ItemStack.Value},
            { "DeathText", FontAssets.DeathText.Value },
            { "CombatText", FontAssets.CombatText[0].Value },
            { "CombatTextCrit", FontAssets.CombatText[1].Value }
        };

        public override void OnModLoad()
        {
            AddFont("Wingdings", CalamityMod.Instance.Assets.Request<DynamicSpriteFont>("Fonts/Wingdings", AssetRequestMode.ImmediateLoad).Value);
            AddFont("CodebreakerDialog", CalamityMod.Instance.Assets.Request<DynamicSpriteFont>("Fonts/CodebreakerDialog", AssetRequestMode.ImmediateLoad).Value);
            AddFont("Impact", CalamityMod.Instance.Assets.Request<DynamicSpriteFont>("Fonts/Impact", AssetRequestMode.ImmediateLoad).Value);
            AddFont("Flexure", CalamityMod.Instance.Assets.Request<DynamicSpriteFont>("Fonts/Flexure", AssetRequestMode.ImmediateLoad).Value);
        }

        private static void AddFont(string key, DynamicSpriteFont font, DynamicSpriteFont fallback = null)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                Fonts.Add(key, font);
            else
                Fonts.Add(key, fallback);
        }
    }
}
