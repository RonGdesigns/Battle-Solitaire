using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    /// <summary>Scalable clipped metal frame, rendered as UI geometry rather than a stretched bitmap.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FantasyFrame : MaskableGraphic
    {
        public Color Edge = new Color32(205, 163, 92, 255);
        public float Thickness = 3f;
        public bool Ornate;
        public bool Focused, Hovered, Glow;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            float cut = Mathf.Min(18f, Mathf.Min(r.width, r.height) * 0.12f);
            if(Glow) { Ring(vh,r,cut,-5,8,.16f);Ring(vh,r,cut,-2,4,.28f); }
            Ring(vh, r, cut, 0, Thickness, Hovered ? 1f : .85f);
            Ring(vh, r, cut, Thickness + 3f, 1f, 0.50f);
            if(Focused)
            {
                Color saved=Edge;Edge=new Color32(167,233,251,255);
                Ring(vh,r,cut,Thickness+5,2f,1f);Edge=saved;
            }
            if (!Ornate) return;
            Ring(vh, r, cut, Thickness + 7f, 1.5f, 0.9f);
            for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
            {
                Vector2 origin = new Vector2(x == 0 ? r.xMin + 15 : r.xMax - 15, y == 0 ? r.yMin + 15 : r.yMax - 15);
                Vector2 direction = new Vector2(x == 0 ? 1 : -1, y == 0 ? 1 : -1);
                Line(vh, origin + Vector2.Scale(new Vector2(0, 28), direction), origin + Vector2.Scale(new Vector2(8, 8), direction), 2, Edge);
                Line(vh, origin + Vector2.Scale(new Vector2(28, 0), direction), origin + Vector2.Scale(new Vector2(8, 8), direction), 2, Edge);
                Line(vh, origin + Vector2.Scale(new Vector2(5, 20), direction), origin + Vector2.Scale(new Vector2(20, 5), direction), 1, Edge);
            }
        }

        private void Ring(VertexHelper vh, Rect r, float cut, float inset, float width, float strength)
        {
            Vector2[] outside = Points(r, cut, inset);
            Vector2[] inside = Points(r, cut, inset + width);
            for (int i = 0; i < 8; i++)
            {
                int next = (i + 1) % 8;
                float metal = (i == 2 || i == 3 || i == 4) ? 1.15f : (i == 0 || i == 7 ? 0.50f : 0.8f);
                Color c = new Color(Edge.r * metal, Edge.g * metal, Edge.b * metal, Edge.a * strength);
                Quad(vh, outside[i], outside[next], inside[next], inside[i], c);
            }
        }
        private static Vector2[] Points(Rect r, float cut, float inset)
        {
            float l = r.xMin + inset, b = r.yMin + inset, t = r.yMax - inset, rt = r.xMax - inset;
            float c = Mathf.Max(3, cut - inset * 0.35f);
            return new[] { new Vector2(l+c,b), new Vector2(l,b+c), new Vector2(l,t-c), new Vector2(l+c,t),
                new Vector2(rt-c,t), new Vector2(rt,t-c), new Vector2(rt,b+c), new Vector2(rt-c,b) };
        }
        internal static void Line(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 n = new Vector2(-(b-a).y, (b-a).x).normalized * width * 0.5f;
            Quad(vh,a+n,b+n,b-n,a-n,color);
        }
        internal static void Quad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int start=vh.currentVertCount;
            vh.AddVert(a,color,Vector2.zero); vh.AddVert(b,color,Vector2.zero);
            vh.AddVert(c,color,Vector2.zero); vh.AddVert(d,color,Vector2.zero);
            vh.AddTriangle(start,start+1,start+2); vh.AddTriangle(start,start+2,start+3);
        }
    }
}
