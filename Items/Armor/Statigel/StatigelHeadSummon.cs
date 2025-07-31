using CalamityMod.Buffs.Summon;
using CalamityMod.ExtraJumps;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Statigel
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("StatigelHood")]
    public class StatigelHeadSummon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static float SummonKBBoost = 1.5f;

        // Set Bonus
        public static int SetBonusMinionSlotBoost = 1;
        public static float SetBonusSummonDamageBoost = 0.18f;
        public static int SlimeDamage = 18;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 4; //20
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<StatigelArmor>() && legs.type == ModContent.ItemType<StatigelGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusMinionSlotBoost, SetBonusSummonDamageBoost.ToPercent())
            + "\n" + CalamityUtils.GetTextFromModItem<StatigelArmor>("CommonSetBonus").Format(StatigelArmor.SetBonusHurtDamageThreshold, StatigelArmor.SetBonusJumpSpeedBoost.ToJumpSpeedPercent());
            var modPlayer = player.Calamity();
            modPlayer.statigelSet = true;
            modPlayer.slimeGod = true;
            player.GetJumpState<StatigelJump>().Enable();
            Player.jumpHeight += (int)(StatigelArmor.SetBonusJumpHeightPercentBoost * 15);
            player.jumpSpeedBoost += StatigelArmor.SetBonusJumpSpeedBoost;
            player.GetDamage<SummonDamageClass>() += SetBonusSummonDamageBoost;
            player.maxMinions += SetBonusMinionSlotBoost;
            if (player.whoAmI == Main.myPlayer)
            {
                var source = player.GetSource_Accessory(Item);
                if (player.FindBuffIndex(ModContent.BuffType<BabySlimeGodBuff>()) == -1)
                {
                    player.AddBuff(ModContent.BuffType<BabySlimeGodBuff>(), 3600, true);
                }

                int minionID = -1;
                int minionDamage = (int)player.GetTotalDamage<SummonDamageClass>().ApplyTo(SlimeDamage);

                if (WorldGen.crimson && player.ownedProjectileCounts[ModContent.ProjectileType<CrimsonSlimeGodMinion>()] < 1)
                    minionID = Projectile.NewProjectile(source, player.Center, -Vector2.UnitY, ModContent.ProjectileType<CrimsonSlimeGodMinion>(), minionDamage, 0f, Main.myPlayer);
                else if (!WorldGen.crimson && player.ownedProjectileCounts[ModContent.ProjectileType<CorruptionSlimeGodMinion>()] < 1)
                    minionID = Projectile.NewProjectile(source, player.Center, -Vector2.UnitY, ModContent.ProjectileType<CorruptionSlimeGodMinion>(), minionDamage, 0f, Main.myPlayer);

                if (Main.projectile.IndexInRange(minionID))
                    Main.projectile[minionID].originalDamage = SlimeDamage;
            }
        }

        public override void UpdateEquip(Player player) => player.GetKnockback<SummonDamageClass>() += SummonKBBoost;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PurifiedGel>(5).
                AddIngredient<BlightedGel>(5).
                AddTile(TileID.Solidifier).
                SortBeforeFirstRecipesOf(ModContent.ItemType<StatigelHeadRogue>()).
                Register();
        }
    }
}
