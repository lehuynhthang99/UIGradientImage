using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Nura.CustomGradientImage
{
    public partial class CustomVerticalGradientUIMesh : MaskableGraphic
    {
        [SerializeField]
        private Sprite m_Sprite;

        public Sprite sprite
        {
            get { return m_Sprite; }
            set
            {
                if (m_Sprite != null)
                {
                    if (m_Sprite != value)
                    {
                        m_SkipLayoutUpdate = m_Sprite.rect.size.Equals(value ? value.rect.size : Vector2.zero);
                        m_SkipMaterialUpdate = m_Sprite.texture == (value ? value.texture : null);
                        m_Sprite = value;

                        SetAllDirty();
                        TrackSprite();
                    }
                }
                else if (value != null)
                {
                    m_SkipLayoutUpdate = value.rect.size == Vector2.zero;
                    m_SkipMaterialUpdate = value.texture == null;
                    m_Sprite = value;

                    SetAllDirty();
                    TrackSprite();
                }
            }
        }

        public float pixelsPerUnit
        {
            get
            {
                float spritePixelsPerUnit = 100;
                if (sprite)
                    spritePixelsPerUnit = sprite.pixelsPerUnit;

                if (canvas)
                    m_CachedReferencePixelsPerUnit = canvas.referencePixelsPerUnit;

                return spritePixelsPerUnit / m_CachedReferencePixelsPerUnit;
            }
        }

        // Whether this is being tracked for Atlas Binding.
        private bool m_Tracked = false;
        private float m_CachedReferencePixelsPerUnit = 100;

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetVerticesDirty();
            SetMaterialDirty();
        }

        // if no texture is configured, use the default white texture as mainTexture
        public override Texture mainTexture
        {
            get
            {
                if (sprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return sprite.texture;
            }
        }


        #region Image function copied

        [ContextMenu("Set Native Size")]
        public override void SetNativeSize()
        {

            if (sprite != null)
            {
                float pixelsPerUnitCachedLocal = pixelsPerUnit;
                float w = sprite.rect.width / pixelsPerUnitCachedLocal;
                float h = sprite.rect.height / pixelsPerUnitCachedLocal;
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
                SetAllDirty();
            }
        }

        private void TrackSprite()
        {
            if (sprite != null && sprite.texture == null)
            {
                TrackImage(this);
                m_Tracked = true;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TrackSprite();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_Tracked)
                UnTrackImage(this);
        }

        #endregion

        #region Image function copied static

        // To track textureless images, which will be rebuild if sprite atlas manager registered a Sprite Atlas that will give this image new texture
        static List<CustomVerticalGradientUIMesh> m_TrackedTexturelessImages = new List<CustomVerticalGradientUIMesh>();
        static bool s_Initialized;

        static void RebuildImage(SpriteAtlas spriteAtlas)
        {
            for (var i = m_TrackedTexturelessImages.Count - 1; i >= 0; i--)
            {
                var g = m_TrackedTexturelessImages[i];
                if (null != g.sprite && spriteAtlas.CanBindTo(g.sprite))
                {
                    g.SetAllDirty();
                    m_TrackedTexturelessImages.RemoveAt(i);
                }
            }
        }

        private static void TrackImage(CustomVerticalGradientUIMesh g)
        {
            if (!s_Initialized)
            {
                SpriteAtlasManager.atlasRegistered += RebuildImage;
                s_Initialized = true;
            }

            m_TrackedTexturelessImages.Add(g);
        }

        private static void UnTrackImage(CustomVerticalGradientUIMesh g)
        {
            m_TrackedTexturelessImages.Remove(g);
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetMaterialDirty();
            SetVerticesDirty();
            SetRaycastDirty();
        }

        #endregion
    }
}