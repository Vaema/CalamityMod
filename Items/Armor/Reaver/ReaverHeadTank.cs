using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Reaver
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("ReaverHelm")]
    public class ReaverHeadTank : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";
        internal static string HealOrbEntitySourceContext => "SetBonus_Calamity_ReaverTank";

        public static int ReaverRageDefenseBoost = 5;
        public static float ReaverRageDamageBoost = 0.1f;

        //Defense and DR Helm
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.defense = 30; //63 => 73 w/ set bonus (+5 w/ Reaver Rage)
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ReaverScaleMail>() && legs.type == ModContent.ItemType<ReaverCuisses>();
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadowSubtle = true;
            player.armorEffectDrawOutlines = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.Calamity();
            player.thorns += 0.33f;
            player.moveSpeed -= 0.3f;
            player.statDefense += 10;
            player.lifeRegen += 10;
            player.aggro += 600;
            modPlayer.reaverDefense = true;
            modPlayer.wearingRogueArmor = true;
            player.setBonus = this.GetLocalizedValue("SetBonus");
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() -= 0.3f;
            player.endurance += 0.15f;
            player.statLifeMax2 += 50;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PerennialBar>(7).
                AddIngredient<LivingShard>().
                AddTile(TileID.MythrilAnvil).
                SortBeforeFirstRecipesOf(ModContent.ItemType<ReaverHeadMobility>()).
                Register();
        }
    }
}
