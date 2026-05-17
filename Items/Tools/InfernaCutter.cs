using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class InfernaCutter : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 66;
            Item.damage = 70;
            Item.crit = 10;
            Item.knockBack = 7f;
            Item.useTime = 8;
            Item.useAnimation = 12;
            Item.axe = 135 / 5;
            Item.tileBoost += 1;

            Item.DamageType = DamageClass.Melee;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            hitbox = CalamityUtils.FixSwingHitbox(54, 54);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AxeofPurity>().
                AddIngredient(ItemID.SoulofFright, 8).
                AddIngredient<EssenceofHavoc>(3).
                AddTile(TileID.MythrilAnvil).
                Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(4))
            {
                int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hit.Crit)
            {
                var source = player.GetSource_ItemUse(Item);
                int boom = Projectile.NewProjectile(source, target.Center, Vector2.Zero, ModContent.ProjectileType<FuckYou>(), (int)(Item.damage * 0.33f), Item.knockBack, player.whoAmI, 0f, 0.85f + Main.rand.NextFloat() * 1.15f);
                if (boom.WithinBounds(Main.maxProjectiles))
                    Main.projectile[boom].DamageType = DamageClass.Melee;
            }
            target.AddBuff(BuffID.OnFire3, 300);
        }
    }
}
