using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.FishingRods
{
    public class VerstaltiteFishingRod : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";

        public static float FishingPowerBiomeMult = 1.1f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FishingPowerBiomeMult.ToString());

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 35;
            Item.shootSpeed = 15f;
            Item.shoot = ModContent.ProjectileType<VerstaltiteBobber>();
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override bool AllowPrefix(int pre)
        {
            if (pre == 0)
                return true;
            return false;
        }

        public override bool CanReforge()
        {
            return false;
        }
        public override void HoldItem(Player player)
        {
            player.accFishingLine = true;
            if (player.Calamity().SelectedFishingMinigame == CalamityPlayer.FishingMinigames.None)
                player.Calamity().SelectedFishingMinigame = CalamityPlayer.FishingMinigames.VerstaltiteFishingRod;
        }

        public override void UpdateEquip(Player player)
        {
            player.Calamity().SelectedFishingMinigame = CalamityPlayer.FishingMinigames.VerstaltiteFishingRod;
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(43f, -36f);
            lineColor = new Color(95, 158, 160, 100);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CryonicBar>(6).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
