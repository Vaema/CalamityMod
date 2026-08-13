using CalamityMod.Projectiles.Melee.Yoyos;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee;

public class TheMicrowave : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Melee";
    public static readonly SoundStyle BeepSound = new("CalamityMod/Sounds/Custom/MicrowaveBeep");
    public static readonly SoundStyle MMMSound = new("CalamityMod/Sounds/Custom/MMMMMMMMMMMMM") { IsLooped = true };

    public static float Reach = 512f;
    public static float Speed = 54f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(Reach.ToTiles(), Speed);

    public override void SetStaticDefaults()
    {
        ItemID.Sets.Yoyo[Type] = true;
        ItemID.Sets.GamepadExtraRange[Type] = 15;
        ItemID.Sets.GamepadSmartQuickReach[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 34;
        Item.height = 34;
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.damage = 111;
        Item.knockBack = 3f;
        Item.useTime = 22;
        Item.useAnimation = 22;
        Item.autoReuse = true;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.Item1;
        Item.channel = true;
        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<MicrowaveYoyo>();
        Item.shootSpeed = 14f;

        Item.rare = ItemRarityID.Cyan;
        Item.value = CalamityGlobalItem.RarityCyanBuyPrice;
    }
}
