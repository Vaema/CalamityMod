using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Melee.Spears;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class GildedProboscis : BaseSwordHoldoutItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public override int ProjectileType => ModContent.ProjectileType<GildedProboscisProj>();

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<GoldenEagle>();
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 66;
            Item.damage = 4000;
            Item.DamageType = TrueMeleeDamageClass.Instance;
            Item.useAnimation = Item.useTime = 65;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 15f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.shootSpeed = 13f;
            Item.channel = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
            base.SetDefaults();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
    }
}
