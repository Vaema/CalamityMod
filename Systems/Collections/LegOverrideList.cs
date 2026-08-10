using System.Collections.Generic;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Vanity;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections;

public sealed class LegOverrideList : ModSystem
{
    public static IList<int> List { get; private set; }

    public override void OnModLoad()
    {
        List =
        [
            EquipLoader.GetEquipSlot(CalamityMod.Instance, nameof(ProfanedSoulCrystal), EquipType.Legs),
            EquipLoader.GetEquipSlot(CalamityMod.Instance, nameof(AquaticHeart), EquipType.Legs),
            //CalamityMod.Instance.GetEquipSlot(nameof(SirenLeg), EquipType.Legs), whate even was SirenLeg vs SirenLegAlt?
            EquipLoader.GetEquipSlot(CalamityMod.Instance, nameof(Popo), EquipType.Legs)
        ];
    }

    public override void Unload() => List = null;

    public static bool Includes(int equipSlot) => List.Contains(equipSlot);
}
