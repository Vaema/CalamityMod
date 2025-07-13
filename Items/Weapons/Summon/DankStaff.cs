using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class DankStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<RotBall>();
        }
        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 58;
            Item.damage = 14;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 2.25f;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<DankCreeperMinion>();
            Item.shootSpeed = 10f;
            Item.DamageType = DamageClass.Summon;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2)
            {
                position = player.ClampedMouseWorld();
                velocity.X = 0;
                velocity.Y = 0;
                int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (Main.projectile.IndexInRange(p))
                    Main.projectile[p].originalDamage = Item.damage;
            }
            return false;
        }
    }
}
