using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod.UI.WhisperingPearls
{
    internal class Shaking : TextEffect
    {
        private const float StandardAmp = 2f;

        public override Vector2 ModifyPos(Vector2 pos, DialogueCharacterData data, float[] args)
        {
            float amp = StandardAmp;
            if (args.Length > 0)
                amp = args[0];

            return pos + Main.rand.NextVector2Circular(amp, amp);
        }
    }
}
