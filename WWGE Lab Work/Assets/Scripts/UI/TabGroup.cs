using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class TabGroup : MonoBehaviour
{
    [SerializeField] private List<TabGroupButton> _tabButtons = new List<TabGroupButton>();
    private int _selectedTabIndex = -1;

    [System.Serializable]
    private struct ContainerInfo
    {
        public GameObject Container;
        public GameObject FirstSelectedObject;
    }
    [SerializeField] private List<ContainerInfo> _tabContainers = new List<ContainerInfo>();

    // Colour (Color Fields cannot be Serialzed, apparently).
    [SerializeField] private Color idleColour = Color.white;
    [SerializeField] private Color hoverColour = Color.white;
    [SerializeField] private Color selectedColour = Color.white;

    private void OnEnable()
    {
        // If we have more than 1 tab button, select the first tab button.
        if (_tabButtons.Count > 0)
            OnTabSelected(_tabButtons[0]);
    }


    public void OnSelectNextTabPressed(InputAction.CallbackContext context)
    {
        // Only switch tab if the button was just pressed and we are currently active.
        if (context.performed && gameObject.activeInHierarchy)
            OnTabSelected(_tabButtons[_selectedTabIndex + 1]);
    }
    public void OnSelectPreviousTabPressed(InputAction.CallbackContext context)
    {
        // Only switch tab if the button was just pressed and we are currently active.
        if (context.performed && gameObject.activeInHierarchy)
            OnTabSelected(_tabButtons[_selectedTabIndex - 1]);
    }


    public void OnTabEnter(TabGroupButton button)
    {
        // Reset the background of all non-selected tabs.
        ResetTabs();

        // If this button is not the selected button, change its colour to the hover colour.
        if (_tabButtons.IndexOf(button) != _selectedTabIndex)
            button.SetBackgroundColour(hoverColour);
    }
    public void OnTabExit(TabGroupButton button) => ResetTabs(); // Reset the background of all non-selected tabs.
    public void OnTabSelected(TabGroupButton button)
    {
        // Deselect the old tab (If there was one).
        if (_selectedTabIndex != -1 && _tabButtons[_selectedTabIndex] != null)
            _tabButtons[_selectedTabIndex].Deselect();

        // Select the new tab.
        _selectedTabIndex = _tabButtons.IndexOf(button);
        _tabButtons[_selectedTabIndex].Select();

        // Set Apperance.
        ResetTabs();
        button.SetBackgroundColour(selectedColour);


        // Activate the corresponding container GameObject.
        for (int i = 0; i < _tabContainers.Count; i++)
        {
            if (i == _selectedTabIndex)
            {
                _tabContainers[i].Container.SetActive(true);
                
                // Select the first gameobject (Allows for controller navigation).
                EventSystem.current.SetSelectedGameObject(_tabContainers[i].FirstSelectedObject);
            } else {
                _tabContainers[i].Container.SetActive(false);
            }
        }
    }


    // Reset the background of all non-selected tabs.
    private void ResetTabs()
    {
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            // If this tab is the currently selected tab, then continue.
            if (_selectedTabIndex != -1 && i == _selectedTabIndex)
                continue;

            // Otherwise set to idle colour.
            _tabButtons[i].SetBackgroundColour(idleColour);
        }
    }
}