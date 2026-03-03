using UnityEngine;
using UnityEngine.EventSystems;

public class HoverToSelect : MonoBehaviour, IPointerEnterHandler
{
    //Mouse-/Gamepad-Cursor selects objects automatic, once it hovers over them.
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Ignore same Object.
        if (EventSystem.current.currentSelectedGameObject == gameObject) return;
        Debug.Log("Select should be triggered.");
        //Inform EventSystem about the selected Object.
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}