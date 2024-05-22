using CalamityMod.NPCs.SlimeGod;
using CalamityMod.Projectiles.Magic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class AbyssalTome : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.damage = 22;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SlimeGodCore.ShotSound;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AbyssBall>();
            Item.shootSpeed = 9f;
        }
    }
}
