using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee.Spears;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    [LegacyName("TerraLance")]
    public class BotanicPiercer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 112;
            Item.height = 112;
            Item.damage = 88;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 17;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 17;
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.shoot = ModContent.ProjectileType<BotanicPiercerProjectile>();
            Item.shootSpeed = 11f;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/BotanicPiercerGlow").Value);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.ChlorophytePartisan).
                AddIngredient(ItemID.DarkLance).
                AddIngredient(ItemID.Gungnir).
                AddIngredient<LivingShard>(12).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
