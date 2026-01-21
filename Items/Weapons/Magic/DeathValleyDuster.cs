using CalamityMod.Projectiles.Magic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    [LegacyName("DeathValley")]
    public class DeathValleyDuster : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 40;
            Item.damage = 130;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 38;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 8f;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = new SoundStyle("CalamityMod/Sounds/Item/MagicRockSound") with { Volume = 0.4f, Pitch = -0.1f };
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<DeathValleyDusterProjectile>();
            Item.shootSpeed = 6.5f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.velocity.Length() <= 12)
                player.velocity += -velocity.SafeNormalize(Vector2.UnitX) * 5f;
            bool MaxMana = player.statMana >= (player.statManaMax2 - ((int)(Item.mana * player.manaCost))) && !player.HasBuff(BuffID.ManaSickness);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 0f, MaxMana ? 1f : 0f);
            
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.SpellTome).
                AddIngredient(ItemID.FossilOre, 25).
                AddIngredient(ItemID.AncientCloth, 2).
                AddTile(TileID.Bookcases).
                Register();
        }
    }
}
