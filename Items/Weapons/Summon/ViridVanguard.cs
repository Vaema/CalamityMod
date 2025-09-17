using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Weapons.Summon
{
    public class ViridVanguard : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public const int HorizontalSlashChargeTime = 14;

        public const float HorizontalSlashSpeed = 44f;

        public const int VerticalSlashChargeTime = 32;

        public const float VerticalSlashSpeed = 45f;

        public const float VerticalTeleportOffset = 850f;

        public const int PierceChargeAttackCycleTime = 44;

        public const float MaxTargetingDistance = 1550f;

        public const int ChargesPerAttackCycle = 7;

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 36;
            Item.damage = 73;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 14;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.buffType = ModContent.BuffType<ViridVanguardBuff>();
            Item.shoot = ModContent.ProjectileType<ViridVanguardBlade>();
            Item.rare = ModContent.RarityType<Turquoise>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            var minion = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 1f);
            minion.originalDamage = Item.damage;
            minion.ModProjectile<ViridVanguardBlade>().BladeIndex = player.ownedProjectileCounts[type];

            int bladeIndex = 0;
            foreach (Projectile pro in Main.ActiveProjectiles)
            {
                if (pro.type == type && pro.owner == player.whoAmI)
                {
                    pro.ModProjectile<ViridVanguardBlade>().BladeIndex = bladeIndex++;
                    pro.ModProjectile<ViridVanguardBlade>().AITimer = 0f;
                    pro.netUpdate = true;
                }
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<UelibloomBar>(15).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
