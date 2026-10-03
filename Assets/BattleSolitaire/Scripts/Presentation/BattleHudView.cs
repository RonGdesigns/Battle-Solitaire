using BattleSolitaire.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleHudView : MonoBehaviour
    {
        private BattleGameController _controller;
        private RawImage _opponentPortrait,_playerPortrait;
        private Text _opponentIdentity,_opponentTitle,_opponentHealthText,_opponentShield,_opponentQuote;
        private Text _playerIdentity,_playerTitle,_playerHealthText,_playerShield,_playerQuote;
        private Text _energyText,_comboText,_message,_progressText,_tableCombo;
        private Slider _opponentHealth,_playerHealth,_energy,_combo,_opponentProgress;
        private Button _lockButton,_fogButton,_blockerButton;
        private GameObject _resultPanel;
        private Text _resultText;
        private float _targetOpponentHealth,_targetPlayerHealth,_targetEnergy,_targetCombo,_targetOpponentProgress;
        private BattlerId? _shownPlayer,_shownOpponent;
        public static BattleHudView Create(Transform parent,BattleGameController controller)
        {
            RectTransform root=FantasyUI.Rect("BattleHUD",parent,0,0,1,1);
            var view=root.gameObject.AddComponent<BattleHudView>();view._controller=controller;view.Build();view.Refresh();return view;
        }
        private void Update()
        {
            float step=4.8f*Time.unscaledDeltaTime;
            _opponentHealth.value=Mathf.MoveTowards(_opponentHealth.value,_targetOpponentHealth,step);
            _playerHealth.value=Mathf.MoveTowards(_playerHealth.value,_targetPlayerHealth,step);
            _energy.value=Mathf.MoveTowards(_energy.value,_targetEnergy,step);
            _combo.value=Mathf.MoveTowards(_combo.value,_targetCombo,step);
            _opponentProgress.value=Mathf.MoveTowards(_opponentProgress.value,_targetOpponentProgress,step);
        }
        public void Refresh()
        {
            if(_controller.Match==null)return;
            BattleParticipant player=_controller.Match.Player,opponent=_controller.Match.Opponent;
            BattlerDefinition p=_controller.PlayerBattler,o=_controller.OpponentBattler;
            if(_shownPlayer!=p.Id){GameArt.SetPortrait(_playerPortrait,p.Id);_shownPlayer=p.Id;}
            if(_shownOpponent!=o.Id){GameArt.SetPortrait(_opponentPortrait,o.Id);_shownOpponent=o.Id;}
            _opponentIdentity.text=o.Name;_opponentTitle.text=o.Title;_opponentQuote.text="“"+o.Quote+"”";
            _playerIdentity.text="You · "+p.Name;_playerTitle.text=p.Title;_playerQuote.text="“"+p.Quote+"”";
            _opponentHealthText.text=opponent.Health+" / "+BattleTuning.MaxHealth;
            _playerHealthText.text=player.Health+" / "+BattleTuning.MaxHealth;
            _opponentShield.text=opponent.Shield.ToString();_playerShield.text=player.Shield.ToString();
            _energyText.text=player.Energy+" / "+BattleTuning.MaxEnergy;
            _comboText.text=player.Combo.Count>0?"x"+player.Combo.Count:"READY";
            _tableCombo.text=player.Combo.Count>1?"COMBO x"+player.Combo.Count:"EVERY CARD MOVES THE BATTLE FORWARD.";
            _tableCombo.fontSize=player.Combo.Count>1?48:23;
            _progressText.text="RIVAL BOARD  "+Mathf.RoundToInt(opponent.ClearPercentage*100)+"%";
            _message.text=string.IsNullOrEmpty(_controller.VisibleMessage)?"BUILD YOUR COMBO. CONTROL THE BATTLE.":_controller.VisibleMessage;
            _targetOpponentHealth=opponent.Health/(float)BattleTuning.MaxHealth;
            _targetPlayerHealth=player.Health/(float)BattleTuning.MaxHealth;
            _targetEnergy=player.Energy/(float)BattleTuning.MaxEnergy;
            _targetCombo=player.Combo.Strength01;_targetOpponentProgress=opponent.ClearPercentage;
            bool running=_controller.Match.State==BattleMatchState.Running;
            _lockButton.interactable=running&&player.Energy>=BattleTuning.LockCost;
            _fogButton.interactable=running&&player.Energy>=BattleTuning.FogCost;
            _blockerButton.interactable=running&&player.Energy>=BattleTuning.BlockerCost;
            _resultPanel.SetActive(!running);
            if(!running){_resultText.text=_controller.Match.State==BattleMatchState.PlayerWon?"VICTORY":_controller.Match.State==BattleMatchState.OpponentWon?"DEFEAT":"DRAW";_resultPanel.transform.SetAsLastSibling();}
        }
        private void Build()
        {
            FantasyUI.Logo(transform,.025f,.852f,.36f,.997f);
            Image rival=FantasyUI.Panel("OpponentPanel",transform,.395f,.85f,.98f,.985f,new Color32(75,132,162,255));
            _opponentPortrait=FantasyUI.Portrait("OpponentPortrait",rival.transform,_controller.OpponentBattler.Id,.017f,.075f,.31f,.95f);
            _opponentIdentity=FantasyUI.Label("OpponentIdentity",rival.transform,"",36,FantasyUI.Silver,.35f,.70f,.95f,.97f);
            _opponentTitle=FantasyUI.Label("OpponentTitle",rival.transform,"",25,FantasyUI.Muted,.35f,.56f,.95f,.75f);
            FantasyUI.Label("RivalLabel",rival.transform,"RIVAL",19,FantasyUI.Gold,.79f,.8f,.95f,.97f,true,TextAnchor.MiddleRight);
            _opponentHealth=HealthBar("OpponentHP",rival.transform,.35f,.35f,.75f,.53f);
            _opponentHealthText=FantasyUI.Label("OpponentHPText",rival.transform,"",26,FantasyUI.Silver,.37f,.34f,.74f,.54f,false,TextAnchor.MiddleCenter);
            FantasyUI.Icon("Shield",rival.transform,FantasySymbol.Shield,FantasyUI.Blue,.79f,.35f,.86f,.55f);
            _opponentShield=FantasyUI.Label("ShieldValue",rival.transform,"",26,FantasyUI.Blue,.88f,.34f,.97f,.54f);
            _opponentQuote=FantasyUI.Label("OpponentQuote",rival.transform,"",24,FantasyUI.Muted,.35f,.05f,.97f,.30f);_opponentQuote.fontStyle=FontStyle.Italic;
            _progressText=FantasyUI.Label("RivalProgress",transform,"",19,FantasyUI.Muted,.43f,.827f,.70f,.849f,true);
            _opponentProgress=Bar("OpponentProgress",transform,FantasyUI.Blue,.72f,.833f,.96f,.842f);
            _tableCombo=FantasyUI.Label("TableCombo",transform,"",44,FantasyUI.Gold,.08f,.29f,.92f,.34f,true,TextAnchor.MiddleCenter);
            Image incoming=FantasyUI.Panel("BattleMessage",transform,.038f,.255f,.965f,.285f,new Color32(190,49,68,255));
            _message=FantasyUI.Label("Message",incoming.transform,"",25,FantasyUI.Silver,.04f,.05f,.96f,.95f,false,TextAnchor.MiddleCenter);
            Image player=FantasyUI.Panel("PlayerPanel",transform,.025f,.168f,.975f,.252f,new Color32(55,145,184,255));
            _playerPortrait=FantasyUI.Portrait("PlayerPortrait",player.transform,_controller.PlayerBattler.Id,.013f,.04f,.24f,.96f);
            _playerIdentity=FantasyUI.Label("PlayerIdentity",player.transform,"",34,FantasyUI.Silver,.27f,.62f,.67f,.99f);
            _playerTitle=FantasyUI.Label("PlayerTitle",player.transform,"",25,FantasyUI.Muted,.27f,.40f,.66f,.67f);
            _playerHealth=HealthBar("PlayerHP",player.transform,.27f,.12f,.52f,.36f);
            _playerHealthText=FantasyUI.Label("PlayerHPText",player.transform,"",23,FantasyUI.Silver,.28f,.1f,.51f,.36f,false,TextAnchor.MiddleCenter);
            FantasyUI.Icon("Shield",player.transform,FantasySymbol.Shield,FantasyUI.Blue,.55f,.1f,.60f,.36f);
            _playerShield=FantasyUI.Label("PlayerShield",player.transform,"",25,FantasyUI.Blue,.615f,.1f,.69f,.36f);
            _playerQuote=FantasyUI.Label("PlayerQuote",player.transform,"",25,FantasyUI.Muted,.70f,.15f,.96f,.90f,false,TextAnchor.MiddleCenter);_playerQuote.fontStyle=FontStyle.Italic;
            FantasyUI.Icon("EnergyIcon",transform,FantasySymbol.Energy,FantasyUI.Blue,.045f,.141f,.065f,.164f);
            FantasyUI.Label("EnergyLabel",transform,"ENERGY",18,FantasyUI.Blue,.08f,.140f,.22f,.163f,true);
            _energy=Bar("Energy",transform,FantasyUI.Blue,.22f,.147f,.42f,.158f);
            _energyText=FantasyUI.Label("EnergyValue",transform,"",24,FantasyUI.Blue,.44f,.139f,.55f,.165f);
            FantasyUI.Label("ComboLabel",transform,"COMBO",18,FantasyUI.Gold,.60f,.140f,.72f,.163f,true);
            _combo=Bar("Combo",transform,FantasyUI.Gold,.73f,.147f,.88f,.158f);
            _comboText=FantasyUI.Label("ComboText",transform,"",23,FantasyUI.Gold,.90f,.140f,.975f,.164f);
            _lockButton=Ability("LockButton","LOCK",BattleTuning.LockCost,FantasySymbol.Lock,.045f,"Stop a rival column.");
            _fogButton=Ability("FogButton","FOG",BattleTuning.FogCost,FantasySymbol.Fog,.356f,"Obscure rival cards.");
            _blockerButton=Ability("BlockerButton","BLOCK",BattleTuning.BlockerCost,FantasySymbol.Shield,.667f,"Block a rival column.");
            _lockButton.onClick.AddListener(()=>_controller.UsePlayerAttack(BattleAttackType.Lock));
            _fogButton.onClick.AddListener(()=>_controller.UsePlayerAttack(BattleAttackType.Fog));
            _blockerButton.onClick.AddListener(()=>_controller.UsePlayerAttack(BattleAttackType.Blocker));
            Button menu=FantasyUI.Button("MenuButton",transform,"LOADOUT",.035f,.006f,.30f,.042f,FantasyUI.Muted,21);menu.onClick.AddListener(_controller.ShowFrontEnd);
            Button help=FantasyUI.Button("HelpButton",transform,"HOW TO PLAY",.70f,.006f,.965f,.042f,FantasyUI.Muted,19);help.onClick.AddListener(_controller.ShowTutorial);
            FantasyUI.Label("BattleFooter",transform,"SKILL PLAYS. HIGHER STAKES.",17,FantasyUI.Muted,.31f,.007f,.69f,.04f,true,TextAnchor.MiddleCenter);
            BuildResult();
        }
        private Button Ability(string name,string label,int cost,FantasySymbol symbol,float x,string description)
        {
            Button button=FantasyUI.Button(name,transform,"",x,.075f,x+.287f,.133f,new Color32(44,161,211,255));
            FantasyUI.Icon("AbilityIcon",button.transform,symbol,FantasyUI.Blue,.055f,.17f,.25f,.83f);
            FantasyUI.Label("Ability",button.transform,label,27,FantasyUI.Silver,.32f,.47f,.96f,.94f,true);
            FantasyUI.Label("Cost",button.transform,cost+" ENERGY",23,FantasyUI.Blue,.32f,.1f,.96f,.5f);
            FantasyUI.Label("Description",transform,description,23,FantasyUI.Muted,x,.043f,x+.287f,.074f,false,TextAnchor.MiddleCenter);
            return button;
        }
        private static Slider HealthBar(string name,Transform parent,float x,float y,float xx,float yy)
            => Bar(name,parent,new Color32(192,34,54,255),x,y,xx,yy);
        private static Slider Bar(string name,Transform parent,Color color,float x,float y,float xx,float yy)
        {
            Slider slider=PrototypeUI.CreateBar(name,parent,color);FantasyUI.Box((RectTransform)slider.transform,x,y,xx,yy);
            slider.GetComponentInChildren<Image>().color=new Color32(3,9,15,255);
            FantasyUI.Frame(slider.transform,FantasyUI.Muted);return slider;
        }
        private void BuildResult()
        {
            Image result=FantasyUI.Panel("ResultPanel",transform,.10f,.40f,.90f,.65f,FantasyUI.Gold,true);_resultPanel=result.gameObject;
            // Keep the finished-match overlay above the table and fully interactive.
            result.raycastTarget=true;
            _resultText=FantasyUI.Label("ResultText",result.transform,"VICTORY",61,FantasyUI.Gold,.04f,.49f,.96f,.94f,true,TextAnchor.MiddleCenter);
            Button rematch=FantasyUI.Button("RematchButton",result.transform,"REMATCH",.07f,.14f,.47f,.39f,FantasyUI.Blue,29);rematch.onClick.AddListener(_controller.StartRematch);
            Button loadout=FantasyUI.Button("LoadoutButton",result.transform,"LOADOUT",.53f,.14f,.93f,.39f,FantasyUI.Gold,29);loadout.onClick.AddListener(_controller.ShowFrontEnd);
            _resultPanel.SetActive(false);
        }
    }
}
