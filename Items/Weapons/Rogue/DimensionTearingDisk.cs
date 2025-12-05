using System.Collections.Generic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    [LegacyName("Eradicator")]
    public class DimensionTearingDisk : RogueWeapon
    {
        public static float Speed = 10.5f;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<MawOfInfinity>();
        }
        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 58;
            Item.DamageType = RogueDamageClass.Instance;
            Item.useTime = Item.useAnimation = 24;
            Item.knockBack = 7f;
            Item.damage = 725;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.shoot = ModContent.ProjectileType<DimensionTearingDiskProjectile>();
            Item.shootSpeed = Speed;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { 
            tooltips.FindAndReplaceAll("ff00ff", Utils.Hex3(DevourerofGodsHead.SpecialMoveColor));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (player.Calamity().StealthStrikeAvailable() && proj.WithinBounds(Main.maxProjectiles))
            {
                Main.projectile[proj].Calamity().stealthStrike = true;
            }
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Rogue/DimensionTearingDiskGlow").Value);
        }
    }
}
