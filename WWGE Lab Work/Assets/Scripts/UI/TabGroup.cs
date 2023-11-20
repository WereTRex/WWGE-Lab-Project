using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TabGroup : MonoBehaviour
{
    private List<TabGroupButton> _tabButtons;
    private TabGroupButton _selectedTab;

    [SerializeField] private List<GameObject> _tabContainers;

    // Colour (Color Fields cannot be Serialzed, apparently).
    [SerializeField] private Color idleColour = Color.white;
    [SerializeField] private Color hoverColour = Color.white;
    [SerializeField] private Color selectedColour = Color.white;

    public void Subscribe(TabGroupButton button)
    {
        if (_tabButtons == null)
            _tabButtons = new List<TabGroupButton>();

        _tabButtons.Add(button);
    }

    private void OnEnable()
    {
        if (_tabButtons != null && _tabButtons.Count > 0)
            OnTabSelected(_tabButtons[0].transform.parent.GetChild(0).GetComponent<TabGroupButton>());
    }


    public void OnTabEnter(TabGroupButton button)
    {
        ResetTabs();

        if (_selectedTab == null || button != _selectedTab)
            button.SetBackgroundColour(hoverColour);
    }
    public void OnTabExit(TabGroupButton button) => ResetTabs();
    public void OnTabSelected(TabGroupButton button)
    {
        // Deselect the old tab.
        if (_selectedTab != null)
            _selectedTab.Deselect();

        // Select the new tab.
        _selectedTab = button;
        _selectedTab.Select();

        // Apperance.
        ResetTabs();
        button.SetBackgroundColour(selectedColour);

        // Activate the corresponding container GameObject.
        int buttonIndex = button.transform.GetSiblingIndex();
        for (int i = 0; i < _tabContainers.Count; i++)
        {
            _tabContainers[i].SetActive(i == buttonIndex);
        }
    }


    private void ResetTabs()
    {
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            if (_selectedTab != null && _tabButtons[i] == _selectedTab)
                continue;

            _tabButtons[i].SetBackgroundColour(idleColour);
        }
    }
}