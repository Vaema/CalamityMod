using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class ElephantKiller : RogueWeapon
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 58;
            Item.damage = 67;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = Item.useTime = 38;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.shoot = ModContent.ProjectileType<ElephantKillerThrown>();
            Item.shootSpeed = 15f;
            Item.DamageType = RogueDamageClass.Instance;
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override bool AltFunctionUse(Player player) => true;
        public override void HoldItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                player.Calamity().rightClickListener = true;
            player.Calamity().mouseWorldListener = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool shoot = player.Calamity().mouseRight;
            Projectile gun = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, 0, shoot ? 1f : 0);
            gun.scale = 0;
            return false;
        }
    }
}
