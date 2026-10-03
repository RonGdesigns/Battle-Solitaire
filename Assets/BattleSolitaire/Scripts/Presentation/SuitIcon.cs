using BattleSolitaire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    // Card suits are geometry, so Android does not need a system-font glyph fallback.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SuitIcon : MaskableGraphic
    {
        public Suit Suit;
        public static SuitIcon Create(string name,Transform parent,Suit suit,float x,float y,float xx,float yy)
        {
            var icon=FantasyUI.Rect(name,parent,x,y,xx,yy).gameObject.AddComponent<SuitIcon>();
            icon.Suit=suit; icon.raycastTarget=false; return icon;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(Suit==Suit.Diamonds) Polygon(vh,new[]{new Vector2(.5f,.98f),new Vector2(.92f,.5f),new Vector2(.5f,.02f),new Vector2(.08f,.5f)});
            else if(Suit==Suit.Clubs)
            {
                Circle(vh,.5f,.71f,.25f); Circle(vh,.28f,.43f,.25f); Circle(vh,.72f,.43f,.25f); Stem(vh);
            }
            else
            {
                var points=new Vector2[64];
                for(int i=0;i<points.Length;i++)
                {
                    float t=i*2*Mathf.PI/points.Length;
                    float x=16*Mathf.Pow(Mathf.Sin(t),3);
                    float y=13*Mathf.Cos(t)-5*Mathf.Cos(2*t)-2*Mathf.Cos(3*t)-Mathf.Cos(4*t);
                    var heart=new Vector2(.5f+x/35f,.08f+(y+17f)/33f);
                    points[i]=Suit==Suit.Hearts?heart:new Vector2(heart.x,1.04f-heart.y*.82f);
                }
                Polygon(vh,points);
                if(Suit==Suit.Spades) Stem(vh);
            }
        }
        private void Stem(VertexHelper vh) => Polygon(vh,new[]{new Vector2(.45f,.42f),new Vector2(.55f,.42f),new Vector2(.58f,.14f),new Vector2(.72f,.05f),new Vector2(.28f,.05f),new Vector2(.42f,.14f)});
        private void Circle(VertexHelper vh,float x,float y,float radius)
        {
            var points=new Vector2[32];
            for(int i=0;i<points.Length;i++) { float a=i*2*Mathf.PI/points.Length;points[i]=new Vector2(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius); }
            Polygon(vh,points);
        }
        private void Polygon(VertexHelper vh,Vector2[] points)
        {
            Rect r=rectTransform.rect; float size=Mathf.Min(r.width,r.height);
            Vector2 center=Vector2.zero;foreach(var point in points) center+=point;center/=points.Length;
            int start=vh.currentVertCount;
            vh.AddVert(r.center+(center-Vector2.one*.5f)*size,color,Vector2.zero);
            foreach(var point in points) vh.AddVert(r.center+(point-Vector2.one*.5f)*size,color,Vector2.zero);
            for(int i=0;i<points.Length;i++) vh.AddTriangle(start,start+1+i,start+1+(i+1)%points.Length);
        }
    }
}
