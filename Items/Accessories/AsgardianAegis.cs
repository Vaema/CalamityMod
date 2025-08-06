using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class AsgardianAegis : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";

        public Color? TooltipExtensionColor => new(195, 223, 255);

        public const int ShieldSlamDamage = 1000;
        public const float ShieldSlamKnockback = 15f;
        public const int ShieldSlamIFrames = 12;

        public const int RamExplosionDamage = 300;
        public const float RamExplosionKnockback = 20f;

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 54;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.defense = 6;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();

            // Asgardian Aegis ram dash
            modPlayer.DashID = AsgardianAegisDash.ID;
            player.dashType = 0;

            // Inherited Ankh Shield effects
            player.noKnockback = true;
            player.fireWalk = true;

            //Immune to all major Fire debuffs before this point
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[ModContent.BuffType<BrimstoneFlames>()] = true;
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[ModContent.BuffType<Daybroken>()] = true;
            player.buffImmune[ModContent.BuffType<HolyFlames>()] = true;
            player.buffImmune[ModContent.BuffType<GodSlayerInferno>()] = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AsgardsValor>().
                AddIngredient<ElysianAegis>().
                AddIngredient<CosmiliteBar>(10).
                AddIngredient<AscendantSpiritEssence>(4).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
