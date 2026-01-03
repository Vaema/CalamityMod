using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("MepheticSprayer")]
    public class BlightSpewer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public static readonly SoundStyle Nanomachines = new("CalamityMod/Sounds/Item/NanoSwarm") { PitchVariance = 0.45f, Volume = 0.4f };

        public static int AmmoSavedPercent = 33;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AmmoSavedPercent);

        public override void SetDefaults()
        {
            Item.width = 76;
            Item.height = 36;
            Item.damage = 52;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 19;
            Item.useAnimation = 19;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.UseSound = SoundID.Item34;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlightFlames>();
            Item.shootSpeed = 7f;
            Item.useAmmo = AmmoID.Gel;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

        public override bool CanConsumeAmmo(Item ammo, Player player) => Main.rand.Next(100) >= AmmoSavedPercent;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SparkSpreader>().
                AddIngredient<InfectedArmorPlating>(5).
                AddIngredient<PlagueCellCanister>(10).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
