using BattleSolitaire.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleFrontEndView : MonoBehaviour
    {
        private BattleGameController _controller;
        private readonly Button[] _battlerButtons = new Button[3];
        private readonly RawImage[] _portraits = new RawImage[3];
        private readonly Text[] _names = new Text[3], _titles = new Text[3], _quotes = new Text[3];
        private readonly GameObject[] _details = new GameObject[3], _markers = new GameObject[3];
        private readonly Image[] _namePlates = new Image[3];
        private readonly Button[] _difficultyButtons = new Button[3];
        private readonly GameObject[] _difficultyMarkers = new GameObject[3];
        private readonly Text[] _statValues = new Text[4];
        private Text _activeName, _activeTitle, _record, _mastery;
        private Button _battleButton, _helpButton;
        public bool IsOpen => gameObject.activeSelf;

        public static BattleFrontEndView Create(Transform parent, BattleGameController controller)
        {
            RectTransform root = FantasyUI.Rect("BattleFrontEnd",parent,0,0,1,1);
            root.gameObject.AddComponent<Image>().color=PrototypeUI.Background;
            var view=root.gameObject.AddComponent<BattleFrontEndView>();view._controller=controller;
            FantasyUI.Backdrop(root);
            view.BuildHeader();view.BuildBattlers();view.BuildLoadout();view.BuildCareer();view.ConfigureNavigation();
            view.Refresh();root.gameObject.SetActive(false);return view;
        }
        public void Show()
        {
            gameObject.SetActive(true);transform.SetAsLastSibling();Refresh();
            if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(_battleButton.gameObject);
        }
        public void Hide()=>gameObject.SetActive(false);
        public void Refresh()
        {
            BattlerDefinition active=_controller.PlayerBattler;if(active==null)return;
            _activeName.text=active.Name;_activeTitle.text=active.Title;
            int secondary=0;
            for(int i=0;i<3;i++)
            {
                bool selected=i==(int)active.Id;
                float x0=selected?.055f:.50f+secondary*.235f;
                float x1=selected?.465f:x0+.21f;
                FantasyUI.Box((RectTransform)_battlerButtons[i].transform,x0,selected?.476f:.535f,x1,selected?.786f:.758f);
                if(!selected)secondary++;
                FantasyUI.Box(_portraits[i].rectTransform,.025f,selected?.30f:.17f,.975f,.982f);
                FantasyUI.Box(_namePlates[i].rectTransform,.022f,.013f,.978f,selected?.395f:.265f);
                FantasyUI.Box(_names[i].rectTransform,.05f,selected?.29f:.125f,.95f,selected?.393f:.265f);
                _names[i].fontSize=selected?43:32;
                FantasyUI.Box(_titles[i].rectTransform,.05f,selected?.238f:.015f,.95f,selected?.311f:.135f);
                _titles[i].fontSize=selected?28:24;
                _details[i].SetActive(selected);_markers[i].SetActive(selected);
                FantasyFrame frame=_battlerButtons[i].GetComponentInChildren<FantasyFrame>();
                frame.Edge=selected?FantasyUI.Gold:new Color32(161,137,100,255);frame.Thickness=selected?5:3;frame.SetVerticesDirty();
                _quotes[i].gameObject.SetActive(!selected);
                if(!selected)FantasyUI.Box(_quotes[i].rectTransform,x0,.482f,x1,.531f);
            }
            BattleProfile p=_controller.Profile;
            for(int i=0;i<3;i++)
            {
                bool selected=i==(int)p.Difficulty;
                _difficultyButtons[i].GetComponent<Image>().color=selected?new Color32(12,50,69,255):FantasyUI.Dark;
                _difficultyMarkers[i].SetActive(selected);
                FantasyFrame frame=_difficultyButtons[i].GetComponentInChildren<FantasyFrame>();frame.Edge=selected?FantasyUI.Blue:new Color32(95,119,139,255);frame.SetVerticesDirty();
            }
            _statValues[0].text=p.Wins.ToString();_statValues[1].text="x"+p.LongestCombo;
            _statValues[2].text=p.PerfectClears.ToString();_statValues[3].text=p.GetMasteryLevel(active.Id).ToString();
            _record.text=p.GetRankName()+" • "+p.RankPoints+" RP";
            _mastery.text="MASTERY LEVEL "+p.GetMasteryLevel(active.Id);
        }
        private void BuildHeader()
        {
            FantasyUI.Logo(transform,.043f,.853f,.485f,.99f);
            Image panel=FantasyUI.Panel("ActiveSummary",transform,.548f,.897f,.964f,.98f,new Color32(86,130,161,255));
            _activeName=FantasyUI.Label("ActiveName",panel.transform,"",39,FantasyUI.Silver,.055f,.46f,.92f,.96f);
            _activeTitle=FantasyUI.Label("ActiveTitle",panel.transform,"",28,FantasyUI.Muted,.055f,.13f,.95f,.48f);
            FantasyUI.Label("DifficultyLabel",transform,"RIVAL AI",22,FantasyUI.Gold,.55f,.875f,.94f,.895f,true);
            string[] labels={"CASUAL","STANDARD","EXPERT"};float[] left={.545f,.675f,.833f};float[] right={.663f,.821f,.953f};
            for(int i=0;i<3;i++)
            {
                int option=i;
                Button b=FantasyUI.Button("Difficulty_"+labels[i],transform,labels[i],left[i],.831f,right[i],.875f,FantasyUI.Blue,18);
                b.onClick.AddListener(()=>_controller.SelectDifficulty((BattleDifficulty)option));_difficultyButtons[i]=b;
                Image marker=PrototypeUI.CreatePanel("SelectedDifficulty",b.transform,new Vector2(.16f,.12f),new Vector2(.84f,.15f),Vector2.zero,Vector2.zero,FantasyUI.Blue);
                marker.raycastTarget=false;_difficultyMarkers[i]=marker.gameObject;
            }
        }
        private void BuildBattlers()
        {
            FantasyUI.Panel("BattlerGallery",transform,.025f,.459f,.975f,.823f,FantasyUI.Gold,true);
            FantasyUI.Label("ChooseLabel",transform,"CHOOSE YOUR BATTLER",28,FantasyUI.Gold,.066f,.788f,.72f,.819f,true);
            FantasyUI.Label("RosterCount",transform,"3 BATTLERS",20,FantasyUI.Muted,.72f,.788f,.94f,.819f,true,TextAnchor.MiddleRight);
            for(int i=0;i<3;i++)
            {
                BattlerDefinition b=BattlerCatalog.GetByIndex(i);
                Button button=FantasyUI.Button("Battler_"+b.Name,transform,"",.05f,.49f,.46f,.79f,FantasyUI.Gold);
                _battlerButtons[i]=button;button.onClick.AddListener(()=>_controller.SelectBattler(b.Id));
                FantasyUI.Label("PortraitFallback",button.transform,"PORTRAIT UNAVAILABLE",20,FantasyUI.Muted,.1f,.45f,.9f,.75f,false,TextAnchor.MiddleCenter);
                _portraits[i]=FantasyUI.Portrait("Portrait",button.transform,b.Id,.025f,.3f,.975f,.98f);
                Image namePlate=PrototypeUI.CreatePanel("NamePlate",button.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,new Color32(4,11,19,246));
                namePlate.raycastTarget=false;_namePlates[i]=namePlate;
                _names[i]=FantasyUI.Label("Name",button.transform,b.Name,43,FantasyUI.Silver,.05f,.29f,.95f,.39f,false,TextAnchor.MiddleCenter);
                _titles[i]=FantasyUI.Label("Title",button.transform,b.Title,28,FantasyUI.Muted,.05f,.23f,.95f,.30f,false,TextAnchor.MiddleCenter);
                RectTransform details=FantasyUI.Rect("ActiveDetails",button.transform,0,0,1,1);_details[i]=details.gameObject;
                Text quote=FantasyUI.Label("ActiveQuote",details,"“"+b.Quote+"”",24,FantasyUI.Silver,.06f,.135f,.94f,.248f,false,TextAnchor.MiddleCenter);quote.fontStyle=FontStyle.Italic;
                string[] passive=b.TraitLine.Split('•');
                FantasyUI.Label("PassiveName",details,passive[0].Trim(),20,FantasyUI.Gold,.05f,.078f,.95f,.158f,true,TextAnchor.MiddleCenter);
                FantasyUI.Label("PassiveDescription",details,passive[1].Trim(),22,FantasyUI.Muted,.06f,.007f,.94f,.094f,false,TextAnchor.MiddleCenter);
                Image marker=PrototypeUI.CreatePanel("ActiveMarker",button.transform,new Vector2(.04f,.915f),new Vector2(.37f,.97f),Vector2.zero,Vector2.zero,FantasyUI.Gold);
                marker.raycastTarget=false;FantasyUI.Label("Active",marker.transform,"ACTIVE",21,PrototypeUI.TextDark,0,0,1,1,true,TextAnchor.MiddleCenter);_markers[i]=marker.gameObject;
                FantasyFrame frame=button.GetComponentInChildren<FantasyFrame>();frame.Ornate=true;frame.transform.SetAsLastSibling();
                _quotes[i]=FantasyUI.Label("BattlerQuote",transform,"“"+b.Quote+"”",25,FantasyUI.Muted,0,0,1,1,false,TextAnchor.MiddleCenter);_quotes[i].fontStyle=FontStyle.Italic;
            }
        }
        private void BuildLoadout()
        {
            FantasyUI.Panel("Loadout",transform,.029f,.326f,.971f,.452f,new Color32(109,138,155,255));
            FantasyUI.Label("LoadoutLabel",transform,"BATTLE LOADOUT",27,FantasyUI.Gold,.065f,.421f,.70f,.449f,true);
            string[] names={"LOCK","FOG","BLOCK"};string[] costs={"25 ENERGY","25 ENERGY","50 ENERGY"};
            string[] info={"Stop a rival column.","Obscure rival cards.","Block a rival column."};
            FantasySymbol[] symbols={FantasySymbol.Lock,FantasySymbol.Fog,FantasySymbol.Shield};
            for(int i=0;i<3;i++)
            {
                float x=.052f+i*.306f;
                Image tile=FantasyUI.Panel("Loadout_"+names[i],transform,x,.362f,x+.285f,.417f,new Color32(43,160,211,255));
                FantasyUI.Icon("AbilityIcon",tile.transform,symbols[i],FantasyUI.Blue,.065f,.20f,.27f,.79f);
                FantasyUI.Label("Ability",tile.transform,names[i],27,FantasyUI.Silver,.32f,.43f,.95f,.88f,true);
                FantasyUI.Label("Cost",tile.transform,costs[i],22,FantasyUI.Blue,.32f,.1f,.95f,.46f);
                FantasyUI.Label("AbilityDescription",transform,info[i],24,FantasyUI.Muted,x,.331f,x+.285f,.359f,false,TextAnchor.MiddleCenter);
            }
        }
        private void BuildCareer()
        {
            Image deck=FantasyUI.Panel("DeckPreview",transform,.029f,.163f,.505f,.318f,new Color32(109,138,155,255));
            FantasyUI.Label("DeckLabel",deck.transform,"BATTLE DECK",25,FantasyUI.Gold,.06f,.81f,.95f,.98f,true);
            RawImage back=FantasyUI.Art("DeckArt",deck.transform,GameArt.GetCardBack(),.10f,.08f,.41f,.78f);
            back.gameObject.AddComponent<AspectFillRawImage>().SetTexture(back.texture);
            FantasyUI.Label("DeckMotto",deck.transform,"SAME CARDS.\nHIGHER STAKES.",25,FantasyUI.Gold,.46f,.40f,.94f,.76f,true,TextAnchor.MiddleCenter);
            _mastery=FantasyUI.Label("Mastery",deck.transform,"",20,FantasyUI.Muted,.46f,.16f,.94f,.37f,true,TextAnchor.MiddleCenter);
            Image career=FantasyUI.Panel("Career",transform,.524f,.163f,.971f,.318f,new Color32(109,138,155,255));
            FantasyUI.Label("CareerLabel",career.transform,"CAREER STATS",25,FantasyUI.Gold,.06f,.81f,.95f,.98f,true);
            string[] labels={"WINS","LONGEST COMBO","PERFECT CLEARS","MASTERY LV"};
            for(int i=0;i<4;i++)
            {
                float y=.63f-i*.167f;
                FantasyUI.Label("StatLabel",career.transform,labels[i],19,FantasyUI.Muted,.08f,y,.77f,y+.16f,true);
                _statValues[i]=FantasyUI.Label("StatValue",career.transform,"",30,FantasyUI.Silver,.78f,y,.94f,y+.16f,false,TextAnchor.MiddleRight);
            }
            Image rank=FantasyUI.Panel("Rank",transform,.029f,.066f,.505f,.151f,new Color32(65,143,177,255),true);
            FantasyUI.Icon("RankCrest",rank.transform,FantasySymbol.Spade,FantasyUI.Gold,.06f,.14f,.23f,.86f);
            _record=FantasyUI.Label("Record",rank.transform,"",26,FantasyUI.Gold,.29f,.42f,.95f,.86f,true);
            FantasyUI.Label("RankCaption",rank.transform,"SOLVE. GROW. RISE.",21,FantasyUI.Muted,.29f,.13f,.95f,.45f,true);
            _battleButton=FantasyUI.Button("BattleButton",transform,"BATTLE",.539f,.081f,.966f,.150f,PrototypeUI.Danger,46);
            _battleButton.GetComponent<Image>().color=new Color32(75,8,20,255);
            _battleButton.GetComponentInChildren<Text>().rectTransform.anchorMin=new Vector2(.27f,.05f);
            FantasyUI.Icon("CrossedSwords",_battleButton.transform,FantasySymbol.Swords,new Color32(255,130,135,255),.07f,.22f,.24f,.8f);
            FantasyFrame border=_battleButton.GetComponentInChildren<FantasyFrame>();border.Thickness=4;border.Ornate=true;border.Glow=true;
            _battleButton.gameObject.AddComponent<MenuGradient>();
            _battleButton.onClick.AddListener(_controller.StartBattleFromMenu);
            FantasyUI.Label("BattleTagline",transform,"SOLVE. STRIKE. ASCEND.",21,FantasyUI.Muted,.53f,.054f,.97f,.08f,true,TextAnchor.MiddleCenter);
            _helpButton=FantasyUI.Button("TutorialButton",transform,"HOW TO PLAY",.30f,.012f,.70f,.056f,new Color32(82,121,148,255),21);
            _helpButton.onClick.AddListener(_controller.ShowTutorial);
        }
        private void ConfigureNavigation()
        {
            for(int i=0;i<3;i++)
            {
                _difficultyButtons[i].navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnLeft=_difficultyButtons[(i+2)%3],selectOnRight=_difficultyButtons[(i+1)%3],selectOnDown=_battlerButtons[i],selectOnUp=_helpButton};
                _battlerButtons[i].navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnLeft=_battlerButtons[(i+2)%3],selectOnRight=_battlerButtons[(i+1)%3],selectOnUp=_difficultyButtons[i],selectOnDown=_battleButton};
            }
            _battleButton.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=_battlerButtons[1],selectOnDown=_helpButton};
            _helpButton.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=_battleButton,selectOnDown=_difficultyButtons[1]};
        }
    }
}
