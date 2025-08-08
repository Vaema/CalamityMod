using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Empyrean
{
    [AutoloadEquip(EquipType.Body)]
    [LegacyName("XerocPlateMail")]
    public class EmpyreanCloak : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";
        public override void Load()
        {
            if (!Main.dedServ)
            {
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Armor/Empyrean/EmpyreanCloak_Neck", EquipType.Neck, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Armor/Empyrean/EmpyreanCloak_Back", EquipType.Back, this);
            }
        }

        public override void SetStaticDefaults()
        {

            if (!Main.dedServ)
            {
                var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
                ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
                ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
            }
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.defense = 27;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance<ThrowingDamageClass>() += 7;
            player.GetDamage<ThrowingDamageClass>() += 0.07f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MeldConstruct>(20).
                AddIngredient(ItemID.LunarBar, 16).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
