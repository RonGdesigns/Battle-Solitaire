using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public enum FantasySymbol { Swords, Shield, Lock, Fog, Spade, Energy }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FantasyIcon : MaskableGraphic
    {
        public FantasySymbol Symbol;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (Symbol == FantasySymbol.Swords)
            {
                Sword(vh, false); Sword(vh, true);
            }
            else if (Symbol == FantasySymbol.Shield)
            {
                Polygon(vh, new[] { P(.18f,.88f), P(.5f,.98f), P(.82f,.88f), P(.78f,.40f), P(.5f,.06f), P(.22f,.40f) });
                Line(vh,.5f,.86f,.5f,.22f,2.5f);
            }
            else if (Symbol == FantasySymbol.Lock)
            {
                Polygon(vh,new[] { P(.16f,.55f),P(.84f,.55f),P(.84f,.08f),P(.16f,.08f) });
                for(int i=0;i<20;i++)
                {
                    float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;
                    FantasyFrame.Line(vh,P(.5f+Mathf.Cos(a)*.23f,.55f+Mathf.Sin(a)*.37f),P(.5f+Mathf.Cos(b)*.23f,.55f+Mathf.Sin(b)*.37f),5,color);
                }
                FantasyFrame.Line(vh,P(.5f,.39f),P(.5f,.22f),4,new Color32(6,20,33,255));
            }
            else if(Symbol == FantasySymbol.Fog)
            {
                Circle(vh,.28f,.4f,.22f); Circle(vh,.52f,.58f,.28f); Circle(vh,.76f,.37f,.20f);
                Line(vh,.17f,.18f,.8f,.18f,2);
            }
            else if(Symbol == FantasySymbol.Energy)
                Polygon(vh,new[]{ P(.56f,1),P(.15f,.4f),P(.44f,.4f),P(.32f,0),P(.86f,.62f),P(.57f,.62f) });
            else
            {
                Polygon(vh,new[]{P(.5f,.98f),P(.12f,.43f),P(.20f,.24f),P(.42f,.31f),P(.35f,.06f),P(.65f,.06f),P(.58f,.31f),P(.8f,.24f),P(.88f,.43f)});
                Line(vh,.5f,.85f,.5f,.28f,1.5f);
            }
        }
        private Vector2 P(float x,float y) { Rect r=rectTransform.rect; return new Vector2(r.xMin+x*r.width,r.yMin+y*r.height); }
        private void Line(VertexHelper vh,float x,float y,float xx,float yy,float width) => FantasyFrame.Line(vh,P(x,y),P(xx,yy),width,new Color32(233,232,211,220));
        private void Polygon(VertexHelper vh,Vector2[] p)
        {
            Vector2 center=Vector2.zero; foreach(Vector2 point in p)center+=point; center/=p.Length;
            int start=vh.currentVertCount;
            vh.AddVert(center,color,Vector2.zero);
            for(int i=0;i<p.Length;i++) vh.AddVert(p[i], Color.Lerp(color,new Color32(236,241,237,255),(i%3)*.18f),Vector2.zero);
            for(int i=0;i<p.Length;i++) vh.AddTriangle(start,start+1+i,start+1+(i+1)%p.Length);
        }
        private void Circle(VertexHelper vh,float x,float y,float radius)
        {
            Vector2[] p=new Vector2[24];
            for(int i=0;i<p.Length;i++){float a=i*2*Mathf.PI/p.Length;p[i]=P(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius);}
            Polygon(vh,p);
        }
        private void Sword(VertexHelper vh,bool flip)
        {
            Vector2 a=P(flip?.78f:.22f,.1f), b=P(flip?.22f:.78f,.90f);
            Vector2 d=(b-a).normalized, n=new Vector2(-d.y,d.x), mid=Vector2.Lerp(a,b,.28f);
            FantasyFrame.Quad(vh,mid+n*4,b+n*3,b+d*7,mid-n*4,color);
            FantasyFrame.Line(vh,a,mid,6,color);
            FantasyFrame.Line(vh,mid+n*11,mid-n*11,5,color);
            FantasyFrame.Line(vh,mid,b,1.3f,new Color32(251,232,190,255));
        }
    }
}
