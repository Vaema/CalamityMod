using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.UI.DialogueDisplay.DisplayEffects
{
    public class DisplayEffect
    {
        public virtual Vector2 TextOffsetFromStart(Vector2 startPos, Vector2 textSize) => startPos + new Vector2(-textSize.X / 2f, -(textSize.Y + 40));

        public virtual bool FadeWhenTooFar => true;

        public virtual bool DespawnWithAttachedNPC => true;

        public virtual float FadeBuffer => 150f;

        public virtual float FadeDistance => 150f;

        public virtual void PreDraw(SpriteBatch spriteBatch, Vector2 textStart, Vector2 textSize, int textTimer, int switchTimer) { }

        public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 textStart, Vector2 textSize, int textTimer, int switchTimer) { }

        public virtual float TimeToAppear => 30;

        #region Appear Functions
        public virtual Vector2 AppearPositioning(Vector2 startPos, Vector2 goalPos, float time, DialogueCharacterData charData) => Vector2.Lerp(startPos, goalPos, CalamityUtils.SineOutEasing(time / TimeToAppear, 1));

        public virtual Color AppearColoring(Color goalColor, float time, DialogueCharacterData charData) => goalColor;

        public virtual float AppearOpacity(float goalOpacity, float time, DialogueCharacterData charData) => CalamityUtils.SineOutEasing(MathHelper.Clamp(time / 20f, 0f, 1f), 1);

        public virtual float AppearRotation(float goalRotation, float time, DialogueCharacterData charData) => goalRotation;

        public virtual Vector2 AppearScale(Vector2 goalScale, float time, DialogueCharacterData charData) => Vector2.Lerp(Vector2.Zero, goalScale, CalamityUtils.CircOutEasing(time / TimeToAppear, 1));
        #endregion

        public virtual float TimeToDisappear => 30;

        #region Disappear Functions
        public virtual Vector2 DisappearPositioning(Vector2 startPos, float time, DialogueCharacterData charData) => startPos;

        public virtual Color DisappearColoring(Color startColor, float time, DialogueCharacterData charData) => startColor;

        public virtual float DisappearOpacity(float startOpacity, float time, DialogueCharacterData charData) => 1 - CalamityUtils.SineOutEasing(MathHelper.Clamp(time / (TimeToDisappear * 0.66f), 0f, 1f), 1);

        public virtual float DisappearRotation(float startRotation, float time, DialogueCharacterData charData) => startRotation;

        public virtual Vector2 DisappearScale(Vector2 startScale, float time, DialogueCharacterData charData) => Vector2.Lerp(startScale, startScale * 1.5f, CalamityUtils.ExpOutEasing(time / TimeToDisappear, 1));
        #endregion
    }

}
