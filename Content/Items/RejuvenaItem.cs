﻿using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Rejuvena.Assets;
using ReLogic.Content;
using Terraria.ModLoader;

namespace Rejuvena.Content.Items
{
    /// <summary>
    ///     Abstract base class shared between all <see cref="Rejuvena"/> items.
    /// </summary>
    public abstract class RejuvenaItem : ModItem
    {
        [Flags]
        public enum Defaults
        {
            Accessory = 0x1,
            Staff = 0x2
        }

        public override string Texture
        {
            get
            {
                if (ModContent.RequestIfExists<Texture2D>(base.Texture, out _, AssetRequestMode.ImmediateLoad))
                {
                    Mod.Logger.Debug($"[Loading] Texture exists: {base.Texture}, no fallback texture needed.");
                    return base.Texture;
                }

                try
                {
                    return GetFallbackAsset().Path;
                }
                catch (Exception e)
                {
                    // don't care enough about the stack trace
                    switch (e)
                    {
                        case NullReferenceException:
                            throw new NullReferenceException($"[Loading] {e.Message}, normal asset: {base.Texture}");

                        case NotImplementedException:
                            throw new NotImplementedException($"[Loading] {e.Message}, normal asset: {base.Texture}");
                    }

                    throw;
                }
            }
        }

        public void SetDefaultsFromEnum(Defaults defaultsToSet)
        {
            bool Any(Defaults defaults) => (defaults & defaultsToSet) != 0;

            if (Any(Defaults.Accessory))
                Item.DefaultToAccessory(Item.width, Item.height);

            if (Any(Defaults.Staff))
                Item.DefaultToStaff(Item.shoot, Item.useAnimation, Item.useTime, Item.mana);
        }

        public FallbackAsset GetFallbackAsset()
        {
            FallbackAssetType? assetType = GetType().GetCustomAttribute<FallbackAssetAttribute>()?.AssetType;

            if (assetType is null)
                throw new NullReferenceException(
                    "Attempted to retrieve a fallback asset from an unspecified asset type.");

            return assetType switch
            {
                FallbackAssetType.Default => new FallbackAsset("ModLoader/UnloadedItem", 20, 20),
                FallbackAssetType.Tome => new FallbackAsset("Rejuvena/Assets/Textures/Defaults/Fallbacks/Tome", 28, 32),
                _ => throw new NotImplementedException("Unable to fall back to an unexpected asset.")
            };
        }
    }
}