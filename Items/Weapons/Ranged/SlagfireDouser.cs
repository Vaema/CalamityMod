using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class SlagfireDouser : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 104;
            Item.height = 34;
            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 30;
            Item.useAnimation = 30; 
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.UseSound = new SoundStyle("CalamityMod/Sounds/Item/DudFire") with { Volume = .4f, Pitch = -.95f, PitchVariance = 0.1f };
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.autoReuse = false; // Because holdout
            Item.shoot = ModContent.ProjectileType<SlagfireDouserHoldout>();
            Item.ArmorPenetration = 10;
            Item.shootSpeed = 13f;
            Item.reuseDelay = 0; // Because holdout
            Item.channel = true; // Because holdout
            Item.noUseGraphic = true;
        }

        public override void HoldItem(Player player) => player.Calamity().mouseRotationListener = true;
        public override Vector2? HoldoutOffset() => new Vector2(102, 0);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile holdout = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, ProjectileType<SlagfireDouserHoldout>(), damage, knockback, player.whoAmI);

            // Seting its velocity like this is what aims to the mouse
            holdout.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);

            return false; 
        }
    }
}
