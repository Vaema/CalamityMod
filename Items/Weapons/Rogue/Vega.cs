using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
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
    [LegacyName("NightsGaze")]
    public class Vega : RogueWeapon, IHoldShiftTooltipItem
    {
        public static int StarburstCost = 20;
        public override void SetDefaults()
        {
            Item.width = 82;
            Item.height = 82;
            Item.damage = 531;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.knockBack = 1f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.maxStack = 1;
            Item.shoot = ModContent.ProjectileType<VegaProjectile>();
            Item.shootSpeed = 30f;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.DamageType = RogueDamageClass.Instance;
        }

        public override float StealthDamageMultiplier => 1.2f;
        public override void HoldItem(Player player)
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
            player.Calamity().HasStratusItemCooldown = (int)MathHelper.Max(player.Calamity().HasStratusItemCooldown, 600);
        }

        public override bool AltFunctionUse(Player player)
        {
            if (player.Calamity().AvaliableStarburst >= StarburstCost)
            {
                return true;
            }
            return false;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {

                player.Calamity().rogueStealth = player.Calamity().rogueStealthMax;
                player.Calamity().StratusStarburst -= StarburstCost;
                int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI,0,1);
                if (p.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[p].Calamity().stealthStrike = true;
                    Main.projectile[p].extraUpdates += 1;
                }
                return false;
            }
            if (player.Calamity().StealthStrikeAvailable())
            {
                int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (p.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[p].Calamity().stealthStrike = true;
                    Main.projectile[p].extraUpdates += 1;
                }
                return false;
            }
            return true;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Rogue/VegaGlow").Value);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ProfanedPartisan>().
                AddIngredient<Lumenyl>(7).
                AddIngredient<RuinousSoul>(4).
                AddIngredient<ExodiumCluster>(12).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
