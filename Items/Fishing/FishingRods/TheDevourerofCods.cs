using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Fishing.AstralCatches;
using CalamityMod.Items.Fishing.BrimstoneCragCatches;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.FishingRods
{
    public class TheDevourerofCods : ModItem, ILocalizedModType
    {
        public static List<int> FishToEat = new List<int>()
                    {
                        ItemID.Bass,
                        ItemID.AtlanticCod,
                        ItemID.Flounder,
                        ItemID.NeonTetra,
                        ItemID.RedSnapper,
                        ItemID.RockLobster,
                        ItemID.Salmon,
                        ItemID.Shrimp,
                        ItemID.Trout,
                        ItemID.Tuna,
                        ModContent.ItemType<CharredLasher>(),
                        ModContent.ItemType<CragBullhead>(),
                        ModContent.ItemType<ProcyonidPrawn>(),
                        ModContent.ItemType<TwinklingPollox>(),
                        ModContent.ItemType<PlantyMush>()
    };
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanFishInLava[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 75;
            Item.shootSpeed = 20f;
            Item.shoot = ModContent.ProjectileType<DevourerofCodsBobber>();
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }

        public override void HoldItem(Player player)
        {
            if (player.Calamity().SelectedFishingMinigame == CalamityPlayer.FishingMinigames.None)
                player.Calamity().SelectedFishingMinigame = CalamityPlayer.FishingMinigames.DevourerofCods;
            player.accFishingLine = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.Calamity().SelectedFishingMinigame = CalamityPlayer.FishingMinigames.DevourerofCods;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 10; i++)
            {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(MathHelper.ToRadians(18f)), type, 0, 0f, player.whoAmI, ai2: Main.rand.Next(2));
            }
            return false;
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(53f, -33f);
            if (bobber.ai[2] == 0f)
                lineColor = new Color(252, 109, 202, 100);
            else
                lineColor = new Color(39, 151, 171, 100);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CosmiliteBar>(6).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
