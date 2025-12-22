using CalamityMod.ExtraJumps;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Sulphurous
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("SulfurHelmet")]
    public class SulphurousHelmet : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static int RogueCritBoost = 6;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RogueCritBoost);

        // Set Bonus
        public static int SetBonusPoisonDuration = CalamityUtils.SecondsToFrames(1);
        public static float SetBonusRogueStealth = 0.65f;
        public static int BubbleDamage = 20;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.defense = 5;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<SulphurousBreastplate>() && legs.type == ModContent.ItemType<SulphurousLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusRogueStealth.ToStealth(), SetBonusPoisonDuration.FramesToSeconds());
            var modPlayer = player.Calamity();
            modPlayer.sulphurSet = true;
            player.GetJumpState<SulphurJump>().Enable();
            modPlayer.rogueStealthMax += SetBonusRogueStealth;
            modPlayer.wearingRogueArmor = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance<ThrowingDamageClass>() += RogueCritBoost;
            if (player.Calamity().countsAsAnyWet)
                player.gills = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(10).
                AddIngredient<SulphuricScale>(10).
                AddTile(TileID.Anvils).
                SortBeforeFirstRecipesOf(ModContent.ItemType<SulphurousBreastplate>()).
                Register();
        }
    }
}
