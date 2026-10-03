using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class TitleScreenView : MonoBehaviour
    {
        private BattleGameController _controller;
        private Button _play, _continue, _help, _settings;
        public bool IsOpen => gameObject.activeSelf;
        public static TitleScreenView Create(Transform parent,BattleGameController controller)
        {
            var root=FantasyUI.Rect("TitleScreen",parent,0,0,1,1);
            root.gameObject.AddComponent<Image>().color=PrototypeUI.Background;
            var view=root.gameObject.AddComponent<TitleScreenView>();view._controller=controller;
            FantasyUI.Backdrop(root);FantasyUI.Logo(root,.12f,.735f,.88f,.965f);
            root.Find("HeaderShade").gameObject.SetActive(false);
            root.Find("GameLogo/BattleLogo").GetComponent<Text>().fontSize=100;
            root.Find("GameLogo/SolitaireLogo").GetComponent<Text>().fontSize=72;
            var portrait=FantasyUI.Panel("TitlePortrait",root,.25f,.44f,.75f,.715f,FantasyUI.Gold,true);
            FantasyUI.Portrait("Vesper",portrait.transform,BattlerId.Vesper,.016f,.015f,.984f,.985f);
            FantasyUI.Frame(portrait.transform,FantasyUI.Gold,true);
            FantasyUI.Label("Invitation",root,"Your next battle begins with a card.",34,FantasyUI.Silver,.08f,.375f,.92f,.435f,false,TextAnchor.MiddleCenter);
            view._play=FantasyUI.Button("PlayButton",root,"PLAY",.13f,.279f,.87f,.355f,FantasyUI.Gold,41);
            view._continue=FantasyUI.Button("TitleContinue",root,"CONTINUE BATTLE",.13f,.181f,.87f,.257f,FantasyUI.Blue,32);
            view._help=FantasyUI.Button("TitleHelp",root,"HOW TO PLAY",.13f,.083f,.485f,.159f,FantasyUI.Muted,27);
            view._settings=FantasyUI.Button("TitleSettings",root,"SETTINGS",.515f,.083f,.87f,.159f,FantasyUI.Muted,27);
            FantasyUI.Label("Version",root,"LOCAL BATTLES  /  v0.9.0",23,FantasyUI.Muted,.12f,.015f,.88f,.057f,true,TextAnchor.MiddleCenter);
            view._play.onClick.AddListener(controller.ShowFrontEnd);view._continue.onClick.AddListener(controller.ContinueSavedBattle);
            view._help.onClick.AddListener(controller.ShowTutorial);view._settings.onClick.AddListener(controller.OpenPause);
            root.gameObject.SetActive(false);return view;
        }
        public void Show()
        {
            gameObject.SetActive(true);transform.SetAsLastSibling();
            _continue.interactable=_controller.HasSavedMatch;
            _continue.GetComponentInChildren<Text>().text=_controller.HasSavedMatch?"CONTINUE BATTLE":"NO SAVED BATTLE";
            var controls=_controller.HasSavedMatch?new[]{_play,_continue,_help,_settings}:new[]{_play,_help,_settings};
            for(int i=0;i<controls.Length;i++) controls[i].navigation=new Navigation {
                mode=Navigation.Mode.Explicit,selectOnUp=controls[(i+controls.Length-1)%controls.Length],
                selectOnDown=controls[(i+1)%controls.Length],selectOnLeft=controls[(i+controls.Length-1)%controls.Length],selectOnRight=controls[(i+1)%controls.Length]
            };
            EventSystem.current?.SetSelectedGameObject((_controller.HasSavedMatch?_continue:_play).gameObject);
        }
        public void Hide()=>gameObject.SetActive(false);
    }
}
