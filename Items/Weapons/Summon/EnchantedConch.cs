using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    [LegacyName("MagicalConch")]
    public class EnchantedConch : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 26;
            Item.damage = 20;
            Item.DamageType = DamageClass.Summon;
            Item.shoot = ModContent.ProjectileType<HermitCrabMinion>();
            Item.knockBack = 2f;

            Item.useTime = Item.useAnimation = 35;
            Item.mana = 10;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item44;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, Main.MouseWorld, Main.rand.NextVector2CircularEdge(5f, 5f), type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}
