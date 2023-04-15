using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour {
    [SerializeField] private List<Transform> pages;
    [SerializeField] private TabItem defaultPage = null;
    [SerializeField] private bool rememberCurrentPage = true;

    [SerializeField] private Color idleColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color activeColor;

    private List<TabItem> tabItems = new List<TabItem>();
    private TabItem selectedTab = null;

    private TabItem tabToLoadOnStart = null;

    private void OnEnable() {
        if ((selectedTab == null || !rememberCurrentPage) && defaultPage != null)
            tabToLoadOnStart = defaultPage;
        else
            tabToLoadOnStart = null;
    }

    private void Start() {
        if (tabToLoadOnStart != null) {
            OnTabSelected(tabToLoadOnStart);
        }
    }

    public void Subscribe(TabItem tabItem) {
        tabItems.Add(tabItem);
    }

    public void OnTabEnter(TabItem tabItem) {
        ResetTabs();
        if (tabItem != selectedTab) {
            tabItem.SetColor(hoverColor);
        }
    }

    public void OnTabExit(TabItem tabItem) {
        ResetTabs();
    }

    public void OnTabSelected(int tabIndex) => OnTabSelected(transform.GetChild(tabIndex).GetComponent<TabItem>());
    public void OnTabSelected(TabItem tabItem) {
        if (selectedTab != null) {
            selectedTab.Deselect();
        }

        selectedTab = tabItem;
        selectedTab.Select();

        ResetTabs();
        tabItem.SetColor(activeColor);

        int index = tabItem.transform.GetSiblingIndex();
        for (int i = 0; i < pages.Count; i++) {
            pages[i].gameObject.SetActive(i == index);
        }
    }

    public void ResetTabs() {
        foreach (TabItem tabItem in tabItems) {
            if (selectedTab == tabItem) continue;
            tabItem.SetColor(idleColor);
        }
    }
}
