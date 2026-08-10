using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CalamityMod.Cooldowns;

public class ParryCooldown : CooldownHandler
{
    public static new string ID => "ParryCooldown";
    public override bool ShouldDisplay => true;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override string Texture => "CalamityMod/Cooldowns/" + skinTexture;
    public override Color OutlineColor => outlineColor;
    public override Color CooldownStartColor => cooldownColorStart;
    public override Color CooldownEndColor => cooldownColorEnd;

    //It's the same cooldown with different skins each time, basically.
    public string skinTexture;
    public Color outlineColor;
    public Color cooldownColorStart;
    public Color cooldownColorEnd;

    public ParryCooldown() : this("") { }
    public ParryCooldown(string skin)
    {
        switch (skin)
        {
            case "blazingcore":
                skinTexture = "BlazingCoreParry";
                outlineColor = new Color(255, 191, 73);
                cooldownColorStart = new Color(181, 136, 177);
                cooldownColorEnd = new Color(255, 194, 161);
                break;
            case "flamelickedshell":
                skinTexture = "FlameShellParry";
                outlineColor = new Color(211, 124, 93);
                cooldownColorStart = new Color(107, 6, 6);
                cooldownColorEnd = new Color(228, 78, 78);
                break;
            case "shieldoftheocean":
                skinTexture = "OceanShieldParry";
                outlineColor = Color.White;
                cooldownColorStart = new Color(233, 111, 165);
                cooldownColorEnd = new Color(105, 139, 148);
                break;
            default:
                skinTexture = "ParryCooldown";
                outlineColor = Color.White;
                cooldownColorStart = Color.CornflowerBlue;
                cooldownColorEnd = Color.White;
                break;
        }
    }
}
