using CalamityMod.Buffs.Summon;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.ExtraTextures;

[Autoload(Side = ModSide.Client)]
public class ExtraTextureRefs : ModSystem
{
    // WoF, These are not "exactly" the ExtraTextures
    public static Asset<Texture2D> WallOfFleshDemonSickleTexture;

    // Flying Carpet Replacements
    public static Asset<Texture2D> FlyingCarpetVanilla;
    public static Asset<Texture2D> FlyingCarpetAuric;

    // Lucky Buff icon replacements
    public static Asset<Texture2D> LuckIconGreater;
    public static Asset<Texture2D> LuckIconVanilla;
    public static Asset<Texture2D> LuckIconLesser;

    // Blighted Slime Buff icons
    public static Asset<Texture2D> BlightedSlimeCorroIcon;
    public static Asset<Texture2D> BlightedSlimeCrimIcon;

    // Particles
    public static Asset<Texture2D> CircularSmear;
    public static Asset<Texture2D> CircularSmearFire1;
    public static Asset<Texture2D> CircularSmearFire2;
    public static Asset<Texture2D> CircularSmearFire3;


    // TODO: - Every other ExtraTextures Reference could be move in here
    public override void OnModLoad()
    {
        WallOfFleshDemonSickleTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/ForbiddenOathbladeProjectile", AssetRequestMode.AsyncLoad);

        FlyingCarpetVanilla = TextureAssets.FlyingCarpet;
        FlyingCarpetAuric = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/AuricCarpet", AssetRequestMode.AsyncLoad);

        LuckIconGreater = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBuffs/LuckyGreater", AssetRequestMode.AsyncLoad);
        LuckIconVanilla = TextureAssets.Buff[BuffID.Lucky];
        LuckIconLesser = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBuffs/LuckyLesser", AssetRequestMode.AsyncLoad);

        BlightedSlimeCorroIcon = ModContent.Request<Texture2D>("CalamityMod/Buffs/Summon/BlightedSlime");
        BlightedSlimeCrimIcon = ModContent.Request<Texture2D>("CalamityMod/Buffs/Summon/BlightedSlimeCrimson");

        CircularSmear = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmear", AssetRequestMode.AsyncLoad);
        CircularSmearFire1 = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearFire1", AssetRequestMode.AsyncLoad);
        CircularSmearFire2 = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearFire2", AssetRequestMode.AsyncLoad);
        CircularSmearFire3 = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearFire3", AssetRequestMode.AsyncLoad);
    }

    public override void Unload()
    {
        if (!Main.dedServ)
        {
            TextureAssets.FlyingCarpet = FlyingCarpetVanilla;
            TextureAssets.Buff[BuffID.Lucky] = LuckIconVanilla;
        }
    }
}
