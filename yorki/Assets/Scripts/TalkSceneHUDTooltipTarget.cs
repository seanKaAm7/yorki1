using UnityEngine;
using UnityEngine.EventSystems;

public class TalkSceneHUDTooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public TalkSceneHUDController hud;
    public TalkSceneHUDStatKind statKind;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hud != null && !TalkSceneMenuController.IsAnyMenuOpen)
            hud.ShowStatTooltip(statKind, eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (hud != null)
            hud.MoveStatTooltip(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hud != null)
            hud.HideStatTooltip(statKind);
    }
}
