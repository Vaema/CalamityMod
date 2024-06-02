using System.Collections.Generic;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class Atlantis : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.damage = 86;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item34;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AtlantisSpear>();
            Item.shootSpeed = 32f;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (Main.zenithWorld)
            {
                bool devourer = DownedBossSystem.downedDoG;
                float damageMult = 1f + (devourer ? (348f / 86f - 1f) : 0f);
                damage *= damageMult;
            }
        }
        public override float UseSpeedMultiplier(Player player) => (DownedBossSystem.downedDoG && Main.zenithWorld) ? 2.5f : 1f;

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            bool devourer = DownedBossSystem.downedDoG;
            string line = this.GetLocalizedValue("TooltipNormal");
            if (Main.zenithWorld && devourer)
                line = this.GetLocalizedValue("TooltipGFBDoG");
            else if (Main.zenithWorld)
                line = this.GetLocalizedValue("TooltipGFB");
            list.FindAndReplace("[GFB]", line);
        }
    }
}
