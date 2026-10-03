using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BattleSolitaire.Core;

namespace BattleSolitaire.Presentation
{
    public sealed class TutorialOverlay : MonoBehaviour
    {
        private const string TutorialKey="BattleSolitaire.TutorialSeen.v1";
        private readonly string[] _titles={"BUILD DOWN","ACES GO ANYWHERE","TURN CARDS INTO POWER","TAKE A BREATHER"};
        private readonly string[] _bodies={
            "Build downward in alternating colors. Place a red 4 on a black 5. Drop on the last card or anywhere in its column. Only kings can start an empty column.",
            "Start any empty foundation with any ace. That pile then follows its suit: ace, 2, 3, all the way to king. Double-tap an exposed card to send it to a legal foundation.",
            "Foundation moves damage your rival and build shield. Earn energy for abilities. Fog hides only foundations for 2 seconds, with a 20-second cooldown. Your tableau stays readable.",
            "Pause freezes both boards. Save & Loadout keeps your match for later. Quit Battle ends the match and returns to the title screen. To win, reduce your rival to 0 HP or finish all four foundations."
        };
        private Text _stepText,_title,_body,_nextLabel;
        private Button _next,_back,_close;
        private GameObject _previousFocus;
        private int _step;
        public bool IsOpen=>gameObject.activeSelf;
        public static TutorialOverlay Create(Transform parent,BattleGameController controller)
        {
            var root=FantasyUI.Rect("TutorialOverlay",parent,0,0,1,1);
            root.gameObject.AddComponent<Image>().color=new Color32(3,9,18,245);
            var view=root.gameObject.AddComponent<TutorialOverlay>();view.Build();
            root.gameObject.SetActive(false);return view;
        }
        public void OpenIfNeeded() { if(PlayerPrefs.GetInt(TutorialKey,0)==0) Open(); }
        public void Open()
        {
            _previousFocus=EventSystem.current?.currentSelectedGameObject;CardView.CancelCurrentDrag();
            _step=0;gameObject.SetActive(true);transform.SetAsLastSibling();RefreshStep();
        }
        public void CloseAndRemember()
        {
            PlayerPrefs.SetInt(TutorialKey,1);PlayerPrefs.Save();gameObject.SetActive(false);
            if(_previousFocus!=null && _previousFocus.activeInHierarchy) EventSystem.current?.SetSelectedGameObject(_previousFocus);
        }
        private void Next() { if(_step==_titles.Length-1) CloseAndRemember();else { _step++;RefreshStep(); } }
        private void RefreshStep()
        {
            _stepText.text="HOW TO PLAY  ·  "+(_step+1)+" / "+_titles.Length;
            _title.text=_titles[_step];_body.text=_bodies[_step];_nextLabel.text=_step==_titles.Length-1?"DONE":"NEXT";
            _back.interactable=_step>0;
            var controls=_step>0?new[]{_back,_next,_close}:new[]{_next,_close};
            for(int i=0;i<controls.Length;i++) controls[i].navigation=new Navigation {
                mode=Navigation.Mode.Explicit,selectOnUp=controls[(i+controls.Length-1)%controls.Length],selectOnDown=controls[(i+1)%controls.Length],
                selectOnLeft=controls[(i+controls.Length-1)%controls.Length],selectOnRight=controls[(i+1)%controls.Length]
            };
            EventSystem.current?.SetSelectedGameObject(_next.gameObject);
        }
        private void Build()
        {
            var panel=FantasyUI.Panel("TutorialPanel",transform,.055f,.13f,.945f,.87f,FantasyUI.Gold,true).transform;
            _stepText=FantasyUI.Label("Step",panel,"",26,FantasyUI.Muted,.07f,.89f,.93f,.965f,true,TextAnchor.MiddleCenter);
            _title=FantasyUI.Label("TutorialTitle",panel,"",40,FantasyUI.Gold,.06f,.765f,.94f,.88f,true,TextAnchor.MiddleCenter);
            var suits=FantasyUI.Rect("SuitRow",panel,.24f,.625f,.76f,.735f);
            for(int i=0;i<4;i++) { var icon=SuitIcon.Create("Suit"+i,suits,(Suit)i,i*.25f,0,i*.25f+.18f,1);icon.color=i==1||i==2?new Color32(235,112,123,255):FantasyUI.Silver; }
            _body=FantasyUI.Label("TutorialBody",panel,"",44,FantasyUI.Silver,.09f,.255f,.91f,.60f,false,TextAnchor.UpperLeft);
            _body.horizontalOverflow=HorizontalWrapMode.Wrap;_body.verticalOverflow=VerticalWrapMode.Truncate;
            _back=FantasyUI.Button("Back",panel,"BACK",.08f,.115f,.47f,.22f,FantasyUI.Muted,32);
            _next=FantasyUI.Button("Next",panel,"NEXT",.53f,.115f,.92f,.22f,FantasyUI.Blue,32);
            _nextLabel=_next.GetComponentInChildren<Text>();_next.onClick.AddListener(Next);
            _back.onClick.AddListener(()=>{_step=Mathf.Max(0,_step-1);RefreshStep();});
            _close=FantasyUI.Button("CloseHelp",transform,"CLOSE",.32f,.037f,.68f,.10f,FantasyUI.Muted,29);
            _close.onClick.AddListener(CloseAndRemember);
        }
    }
}
