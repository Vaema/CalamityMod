using CalamityMod.Dusts;
using CalamityMod.Items.Placeables.FurnitureMonolith;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee;

public class MonolithSword : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Melee";

    public static int ArmorPenetration = 15;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ArmorPenetration);

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.PearlwoodSword); // Monolith >= Pearlwood
        Item.width = 40;
        Item.height = 46;
        Item.damage = 30;
        Item.useAnimation = Item.useTime = 7;
        Item.ArmorPenetration = ArmorPenetration;
    }
    public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        float scale = 2.5f;
        Vector2 newSize = new Point(hitbox.Width, hitbox.Height).ToVector2() * scale;
        hitbox = new Rectangle(hitbox.X - (int)((newSize.X - hitbox.Width) / 2f), hitbox.Y - (int)((newSize.Y - hitbox.Height) / 2f), (int)newSize.X, (int)newSize.Y);
    }
    public override void UseAnimation(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            float Rot = (player.direction == -1 ? 5.5f : -5.5f) * Main.rand.NextFloat(0.99f, 1.1f);
            Particle Smear = new SemiCircularSmearFade(player.Center, Vector2.Zero, (Main.rand.NextBool() ? Color.DarkTurquoise : Color.Coral) * 0.7f, Rot, Main.rand.NextFloat(1.48f, 1.53f), new Vector2(1, 1), 6, true, false, true);
            GeneralParticleHandler.SpawnParticle(Smear);
        }
    }
    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        if (Main.rand.NextBool(3))
            Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, Main.rand.NextBool() ? ModContent.DustType<AstralOrange>() : ModContent.DustType<AstralBlue>());
    }
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AstralMonolith>(7).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
