using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    /// <summary>Vertex shading for the Battle button; no generated texture or animation.</summary>
    public sealed class MenuGradient : BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive() || mesh.currentVertCount == 0) return;
            Rect rect = ((RectTransform)transform).rect;
            UIVertex vertex = default;
            for (int i = 0; i < mesh.currentVertCount; i++)
            {
                mesh.PopulateUIVertex(ref vertex, i);
                float t = Mathf.InverseLerp(rect.yMin, rect.yMax, vertex.position.y);
                vertex.color = (Color)vertex.color * Color.Lerp(
                    new Color(0.58f, 0.58f, 0.65f), new Color(1f, 0.93f, 0.90f), t);
                mesh.SetUIVertex(vertex, i);
            }
        }
    }
}
