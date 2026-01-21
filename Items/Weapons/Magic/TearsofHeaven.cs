using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class TearsofHeaven : ModItem, ILocalizedModType
    {
        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/TearsOfHeavenUse");

        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 48;
            Item.damage = 43;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;
            Item.useTime = Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5.5f;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = UseSound;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TearsofHeavenProjectile>();
            Item.shootSpeed = 5.5f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int index = 0; index < 2; ++index)
            {
                float SpeedX = velocity.X + Main.rand.NextFloat(-2f, 2f);
                float SpeedY = velocity.Y + Main.rand.NextFloat(-2f, 2f);
                Projectile.NewProjectile(source, position, new Vector2(SpeedX, SpeedY), type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<FrigidflashBolt>().
                AddIngredient(ItemID.WaterBolt).
                AddIngredient<SeaPrism>(15).
                AddIngredient(ItemID.Ectoplasm, 5).
                AddTile(TileID.Bookcases).
                Register();
        }
    }
}
