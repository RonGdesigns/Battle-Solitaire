using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleSolitaire.Presentation
{
    /// <summary>Visible focus and hover on the metal frame without layout movement.</summary>
    public sealed class FantasyButtonFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _selected, _hovered;
        public void OnSelect(BaseEventData data) { _selected=true; Refresh(); }
        public void OnDeselect(BaseEventData data) { _selected=false; Refresh(); }
        public void OnPointerEnter(PointerEventData data) { _hovered=true; Refresh(); }
        public void OnPointerExit(PointerEventData data) { _hovered=false; Refresh(); }
        private void OnDisable() { _hovered=false; _selected=false; Refresh(); }
        private void Refresh()
        {
            FantasyFrame frame=GetComponentInChildren<FantasyFrame>();
            if(frame==null)return;
            frame.Focused=_selected;frame.Hovered=_hovered;frame.SetVerticesDirty();
        }
    }
}
