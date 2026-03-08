using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.FishingRods
{
    public class TrustyOldRod : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public static int enemyChance = 4; // 1/x chance to turn items into enemies
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 45;
            Item.shootSpeed = 14f;
            Item.shoot = ModContent.ProjectileType<TrustyOldBobber>();
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(copper: 1);
        }
        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(45f, -43f);
            lineColor = new Color(122, 169, 52, 0);
        }
    }
}
