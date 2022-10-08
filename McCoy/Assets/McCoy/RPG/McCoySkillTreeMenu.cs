using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.RPG
{
  public class McCoySkillTreeMenu : MonoBehaviour
  {
    public enum SkillTreeTab
    {
      SkillTreeArbiter,
      SkillTreeHuman,
      SkillTreeWolf
    }

    [SerializeField]
    GameObject arbiterPage = null;

    [SerializeField]
    GameObject humanPage = null;

    [SerializeField]
    GameObject wolfPage = null;

    [SerializeField]
    ToggleGroup tabs = null;

    private SkillTreeTab currentPage = SkillTreeTab.SkillTreeArbiter;

    private void Awake()
    {
      arbiterPage.SetActive(true);
      humanPage.SetActive(false);
      wolfPage.SetActive(false);
      tabs.GetComponentsInChildren<Toggle>().First().Select();
    }

    public void Close()
    {
      Destroy(gameObject);
    }

    public void TabChanged()
    {
      Debug.Log("tab changed");
      foreach(Toggle t in tabs.ActiveToggles())
      {
        if(t.isOn)
        {
          SwitchPage(t.GetComponent<McCoySkillTreePageToggle>().Tab);
        }
      }
    }

    public void SwitchPage(SkillTreeTab page)
    {
      if(currentPage == page)
      {
        return;
      }
      getTabPage(currentPage).SetActive(false);
      currentPage = page;
      getTabPage(currentPage).SetActive(true);
    }

    private GameObject getTabPage(SkillTreeTab tab)
    {
      Debug.Log("switch to " + tab);
      switch(tab)
      {
        case SkillTreeTab.SkillTreeArbiter:
          return arbiterPage;
        case SkillTreeTab.SkillTreeHuman:
          return humanPage;
        case SkillTreeTab.SkillTreeWolf:
          return wolfPage;
      }
      return null;
    }
  }
}
