using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee.Spears;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class LemonNade : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 75;
            Item.DamageType = RogueDamageClass.Instance;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item1;
            Item.channel = true;
            Item.shootSpeed = 13f;
            Item.shoot = ModContent.ProjectileType<LemonNadeHoldout>();
        }
        public override float StealthVelocityMultiplier => 1f;

        public override float StealthDamageMultiplier => 1f;
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 4;

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[Item.shoot] <= 0)
                player.Calamity().rogueStealth = 0;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Supernova>().
                AddIngredient(ItemID.Lemon).
                Register();
        }
    }
}
