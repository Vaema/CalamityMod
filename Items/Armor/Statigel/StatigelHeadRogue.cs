using CalamityMod.ExtraJumps;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Statigel
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("StatigelMask")]
    public class StatigelHeadRogue : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static float RogueDamageBoost = 0.1f;
        public static int RogueCritBoost = 7;
        public static float MoveSpeedBoost = 0.05f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RogueDamageBoost.ToPercent(), RogueCritBoost, MoveSpeedBoost.ToPercent());

        // Set Bonus
        public static float SetBonusRogueStealth = 0.9f;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 6; //23
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<StatigelArmor>() && legs.type == ModContent.ItemType<StatigelGreaves>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusRogueStealth.ToStealth(), StatigelArmor.SetBonusJumpSpeedBoost.ToJumpSpeedPercent());
            var modPlayer = player.Calamity();
            modPlayer.statigelSet = true;
            player.GetJumpState<StatigelJump>().Enable();
            modPlayer.rogueStealthMax += SetBonusRogueStealth;
            modPlayer.wearingRogueArmor = true;
            Player.jumpHeight += (int)(StatigelArmor.SetBonusJumpHeightPercentBoost * 15);
            player.jumpSpeedBoost += StatigelArmor.SetBonusJumpSpeedBoost;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<ThrowingDamageClass>() += RogueDamageBoost;
            player.GetCritChance<ThrowingDamageClass>() += RogueCritBoost;
            player.moveSpeed += MoveSpeedBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PurifiedGel>(5).
                AddIngredient<BlightedGel>(5).
                AddTile(TileID.Solidifier).
                SortBeforeFirstRecipesOf(ModContent.ItemType<StatigelArmor>()).
                Register();
        }
    }
}
