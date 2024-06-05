using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.FishingRods
{
    public class SlurperPole : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanFishInLava[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 25;
            Item.shootSpeed = 14f;
            Item.shoot = ModContent.ProjectileType<SlurperBobber>();
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(45f, -43f);
            lineColor = new Color(227, 79, 79, 100);
        }
    }
}
