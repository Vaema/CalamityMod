using System.Collections.Generic;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    [LegacyName("Excelsus")]
    public class MawOfInfinity: BaseSwordHoldoutItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override int ProjectileType => ModContent.ProjectileType<MawOfInfinityHoldout>();
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<TheObliterator>();
            base.SetStaticDefaults();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.FindAndReplaceAll("ff00ff", Utils.Hex3(DevourerofGodsHead.SpecialMoveColor));
        }
        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 94;
            Item.damage = 2650;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = Item.useAnimation = 33;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.knockBack = 8f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.shoot = ModContent.ProjectileType<MawOfInfinityHoldout>();
            Item.shootSpeed = 12f;
            base.SetDefaults();
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/MawOfInfinityGlow").Value);
        }
    }
}
