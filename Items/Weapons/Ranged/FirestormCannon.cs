using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class FirestormCannon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public static int AmmoSavedPercent = 33;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AmmoSavedPercent);

        // This is intentionally stored on the item instead of the holdout to prevent swapping items to instantly reset the heat
        // This could be theoretically exploited using multiple items, but the heat dissipates fast enough that I can't really care
        public int BuiltUpHeat = 0;
        public const int OverheatLevel = 480;
        public const int OverheatCooldown = 180;
        public const int OverheatDamage = 20;

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 28;
            Item.damage = 14;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = Item.useAnimation = 11;
            Item.knockBack = 1.5f;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;

            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ModContent.ProjectileType<FirestormCannonHoldout>();
            Item.shootSpeed = 5.5f;
            Item.useAmmo = AmmoID.Flare;
        }

        public override void HoldItem(Player player)
        {
            player.Calamity().rightClickListener = true;
            // Heat decrements if:
            // The holdout exists, the player is not firing, the player is not in overheat, and there is heat in the weapon
            if (player.ownedProjectileCounts[Item.shoot] > 0 && !Main.mouseLeft && player.Calamity().flareGunOverheat == 0 && BuiltUpHeat > 0)
            {
                BuiltUpHeat -= player.miscCounter % 2 == 0 ? 3 : 2;
                if (BuiltUpHeat < 0)
                    BuiltUpHeat = 0;
            }
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0 && !player.Calamity().mouseRight;
        public override bool CanConsumeAmmo(Item ammo, Player player) => player.ownedProjectileCounts[Item.shoot] > 0 && Main.rand.Next(100) >= AmmoSavedPercent;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => type = Item.shoot;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.FlareGun).
                AddIngredient(ItemID.HellstoneBar, 10).
                AddIngredient(ItemID.IllegalGunParts).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
