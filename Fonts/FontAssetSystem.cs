#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Fonts
{
    public sealed class FontAssetSystem : ModSystem
    {
        public static readonly Dictionary<string, Asset<DynamicSpriteFont>?> Fonts = new() {
            { "MouseText", FontAssets.MouseText },
            { "ItemStack", FontAssets.ItemStack },
            { "DeathText", FontAssets.DeathText  },
            { "CombatText", FontAssets.CombatText[0] },
            { "CombatTextCrit", FontAssets.CombatText[1] }
        };

        public static Asset<DynamicSpriteFont> MouseText => FontAssets.MouseText;

        public static Asset<DynamicSpriteFont> ItemStack => FontAssets.ItemStack;

        public static Asset<DynamicSpriteFont> DeathText => FontAssets.DeathText;

        public static Asset<DynamicSpriteFont> CombatText => FontAssets.CombatText[0];

        public static Asset<DynamicSpriteFont> CombatTextCrit => FontAssets.CombatText[1];

        // TODO: We could implement pretty simple caching here with the `field`
        // keyword, but not everyone has a newer SDK installed.  Wait until tML
        // updates to .NET 10.
        public static Asset<DynamicSpriteFont>? Wingdings => ExpectFont(nameof(Wingdings));

        public static Asset<DynamicSpriteFont>? CodebreakerDialog => ExpectFont(nameof(CodebreakerDialog));

        public static Asset<DynamicSpriteFont>? Impact => ExpectFont(nameof(Impact));

        public static Asset<DynamicSpriteFont>? Flexure => ExpectFont(nameof(Flexure));

        public override void OnModLoad()
        {
            if (Main.dedServ)
                return;

            // Intentionally does not have a fallback due to it already having null checks where it is utilized to prevent them from appearing
            AddFont("Wingdings", Mod.Assets.Request<DynamicSpriteFont>("Fonts/Wingdings"), fallbackFont: null);
            AddFont("CodebreakerDialog", Mod.Assets.Request<DynamicSpriteFont>("Fonts/CodebreakerDialog"), FontAssets.MouseText);
            AddFont("Impact", Mod.Assets.Request<DynamicSpriteFont>("Fonts/Impact"), FontAssets.CombatText[1]);
            AddFont("Flexure", Mod.Assets.Request<DynamicSpriteFont>("Fonts/Flexure"), FontAssets.CombatText[1]);
        }

        private static void AddFont(string key, Asset<DynamicSpriteFont> newFont, Asset<DynamicSpriteFont>? fallbackFont)
        {
            var font = OperatingSystem.IsWindows()
                ? newFont
                : fallbackFont;

            Fonts.Add(key, font);
        }

        public static Asset<DynamicSpriteFont>? ExpectFont(string fontName)
        {
            if (!Fonts.TryGetValue(fontName, out var font) || font is null)
            {
                return null;
            }

            font.Wait();
            return font;
        }
    }
}
