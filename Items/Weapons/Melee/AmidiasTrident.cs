using CalamityMod.Items.BaseItems;
using CalamityMod.Projectiles.Melee.Spears;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class AmidiasTrident : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Type] = true;
        }

        /// <summary>
        /// The damage dealt in melee range when using attack 1.
        /// </summary>
        public static readonly int BaseAttackMeleeDamage = 20;
        /// <summary>
        /// The damage dealt by the long-range projectiles shot by attack 1.
        /// </summary>
        public static readonly int BaseAttackProjectileDamage = 10;
        /// <summary>
        /// Secondary attack spin damage. In melee range only.
        /// </summary>
        public static readonly int SecondaryAttackMeleeDamage = 25;
        /// <summary>
        /// Secondary attack proc effect damage. Multiplied by the number of stacks of Sea King's Assurance the enemy taking damage has.
        /// </summary>
        public static readonly int SecondaryAttackProjectileDamage = 20;

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 12;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 45;
            Item.knockBack = 4.5f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<AmidiasTridentProj>();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
    }
}
