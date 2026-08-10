using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.DraedonStructures;

public class LaboratoryPanels : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.SetMerge(Type, ModContent.TileType<HazardChevronPanels>());

        HitSound = CommonCalamitySounds.PlatingMine;
        DustType = DustID.Asphalt;
        MinPick = 30;
        AddMapEntry(new Color(36, 35, 37));

        this.RegisterBlendMergeWith(TileID.Dirt);
        this.RegisterBlendMergeWith(TileID.Stone);
    }

    public override bool CanExplode(int i, int j) => false;
}
