using Terraria.ModLoader;

namespace CalamityMod.ExtraTextures.GreyscaleGradients
{
    internal class GreyscaleGradient : ILoadable
    {
        public string BasePath => "CalamityMod/ExtraTextures/GreyscaleGradients/";

        public static GrayscaleTexture1D CinderplatePulse { get; private set; }
        public static GrayscaleTexture1D ElumplatePulse { get; private set; }
        public static GrayscaleTexture1D HavocplatePulse { get; private set; }
        public static GrayscaleTexture1D NavyplatePulse { get; private set; }
        public static GrayscaleTexture1D OnyxplatePulse { get; private set; }
        public static GrayscaleTexture1D PlagueContainmentCellsPulse { get; private set; }

        public static GrayscaleTexture2D BlobbyNoise { get; private set; }

        public void Load(Mod mod)
        {
            CinderplatePulse = new($"{BasePath}{nameof(CinderplatePulse)}");
            ElumplatePulse = new($"{BasePath}{nameof(ElumplatePulse)}");
            HavocplatePulse = new($"{BasePath}{nameof(HavocplatePulse)}");
            NavyplatePulse = new($"{BasePath}{nameof(NavyplatePulse)}");
            OnyxplatePulse = new($"{BasePath}{nameof(OnyxplatePulse)}");
            PlagueContainmentCellsPulse = new($"{BasePath}{nameof(PlagueContainmentCellsPulse)}");

            BlobbyNoise = new($"{BasePath}{nameof(BlobbyNoise)}");
        }

        public void Unload()
        {
            CinderplatePulse?.Unload();
            ElumplatePulse?.Unload();
            HavocplatePulse?.Unload();
            NavyplatePulse?.Unload();
            OnyxplatePulse?.Unload();
            PlagueContainmentCellsPulse?.Unload();
            BlobbyNoise?.Unload();

            CinderplatePulse = null;
            ElumplatePulse = null;
            HavocplatePulse = null;
            NavyplatePulse = null;
            OnyxplatePulse = null;
            PlagueContainmentCellsPulse = null;
            BlobbyNoise = null;
        }
    }
}
