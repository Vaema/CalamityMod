using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class SunSpiritStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 48;
            Item.damage = 12;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 1.15f;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44;
            Item.buffType = ModContent.BuffType<SolarSpirit>();
            Item.shoot = ModContent.ProjectileType<SolarPixie>();
            Item.DamageType = DamageClass.Summon;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            CalamityUtils.KillShootProjectiles(true, type, player);
            var minion = Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            minion.originalDamage = Item.damage;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.SandstoneBrick, 20).
                AddIngredient<StormlionMandible>(2).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
