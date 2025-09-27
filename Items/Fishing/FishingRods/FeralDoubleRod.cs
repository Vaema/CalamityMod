using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.FishingRods
{
    public class FeralDoubleRod : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 40;
            Item.shootSpeed = 16f;
            Item.shoot = ModContent.ProjectileType<FeralDoubleBobber>();
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
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
            if (player.Calamity().SelectedFishingMinigame == CalamityPlayer.FishingMinigames.None)
                player.Calamity().SelectedFishingMinigame = CalamityPlayer.FishingMinigames.FeralDoubleRod;
            Item.fishingPole = 40 +(int)((1f + (player.statLifeMax2 - player.statLife) * 0.25f));

        }

        public override void UpdateEquip(Player player)
        {
            player.Calamity().SelectedFishingMinigame = CalamityPlayer.FishingMinigames.FeralDoubleRod;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 2; i++)
            {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(MathHelper.ToRadians(18f)), type, 0, 0f, player.whoAmI);
            }
            return false;
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(43f, -29f);
            lineColor = new Color(220, 20, 60, 100);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PerennialBar>(6).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
