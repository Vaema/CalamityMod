using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    [LegacyName("AngryChickenStaff")]
    public class YharonsKindleStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public const float ReboundRamDamageFactor = 2f;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 5f;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<TheFinalDawn>();
        }

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 74;
            Item.damage = 325;
            Item.mana = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = Item.useTime = 10;
            Item.noMelee = true;
            Item.knockBack = 7f;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.UseSound = CommonCalamitySounds.FlareSound;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FieryDraconid>();
            Item.shootSpeed = 10f;
            Item.DamageType = DamageClass.Summon;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2)
            {
                position = player.ClampedMouseWorld();
                int dragon = Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
                if (Main.projectile.IndexInRange(dragon))
                    Main.projectile[dragon].originalDamage = Item.damage;
            }
            return false;
        }
    }
}
