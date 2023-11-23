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

    public UnityEvent OnTabSelected;
    public UnityEvent OnTabDeselected;


    private void Awake() => _background = GetComponent<Image>();
    


    public void OnPointerClick(PointerEventData eventData) => _tabGroup.OnTabSelected(this);
    public void OnPointerEnter(PointerEventData eventData) => _tabGroup.OnTabEnter(this);
    public void OnPointerExit(PointerEventData eventData) => _tabGroup.OnTabExit(this);


    public void SetBackground(Sprite newSprite)
    {
        if (_background != null)
            _background.sprite = newSprite;
    }
    public void SetBackgroundColour(Color newColour)
    {
        if (_background != null)
            _background.color = newColour;
    }


    public void Select() => OnTabSelected?.Invoke();
    public void Deselect() => OnTabDeselected?.Invoke();
}
