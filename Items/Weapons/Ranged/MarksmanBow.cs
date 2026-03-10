using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class MarksmanBow : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 110;
            Item.damage = 39;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 10;
            Item.useTime = 5;
            Item.useAnimation = 15;
            Item.useLimitPerAnimation = 3;
            Item.reuseDelay = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6f;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.JestersArrow;
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Arrow;

            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.Calamity().donorItem = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-4, 0);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Convert wooden arrows to Jester's Arrows
            if (CalamityUtils.CheckWoodenAmmo(type, player))
                type = ProjectileID.JestersArrow;

            float SpeedX = velocity.X + Main.rand.NextFloat(-0.5f, 0.5f);
            float SpeedY = velocity.Y + Main.rand.NextFloat(-0.5f, 0.5f);
            int arrow = Projectile.NewProjectile(source, position, new Vector2(SpeedX, SpeedY), type, damage, knockback, player.whoAmI);
            Main.projectile[arrow].noDropItem = true;
            if (type == ProjectileID.JestersArrow)
            {
                Main.projectile[arrow].localNPCHitCooldown = 12 * Main.projectile[arrow].MaxUpdates;
                Main.projectile[arrow].usesLocalNPCImmunity = true;
                Main.projectile[arrow].usesIDStaticNPCImmunity = false;
                Main.projectile[arrow].tileCollide = false;
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ectoplasm, 31).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
