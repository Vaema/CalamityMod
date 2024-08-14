using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.SunkenSeaCatches
{
    public class SparklingEmpress : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public static int BaseDamage = 10;

        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true; //so it doesn't look weird af when holding it
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = BaseDamage;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 5;
            Item.useAnimation = Item.useTime = 20;
            Item.knockBack = 0.25f;
            Item.shoot = ModContent.ProjectileType<SparklingLaser>();
            Item.shootSpeed = 14f;

            Item.UseSound = SoundID.Item13;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true; //Channel so that you can hold the weapon [Important]
            Item.noMelee = true;

            Item.rare = ItemRarityID.Green;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
        }
    }
}
