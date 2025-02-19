using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Nura.CustomGradientImage
{
    public partial class CustomVerticalGradientUIMesh : MaskableGraphic
    {
        [Serializable]
        public struct GradientPoint
        {
            public float normalizedPosition;
            public Color vertexColor;
        }

        [SerializeField]
        GradientPoint[] _gradientPoints;

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            _gradientPoints = new GradientPoint[]
            {
                new GradientPoint()
                {
                    normalizedPosition = 1,
                    vertexColor = Color.white,
                },
                new GradientPoint()
                {
                    normalizedPosition = 0,
                    vertexColor = Color.white,
                }
            };
        }
#endif

        //client code can interact with this to change gradinet
        public void SetGradientPoints(GradientPoint[] gradientPoints)
        {
            if (gradientPoints == null || gradientPoints.Length < 2)
            {
                return;
            }

            _gradientPoints = gradientPoints;
            SetVerticesDirty();
        }

        // helper to easily create quads for our ui mesh. You could make any triangle-based geometry other than quads, too!
        void AddQuad(VertexHelper vh, Vector2 corner1, Vector2 corner2, Vector2 uvCorner1, Vector2 uvCorner2, Color clrCorner1, Color clrCorner2)
        {
            float rectWidth = rectTransform.rect.width;
            float rectHeight = rectTransform.rect.height;
            var i = vh.currentVertCount;

            UIVertex vert = new UIVertex();

            vert.color = clrCorner1;
            vert.position = corner1;
            vert.uv0 = uvCorner1;
            vh.AddVert(vert);

            vert.color = clrCorner1;
            vert.position = new Vector2(corner2.x, corner1.y);
            vert.uv0 = new Vector2(uvCorner2.x, uvCorner1.y);
            vh.AddVert(vert);

            vert.color = clrCorner2;
            vert.position = corner2;
            vert.uv0 = uvCorner2;
            vh.AddVert(vert);

            vert.color = clrCorner2;
            vert.position = new Vector2(corner1.x, corner2.y);
            vert.uv0 = new Vector2(uvCorner1.x, uvCorner2.y);
            vh.AddVert(vert);

            vh.AddTriangle(i + 0, i + 2, i + 1);
            vh.AddTriangle(i + 3, i + 2, i + 0);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var bottomLeftCorner = new Vector2(0, 0) - rectTransform.pivot;

            float rectWidth = rectTransform.rect.width;
            float rectHeight = rectTransform.rect.height;
            bottomLeftCorner.x *= rectWidth;
            bottomLeftCorner.y *= rectHeight;

            //x,y: bottom left corner; z,w: top right corner
            //Vector4 border = new Vector4(bottomLeftCorner.x, bottomLeftCorner.y, bottomLeftCorner.x + rect.width, bottomLeftCorner.y + rect.height);

            //handle if sprite in sprite atlas
            Vector4 spriteUVInAtlas = sprite ? DataUtility.GetOuterUV(sprite) : Vector4.zero;
            Vector2 horizontalMap = new Vector2(spriteUVInAtlas.x, spriteUVInAtlas.z);
            Vector2 verticalMap = new Vector2(spriteUVInAtlas.y, spriteUVInAtlas.w);

            //add vertex for each point in gradient
            for (int i = _gradientPoints.Length - 1; i > 0; i--)
            {
                GradientPoint firstGradientPoint = _gradientPoints[i];
                GradientPoint secondGradientPoint = _gradientPoints[i - 1];
                Vector2 corner1 = new Vector2(bottomLeftCorner.x + 0, bottomLeftCorner.y + firstGradientPoint.normalizedPosition * rectHeight);
                Vector2 corner2 = new Vector2(bottomLeftCorner.x + rectWidth, bottomLeftCorner.y + secondGradientPoint.normalizedPosition * rectHeight);
                Vector2 uvCorner1 = new Vector2(0, firstGradientPoint.normalizedPosition);
                Vector2 uvCorner2 = new Vector2(1, secondGradientPoint.normalizedPosition);

                //remap from (0, 1) uv coordinate to the sprite uv in sprite atlas coordinate
                uvCorner1.x = Mathf.Lerp(horizontalMap.x, horizontalMap.y, uvCorner1.x);
                uvCorner2.x = Mathf.Lerp(horizontalMap.x, horizontalMap.y, uvCorner2.x);
                uvCorner1.y = Mathf.Lerp(verticalMap.x, verticalMap.y, uvCorner1.y);
                uvCorner2.y = Mathf.Lerp(verticalMap.x, verticalMap.y, uvCorner2.y);

                AddQuad(vh, corner1, corner2, uvCorner1, uvCorner2, firstGradientPoint.vertexColor, secondGradientPoint.vertexColor);
            }
        }
    }
}