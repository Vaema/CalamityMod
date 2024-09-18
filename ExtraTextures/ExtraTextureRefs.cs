using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.ExtraTextures
{
    [Autoload(Side = ModSide.Client)]
    internal sealed class ExtraTextureRefs : ModSystem
    {
        // Destroyer glowmasks
        public static Asset<Texture2D> DestroyerHeadGlowmask;
        public static Asset<Texture2D> DestroyerBodyGlowmask;
        public static Asset<Texture2D> DestroyerTailGlowmask;
        public static Asset<Texture2D> ProbeGlowmask;

        // Master Rev+ Skeletron Prime
        public static Asset<Texture2D> ChadPrime;
        public static Asset<Texture2D> ChadPrimeEyeGlowmask;

        // WoF, These are not "exactly" the ExtraTextures. But 
        public static Asset<Texture2D> WallOfFleshEyeGlowmask;
        public static Asset<Texture2D> WallOfFleshDemonSickleTexture;

        // Flying Carpet Replacements
        public static Asset<Texture2D> FlyingCarpetVanilla;
        public static Asset<Texture2D> FlyingCarpetAuric;

        // Boss Heads
        // I know it's not exactly the "Texture" but it belongs to ExtraTextures so 🤷
        public static int BossHeadIndex_ChadPrime;

        public override void Load()
        {
            string chadPrimeIconPath = "CalamityMod/ExtraTextures/ChadPrime_Head_Boss";
            BossHeadIndex_ChadPrime = CalamityMod.Instance.AddBossHeadTexture(chadPrimeIconPath, -1);
        }

        public override void OnModLoad()
        {
            DestroyerHeadGlowmask = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBossGlowmasks/DestroyerHeadGlow", AssetRequestMode.AsyncLoad);
            DestroyerBodyGlowmask = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBossGlowmasks/DestroyerBodyGlow", AssetRequestMode.AsyncLoad);
            DestroyerTailGlowmask = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBossGlowmasks/DestroyerTailGlow", AssetRequestMode.AsyncLoad);

            ProbeGlowmask = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBossGlowmasks/ProbeGlow", AssetRequestMode.AsyncLoad);

            ChadPrime = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/ChadPrime", AssetRequestMode.AsyncLoad);
            ChadPrimeEyeGlowmask = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/ChadPrimeHeadGlow", AssetRequestMode.AsyncLoad);

            WallOfFleshEyeGlowmask = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBossGlowmasks/WallOfFleshEyeTelegraphGlow", AssetRequestMode.AsyncLoad);
            WallOfFleshDemonSickleTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/ForbiddenOathbladeProjectile", AssetRequestMode.AsyncLoad);

            FlyingCarpetVanilla = TextureAssets.FlyingCarpet;
            FlyingCarpetAuric = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/AuricCarpet", AssetRequestMode.AsyncLoad);
        }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                TextureAssets.FlyingCarpet = FlyingCarpetVanilla;
            }
        }
    }
}
