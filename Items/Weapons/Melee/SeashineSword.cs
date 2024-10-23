using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class SeashineSword : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.EnchantedSword);
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.damage = 25;
            Item.DamageType = DamageClass.Melee;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.knockBack = 4f;
            Item.shootSpeed = 12f;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<SeashineSwordProj>();
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 120);
        }
        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 120);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PearlShard>(3).
                AddIngredient<SeaPrism>(7).
                AddIngredient<Navystone>(10).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
