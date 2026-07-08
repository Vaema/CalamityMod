using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Rarities
{
    // Dark Orange is used for Draedon's Arsenal items.
    // It is a unique rarity and does not have its items rarity change on reforge.

    public class DarkOrange : ModRarity
    {
        public override Color RarityColor => new Color(204, 71, 35);
    }
}
