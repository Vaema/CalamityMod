using CalamityMod.Items.BaseItems;
using CalamityMod.Projectiles.Melee;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    [LegacyName("OldLordOathsword")]
    public class OldLordClaymore : CustomUseProjItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 195;
            Item.DamageType = TrueMeleeDamageClass.Instance;
            Item.useAnimation = Item.useTime = 90; // Yes it's actually supposed to be this slow

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ModContent.ProjectileType<OldLordClaymoreHoldout>();
            Item.useTurn = true;
            Item.knockBack = 10f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;

            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
        }
        public override bool MeleePrefix() => true;
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 11;
    }
}
