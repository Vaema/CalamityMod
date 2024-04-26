using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.BaseProjectiles
{
    /// <summary>
    /// An abstract class dedicated to ease the making of gun-like holdouts, regarding of the weapon class.<br/>
    /// All the AI code goes to <see cref="HoldoutAI"/>.<br/>
    /// Sets the texture, localization and SetDefaults automatically.<br/>
    /// You also have convenient properties for offsets, arm visuals and the tip of the gun.<br/>
    /// <b>You must name your holdout's class the name of the item's class plus "Holdout" at the end</b>, you'll get errors otherwise.
    /// </summary>
    public abstract class BaseGunHoldoutProjectile : ModProjectile, ILocalizedModType
    {
        #region Overridable Properties

        /// <summary>
        /// The ID of the item that this holdout belongs to.
        /// </summary>
        public abstract int AssociatedItemID { get; }

        /// <summary>
        /// The position of the gun tip, used for spawning the holdout's shots.<br/>
        /// Override if it isn't aligned properly. For example: when the tip is a bit upwards.
        /// </summary>
        public virtual Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.5f;

        /// <summary>
        /// How fast <see cref="OffsetLengthFromArm"/> returns back to <see cref="MaxOffsetLengthFromArm"/>.<br/>
        /// In consequence, this is how fast the holdout goes back to its position.<br/>
        /// Defaults to 0.3f.
        /// </summary>
        public virtual float RecoilResolveSpeed => 0.3f;

        /// <summary>
        /// The distance of the holdout to the arm.<br/>
        /// Used to properly position the weapon to be held on the player's arms.
        /// </summary>
        public virtual float MaxOffsetLengthFromArm { get; }

        /// <summary>
        /// The offset in the X-axis of the holdout when the player's pointing upwards.
        /// </summary>
        public virtual float OffsetXUpwards { get; }

        /// <summary>
        /// The offset in the X-axis of the holdout when the player's pointing downwards.
        /// </summary>
        public virtual float OffsetXDownwards { get; }

        /// <summary>
        /// The base offset on the Y-axis of the holdout.<br/>
        /// Used to align the holdout when pointing horizontally.
        /// </summary>
        public virtual float BaseOffsetY { get; }

        /// <summary>
        /// The offset in the Y-axis of the holdout when the player's pointing upwards.
        /// </summary>
        public virtual float OffsetYUpwards { get; }

        /// <summary>
        /// The offset in the Y-axis of the holdout when the player's pointing downwards.
        /// </summary>
        public virtual float OffsetYDownwards { get; }

        #endregion

        #region Properties

        /// <summary>
        /// The owner of this holdout.<br/>
        /// It's set on <see cref="OnSpawn(IEntitySource)"/>.
        /// </summary>
        public Player Owner { get; private set; }

        /// <summary>
        /// The item that the player's holding, correspondent to <see cref="AssociatedItemID"/>.<br/>
        /// It's set on <see cref="OnSpawn(IEntitySource)"/>.
        /// </summary>
        public Item HeldItem { get; private set; }

        /// <summary>
        /// Whether the holdout will have its <see cref="Projectile.timeLeft"/> set 2 constantly.<br/>
        /// Used when you want to keep it alive for a bit longer after it should despawn.<br/>
        /// Defaults to <see langword="true"/>.
        /// </summary>
        public bool KeepRefreshingLifetime { get; set; } = true;

        /// <summary>
        /// The current offset length from the arm of the holdout.<br/>
        /// Used to make recoil effects by setting this to a lower value than <see cref="MaxOffsetLengthFromArm"/>.
        /// </summary>
        public float OffsetLengthFromArm { get; set; }

        /// <summary>
        /// Extra rotation added to the front arm if you want to make it be held cooler or more naturally.<br/>
        /// Direction is already taken care of, so you don't need to multiply anything.
        /// </summary>
        public float ExtraFrontArmRotation { get; set; }

        /// <summary>
        /// Extra rotation added to the back arm if you want to make it be held cooler or more naturally.<br/>
        /// Direction is already taken care of, so you don't need to multiply anything.
        /// </summary>
        public float ExtraBackArmRotation { get; set; }

        /// <summary>
        /// The visual stretch of the owner's front arm.<br/>
        /// Defaults to <see cref="Player.CompositeArmStretchAmount.Full"/>.
        /// </summary>
        public Player.CompositeArmStretchAmount FrontArmStretch { get; set; } = Player.CompositeArmStretchAmount.Full;

        /// <summary>
        /// The visual stretch of the owner's back arm.<br/>
        /// Defaults to <see cref="Player.CompositeArmStretchAmount.Full"/>.
        /// </summary>
        public Player.CompositeArmStretchAmount BackArmStretch { get; set; } = Player.CompositeArmStretchAmount.Full;

        private Type AssociatedItemType => ItemLoader.GetItem(AssociatedItemID).GetType();

        #endregion

        #region Overridden Members

        // Gets the display name of the associated item.
        public override LocalizedText DisplayName => CalamityUtils.GetItemName(AssociatedItemID);

        // Gets the texture of the associated item.
        public override string Texture => (AssociatedItemType.Namespace + "." + AssociatedItemType.Name).Replace('.', '/');

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = TextureAssets.Item[AssociatedItemID].Width();
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Owner = Main.player[Projectile.owner];
            OffsetLengthFromArm = MaxOffsetLengthFromArm;
            HeldItem = Owner.ActiveItem();
            Projectile.velocity = Owner.Calamity().mouseWorld - Owner.RotatedRelativePoint(Owner.MountedCenter);
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? CanDamage() => false;

        #endregion

        #region AI

        public override void AI()
        {
            KillHoldoutLogic();
            ManageHoldout();
            HoldoutAI();
        }

        /// <summary>
        /// Here goes the code for when the holdout needs to despawn.<br/>
        /// Defaults to despawning when the owner isn't holding or isn't using the correspondent weapon.
        /// </summary>
        public virtual void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout() || HeldItem.type != AssociatedItemID)
            {
                Projectile.netUpdate = true;
                Projectile.Kill();
            }
        }

        /// <summary>
        /// Does all the needed code for a holdout to work.
        /// </summary>
        public virtual void ManageHoldout()
        {
            // The center of the player, taking into account if they have a mount or not.
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);

            // The vector between the player and the mouse, used for pointing the holdout.
            Vector2 ownerToMouse = Owner.Calamity().mouseWorld - armPosition;

            // The direction this holdout's pointing at.
            float holdoutDirection = Projectile.velocity.ToRotation();

            // A range from -1 to 1 for when the holdout is pointing downards of upwards, respectively.
            // Used for the offsets.
            float proximityLookingUpwards = Vector2.Dot(ownerToMouse.SafeNormalize(Vector2.Zero), -Vector2.UnitY);

            int direction = MathF.Sign(ownerToMouse.X);

            Vector2 lengthOffset = Projectile.rotation.ToRotationVector2() * OffsetLengthFromArm;
            Vector2 armOffset = new Vector2(Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? OffsetXUpwards : OffsetXDownwards) * direction, BaseOffsetY + Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? OffsetYUpwards : OffsetYDownwards));
            Projectile.Center = armPosition + lengthOffset + armOffset;
            Projectile.velocity = holdoutDirection.AngleTowards(ownerToMouse.ToRotation(), 0.2f).ToRotationVector2();
            Projectile.rotation = holdoutDirection;

            Projectile.spriteDirection = direction;
            Owner.ChangeDir(direction);

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 2;
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

            // -Pi/2 because the arms rotation starts with arms pointing down.
            float armRotation = Projectile.rotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(true, FrontArmStretch, armRotation + ExtraFrontArmRotation * direction);
            Owner.SetCompositeArmBack(true, BackArmStretch, armRotation + ExtraBackArmRotation * direction);

            if (KeepRefreshingLifetime)
                Projectile.timeLeft = 2;

            if (OffsetLengthFromArm != MaxOffsetLengthFromArm)
                OffsetLengthFromArm = MathHelper.Lerp(OffsetLengthFromArm, MaxOffsetLengthFromArm, RecoilResolveSpeed);
        }

        /// <summary>
        /// Where all the actual AI goes to.
        /// </summary>
        public abstract void HoldoutAI();

        #endregion

        #region Drawing

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale, flipSprite);

            return false;
        }

        #endregion
    }
}
