using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class PauseOverlay : MonoBehaviour
    {
        private BattleGameController _controller;
        private Text _title, _description, _saveStatus;
        private Button _resume, _sound, _haptics, _motion, _loadout, _newBattle, _quit;
        private GameObject _previousFocus;
        private bool _confirm, _quitting;
        private RectTransform _panel;
        public bool IsOpen => gameObject.activeSelf;
        public static PauseOverlay Create(Transform parent,BattleGameController controller)
        {
            var root=FantasyUI.Rect("PauseOverlay",parent,0,0,1,1);
            root.gameObject.AddComponent<Image>().color=new Color32(3,9,18,245);
            var view=root.gameObject.AddComponent<PauseOverlay>();
            view._controller=controller; view.Build(); root.gameObject.SetActive(false); return view;
        }
        public void Open()
        {
            if(!IsOpen) _previousFocus=EventSystem.current?.currentSelectedGameObject;
            _confirm=false; _quitting=false; gameObject.SetActive(true); transform.SetAsLastSibling(); Refresh();
            EventSystem.current?.SetSelectedGameObject(_resume.gameObject);
        }
        public void Close()
        {
            gameObject.SetActive(false);
            if(_previousFocus!=null && _previousFocus.activeInHierarchy) EventSystem.current?.SetSelectedGameObject(_previousFocus);
        }
        public void ConfirmNewBattle() { _quitting=false; _confirm=true; Refresh(); EventSystem.current?.SetSelectedGameObject(_resume.gameObject); }
        public void ConfirmQuitBattle() { _quitting=true; _confirm=true; Refresh(); EventSystem.current?.SetSelectedGameObject(_resume.gameObject); }
        public void Back() { if(_confirm){_confirm=false;_quitting=false;Refresh();EventSystem.current?.SetSelectedGameObject(_resume.gameObject);} else _controller.ResumeBattle(); }
        private static void Label(Button button,string text) { button.GetComponentInChildren<Text>().text=text; }
        private void Refresh()
        {
            FantasyUI.Box(_panel,.055f,_confirm?.30f:.09f,.945f,_confirm?.78f:.91f);
            FantasyUI.Box((RectTransform)_resume.transform,.08f,_confirm?.32f:.67f,.92f,_confirm?.475f:.75f);
            FantasyUI.Box((RectTransform)_newBattle.transform,.08f,_confirm?.12f:.17f,.92f,_confirm?.265f:.25f);
            FantasyUI.Box(_description.rectTransform,.07f,_confirm?.50f:.765f,.93f,_confirm?.82f:.865f);
            _title.text=_confirm?(_quitting?"QUIT THIS BATTLE?":"START A NEW BATTLE?"):_controller.MenuOpen?"SETTINGS":"BATTLE PAUSED";
            _description.text=_confirm?(_quitting?"End this match and return to the title screen? This saved battle will be removed. Your career stays unchanged.":"Your unfinished battle will be replaced. Your career progress stays safe."):
                _controller.MenuOpen?"Make the game comfortable for you.":"Both boards and battle timers are paused. Resume when you are ready.";
            _saveStatus.text=_controller.RecoveryNotice;
            Label(_resume,_confirm?"KEEP CURRENT BATTLE":_controller.MenuOpen?"BACK":"RESUME BATTLE");
            Label(_sound,"SOUND  ·  "+(_controller.SoundEnabled?"ON":"OFF"));
            Label(_haptics,"VIBRATION  ·  "+(_controller.HapticsEnabled?"ON":"OFF"));
            Label(_motion,"REDUCED MOTION  ·  "+(AccessibilitySettings.ReducedMotion?"ON":"OFF"));
            Label(_newBattle,_confirm?(_quitting?"QUIT BATTLE":"REPLACE AND START"):"NEW BATTLE");
            Label(_loadout,_controller.MenuOpen?"TITLE SCREEN":"SAVE & LOADOUT");
            _sound.gameObject.SetActive(!_confirm); _haptics.gameObject.SetActive(!_confirm); _motion.gameObject.SetActive(!_confirm);
            _loadout.gameObject.SetActive(!_confirm && !_controller.TitleOpen);
            _quit.gameObject.SetActive(!_confirm && !_controller.MenuOpen);
            if(!_confirm) _newBattle.gameObject.SetActive(!_controller.MenuOpen);else _newBattle.gameObject.SetActive(true);
            var controls=new System.Collections.Generic.List<Button>();
            foreach(var button in new[]{_resume,_sound,_haptics,_motion,_loadout,_newBattle,_quit})
                if(button.gameObject.activeSelf) controls.Add(button);
            for(int i=0;i<controls.Count;i++)
                controls[i].navigation=new Navigation {
                    mode=Navigation.Mode.Explicit,
                    selectOnUp=controls[(i+controls.Count-1)%controls.Count],
                    selectOnDown=controls[(i+1)%controls.Count],
                    selectOnLeft=controls[i],selectOnRight=controls[i]
                };
        }
        private void Build()
        {
            var panel=FantasyUI.Panel("PausePanel",transform,.055f,.09f,.945f,.91f,FantasyUI.Gold,true).transform;
            _panel=(RectTransform)panel;
            _title=FantasyUI.Label("Title",panel,"",43,FantasyUI.Gold,.065f,.87f,.935f,.96f,true,TextAnchor.MiddleCenter);
            _description=FantasyUI.Label("Description",panel,"",38,FantasyUI.Silver,.07f,.72f,.93f,.865f,false,TextAnchor.MiddleCenter);
            _resume=FantasyUI.Button("ResumeButton",panel,"",.08f,.62f,.92f,.715f,FantasyUI.Blue,36);
            _sound=FantasyUI.Button("SoundButton",panel,"",.08f,.57f,.92f,.65f,FantasyUI.Muted,33);
            _haptics=FantasyUI.Button("HapticsButton",panel,"",.08f,.47f,.92f,.55f,FantasyUI.Muted,33);
            _motion=FantasyUI.Button("MotionButton",panel,"",.08f,.37f,.92f,.45f,FantasyUI.Muted,31);
            _loadout=FantasyUI.Button("SaveLoadoutButton",panel,"SAVE & LOADOUT",.08f,.27f,.92f,.35f,FantasyUI.Gold,32);
            _newBattle=FantasyUI.Button("NewBattleButton",panel,"NEW BATTLE",.08f,.02f,.92f,.115f,PrototypeUI.Danger,32);
            _quit=FantasyUI.Button("QuitBattleButton",panel,"QUIT BATTLE",.08f,.07f,.92f,.15f,PrototypeUI.Danger,32);
            _quit.onClick.AddListener(ConfirmQuitBattle);
            _saveStatus=FantasyUI.Label("SaveStatus",transform,"",30,FantasyUI.Muted,.08f,.012f,.92f,.084f,false,TextAnchor.MiddleCenter);
            _resume.onClick.AddListener(Back);
            _sound.onClick.AddListener(()=>{_controller.ToggleSound();Refresh();});
            _haptics.onClick.AddListener(()=>{_controller.ToggleHaptics();Refresh();});
            _motion.onClick.AddListener(()=>{AccessibilitySettings.SetReducedMotion(!AccessibilitySettings.ReducedMotion);Refresh();});
            _loadout.onClick.AddListener(()=>{if(_controller.MenuOpen)_controller.ShowTitleScreen();else _controller.ShowFrontEnd();});
            _newBattle.onClick.AddListener(()=>{if(_confirm){if(_quitting)_controller.QuitCurrentBattle();else _controller.StartBattleFromMenu();}else ConfirmNewBattle();});
        }
    }
}
