using CalamityMod.Items.BaseItems;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class AbyssalDivingSuit : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static double PlatesAllDamageReduction = 0.15D;
        public static double PlatesHitDecay = 0.03D;

        public override string AssetPath => "CalamityMod/Items/Accessories/";
        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "AbyssalDivingSuit", null),
            (EquipType.Body, "AbyssalDivingSuit", null),
            (EquipType.Legs, "AbyssalDivingSuit", null),
            (EquipType.Face, null, null), //results in setting this equip slot to -1
        ];

        public override (SoundStyle sound, int delay)? HurtSound(Player p) => (SoundID.NPCHit4, 10);

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) => player.Calamity().abyssalDivingSuit = true;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AbyssalDivingGear>().
                AddIngredient<AnechoicPlating>().
                AddIngredient(ItemID.LunarBar, 5).
                AddIngredient<MolluskHusk>(15).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
