using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TabGroupButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TabGroup _tabGroup;
    private Image _background;

    public UnityEvent OnTabSelected; // An event for when this tab has been selected.
    public UnityEvent OnTabDeselected; // An event for when this tab has been deselected.


    private void Awake() => _background = GetComponent<Image>();
    


    public void OnPointerClick(PointerEventData eventData) => _tabGroup.OnTabSelected(this); // If this tab is clicked, notify the connected TabGroup.
    public void OnPointerEnter(PointerEventData eventData) => _tabGroup.OnTabEnter(this); // If the mouse enters this tab, notify the connected TabGroup.
    public void OnPointerExit(PointerEventData eventData) => _tabGroup.OnTabExit(this); // If the mouse leaves this tab, notify the connected TabGroup.


    // Set the background sprite of this TabButton.
    public void SetBackground(Sprite newSprite)
    {
        if (_background != null)
            _background.sprite = newSprite;
    }
    // Set the background colour of this TabButton.
    public void SetBackgroundColour(Color newColour)
    {
        if (_background != null)
            _background.color = newColour;
    }


    public void Select() => OnTabSelected?.Invoke(); // When selected, invoke the OnTabSelected event.
    public void Deselect() => OnTabDeselected?.Invoke(); // When deselected, invoke the OnTabDeselected event.
}
