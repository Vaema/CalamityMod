using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class SpectralstormCannon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        // This is intentionally stored on the item instead of the holdout to prevent swapping items to instantly reset the heat
        // This could be theoretically exploited using multiple items, but the heat dissipates fast enough that I can't really care
        public int BuiltUpHeat = 0;
        public const int OverheatLevel = 540;
        public const int OverheatCooldown = 160;
        public const int OverheatDamage = 50;

        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 26;
            Item.damage = 82;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = Item.useAnimation = 11;
            Item.knockBack = 1.5f;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;

            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.shoot = ModContent.ProjectileType<SpectralstormCannonHoldout>();
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Flare;
        }

        public override void HoldItem(Player player)
        {
            player.Calamity().rightClickListener = true;
            // Heat decrements if:
            // The holdout exists, the player is not firing, the player is not in overheat, and there is heat in the weapon
            if (player.ownedProjectileCounts[Item.shoot] > 0 && !Main.mouseLeft && player.Calamity().flareGunOverheat == 0 && BuiltUpHeat > 0)
            {
                BuiltUpHeat -= 3;
                if (BuiltUpHeat < 0)
                    BuiltUpHeat = 0;
            }
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0 && !player.Calamity().mouseRight;
        public override bool CanConsumeAmmo(Item ammo, Player player) => player.ownedProjectileCounts[Item.shoot] > 0; // Spawning the holdout cannot consume ammo
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SpectralstormCannonHoldout>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<FirestormCannon>().
                AddIngredient(ItemID.FragmentSolar, 6).
                AddIngredient(ItemID.Ectoplasm, 10).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
