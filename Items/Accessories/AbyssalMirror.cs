using CalamityMod.CalPlayer;
using CalamityMod.Items.Armor.Silva;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class AbyssalMirror : ModItem, ILocalizedModType, IHoldShiftTooltipItem
{
    public new string LocalizationCategory => "Items.Accessories";
    public bool HasFlavorTooltip => true;

    public static int AggroReduction = 450;
    public static float StandingStealthRegenBoost = 0.25f;
    public static float MovingStealthRegenBoost = 0.12f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(StandingStealthRegenBoost.ToPercent(), MovingStealthRegenBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 38;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        CalamityPlayer modPlayer = player.Calamity();
        modPlayer.stealthGenStandstill += StandingStealthRegenBoost;
        modPlayer.stealthGenMoving += MovingStealthRegenBoost;
        modPlayer.abyssalMirror = true;
        player.aggro -= AggroReduction;
        modPlayer.DodgeEffects.Add(AbyssMirrorDodge);
    }

    public string AbyssMirrorDodge(Player Player, Player.HurtInfo info)
    {
        // 17APR2024: Ozzatron: Abyssal Mirror is a dodge. It uses vanilla dodge iframes and benefits from Cross Necklace.
        int abyssalMirrorDodgeIFrames = Player.ComputeDodgeIFrames();
        Player.GiveUniversalIFrames(abyssalMirrorDodgeIFrames, true);

        Player.Calamity().rogueStealth += 0.5f;
        SoundEngine.PlaySound(SilvaArmor.ActivationSound, Player.Center);

        var source = Player.GetSource_Accessory(Player.Calamity().FindAccessory(ModContent.ItemType<AbyssalMirror>()));
        for (int i = 0; i < 10; i++)
        {
            int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(55);

            int lumenyl = Projectile.NewProjectile(source, Player.Center.X, Player.Center.Y, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), ModContent.ProjectileType<AbyssalMirrorProjectile>(), damage, 0, Player.whoAmI);
            Main.projectile[lumenyl].rotation = Main.rand.NextFloat(0, 360);
            Main.projectile[lumenyl].frame = Main.rand.Next(0, 4);
            if (lumenyl.WithinBounds(Main.maxProjectiles))
                Main.projectile[lumenyl].DamageType = DamageClass.Generic;
        }

        // TODO -- Calamity dodges should probably not send a vanilla dodge packet considering that causes Tabi dust
        if (Player.whoAmI == Main.myPlayer)
        {
            NetMessage.SendData(MessageID.Dodge, -1, -1, null, Player.whoAmI, 1f, 0f, 0f, 0, 0, 0);
        }
        return "abyssmirror";
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<MirageMirror>().
            AddIngredient<InkBomb>().
            AddIngredient<DepthCells>(20).
            AddIngredient<Lumenyl>(15).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
