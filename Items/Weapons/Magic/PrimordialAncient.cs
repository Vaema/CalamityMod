using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CalamityMod.Items.Weapons.Magic
{
    public class PrimordialAncient : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 56;
            Item.damage = 2800;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 85;
            Item.useTime = 78;
            Item.useAnimation = 78;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 14;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.UseSound = new SoundStyle("CalamityMod/Sounds/Item/MagicRockSound") with { Volume = 0.4f, Pitch = -0.1f };
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<PrimordialAncientProjectile>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<DarkBlue>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool MaxMana = player.statMana >= (player.statManaMax2 - ((int)(Item.mana * player.manaCost))) && !player.HasBuff(BuffID.ManaSickness);
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = velocity.RotatedByRandom(0.2f * i);
                Projectile.NewProjectile(source, position, vel * Main.rand.NextFloat(0.8f, 1.2f), type, damage, knockback, player.whoAmI, 0f, i == 0 ? 1 : 0, MaxMana ? 1f : 0f);
            }

            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PrimordialEarth>().
                AddIngredient<CosmiliteBar>(8).
                AddTile(TileID.Bookcases).
                Register();
        }
    }
}
