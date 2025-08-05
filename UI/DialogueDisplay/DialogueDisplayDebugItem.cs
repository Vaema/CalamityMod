using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System.Linq;
using CalamityMod.UI.DialogueDisplay.DisplayEffects;

namespace CalamityMod.UI.DialogueDisplay
{
    public class DialogueDisplayDebugItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Debug";
        public override string Texture => "CalamityMod/Items/Weapons/Summon/StaffOfNecrosteocytes";
        public override void SetDefaults()
        {
            Item.width = 25;
            Item.height = 29;
            Item.rare = ItemRarityID.Red;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }
        public override bool? UseItem(Player player)
        {
            if (DialogueDisplayUI.Dialogues.ContainsKey("OceanBlue"))
                DialogueDisplaySystem.ProgressDialogue("OceanBlue");
            else
                DialogueDisplaySystem.StartDialogue("OceanBlue", Main.npc.First(n => n.active), -1, effects: new WhisperingPearlEffects());

            return true;
        }
    }
}
