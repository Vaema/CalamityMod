using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class Effervescence : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Item/EffervescenceFire") { PitchVariance = 0.1f };
        public static readonly SoundStyle BurstSound = new("CalamityMod/Sounds/Item/EffervescenceBurst") { PitchVariance = 0.1f };
        public static readonly SoundStyle PopSound = new("CalamityMod/Sounds/Item/EffervescencePop") { PitchVariance = 0.1f };

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 26;
            Item.damage = 60;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 20;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3.75f;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = FireSound;
            Item.autoReuse = true;
            Item.shootSpeed = 13f;
            Item.shoot = ModContent.ProjectileType<UberBubble>();
        }

        public override Vector2? HoldoutOffset() => new Vector2(-5, 0);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int randomBullets = 0; randomBullets < 4; randomBullets++)
            {
                Vector2 newVel = velocity.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(0.85f, 1.2f);
                Projectile.NewProjectile(source, position, newVel, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BubbleGun).
                AddIngredient(ItemID.LunarBar, 5).
                AddIngredient(ItemID.ShimmerBlock, 5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
