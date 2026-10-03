using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public static class FantasyUI
    {
        public static readonly Color Gold = new Color32(226,190,120,255);
        public static readonly Color Silver = new Color32(222,233,243,255);
        public static readonly Color Blue = new Color32(111,204,245,255);
        public static readonly Color Muted = new Color32(174,199,215,255);
        public static readonly Color Dark = new Color32(5,15,25,246);
        private static Font _display, _body;
        public static Font Display => _display != null ? _display : (_display = Resources.Load<Font>("Fonts/Cinzel") ?? PrototypeUI.Font);
        public static Font Body => _body != null ? _body : (_body = Resources.Load<Font>("Fonts/CormorantGaramond") ?? PrototypeUI.Font);
        public static RectTransform Rect(string name,Transform parent,float x,float y,float xx,float yy)
            => PrototypeUI.CreateRect(name,parent,new Vector2(x,y),new Vector2(xx,yy),Vector2.zero,Vector2.zero);
        public static void Box(RectTransform rect,float x,float y,float xx,float yy)
            => PrototypeUI.SetAnchoredBox(rect,new Vector2(x,y),new Vector2(xx,yy),Vector2.zero,Vector2.zero);
        public static Image Panel(string name,Transform parent,float x,float y,float xx,float yy,Color edge,bool ornate=false)
        {
            RectTransform r=Rect(name,parent,x,y,xx,yy);
            Image fill=r.gameObject.AddComponent<Image>();fill.color=Dark;fill.raycastTarget=false;
            RawImage texture=Art("Material",r,GameArt.GetBackdrop(),0,0,1,1);
            texture.uvRect=new Rect(.2f,.1f,.6f,.45f);texture.color=new Color(.55f,.65f,.8f,.40f);
            Frame(r,edge,ornate);
            return fill;
        }
        public static FantasyFrame Frame(Transform parent,Color edge,bool ornate=false)
        {
            var r=Rect("MetalFrame",parent,0,0,1,1);
            FantasyFrame frame=r.gameObject.AddComponent<FantasyFrame>();frame.Edge=edge;frame.Ornate=ornate;frame.raycastTarget=false;
            return frame;
        }
        public static RawImage Art(string name,Transform parent,Texture texture,float x,float y,float xx,float yy)
        {
            RawImage image=Rect(name,parent,x,y,xx,yy).gameObject.AddComponent<RawImage>();
            image.texture=texture;image.raycastTarget=false;image.enabled=texture!=null;return image;
        }
        public static RawImage Portrait(string name,Transform parent,BattlerId id,float x,float y,float xx,float yy)
        {
            RawImage image=Art(name,parent,null,x,y,xx,yy);
            image.gameObject.AddComponent<AspectFillRawImage>();GameArt.SetPortrait(image,id);return image;
        }
        public static Text Label(string name,Transform parent,string value,int size,Color color,float x,float y,float xx,float yy,
            bool heading=false,TextAnchor alignment=TextAnchor.MiddleLeft)
        {
            Text text=PrototypeUI.CreateText(name,parent,value,size,alignment,color);
            text.font=heading?Display:Body;text.fontStyle=FontStyle.Normal;
            Box(text.rectTransform,x,y,xx,yy);return text;
        }
        public static Button Button(string name,Transform parent,string label,float x,float y,float xx,float yy,Color edge,int size=26)
        {
            Image image=Panel(name,parent,x,y,xx,yy,edge);
            image.raycastTarget=true;
            Button button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
            ColorBlock c=button.colors;c.normalColor=Color.white;c.highlightedColor=new Color(1.3f,1.3f,1.3f);
            c.selectedColor=new Color(1.5f,1.5f,1.5f);c.pressedColor=new Color(.65f,.65f,.65f);c.disabledColor=new Color(.5f,.5f,.5f);c.fadeDuration=0;button.colors=c;
            image.gameObject.AddComponent<FantasyButtonFeedback>();
            Label("Label",image.transform,label,size,Silver,.04f,.05f,.96f,.95f,true,TextAnchor.MiddleCenter);
            return button;
        }
        public static FantasyIcon Icon(string name,Transform parent,FantasySymbol symbol,Color color,float x,float y,float xx,float yy)
        {
            FantasyIcon icon=Rect(name,parent,x,y,xx,yy).gameObject.AddComponent<FantasyIcon>();
            icon.Symbol=symbol;icon.color=color;icon.raycastTarget=false;return icon;
        }
        public static void Logo(Transform parent,float x,float y,float xx,float yy)
        {
            RectTransform root=Rect("GameLogo",parent,x,y,xx,yy);
            Icon("Crest",root,FantasySymbol.Spade,Gold,.43f,.79f,.57f,1f);
            Text first=Label("BattleLogo",root,"BATTLE",62,Gold,0,.40f,1,.79f,true,TextAnchor.MiddleCenter);
            Label("SolitaireLogo",root,"SOLITAIRE",47,Silver,0,.16f,1,.51f,true,TextAnchor.MiddleCenter);
            Label("Motto",root,"SKILL PLAYS. HIGHER STAKES.",19,Gold,0,0,1,.18f,true,TextAnchor.MiddleCenter);
            Shadow shadow=first.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(.02f,.03f,.05f,.9f);shadow.effectDistance=new Vector2(2,-3);
        }
        public static void Backdrop(Transform parent)
        {
            RawImage art=Art("CastleBackdrop",parent,GameArt.GetBackdrop(),0,0,1,1);
            art.gameObject.AddComponent<AspectFillRawImage>().SetTexture(art.texture);
            art.transform.SetAsFirstSibling();
            Image shade=PrototypeUI.CreatePanel("HeaderShade",parent,new Vector2(0,.823f),Vector2.one,Vector2.zero,Vector2.zero,new Color32(3,13,24,115));
            shade.raycastTarget=false;shade.transform.SetSiblingIndex(1);
        }
    }
}
