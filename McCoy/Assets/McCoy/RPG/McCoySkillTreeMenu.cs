using com.cygnusprojects.TalentTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Assets.McCoy.ProjectConstants;

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

    [SerializeField]
    TalentusEngine Engine = null;

    [SerializeField]
    TMP_Text availablePoints = null;

    Action<int, string, PlayerCharacter> applyCallback = null;

    private SkillTreeTab currentPage = SkillTreeTab.SkillTreeArbiter;
    PlayerCharacter player;
    private bool bound = false;

    private void Awake()
    {
      arbiterPage.SetActive(true);
      humanPage.SetActive(false);
      wolfPage.SetActive(false);
      tabs.GetComponentsInChildren<Toggle>().First().Select();
    }

    private void OnEnable()
    {
      bind();
    }

    private void OnDisable()
    {
      unbind();
    }

    private void bind()
    {
      if (bound || Engine == null)
      {
        return;
      }
      bound = true;
      Engine.TalentTree.TreeEvaluated += talentsUpdated;
    }
    private void unbind()
    {
      if (Engine == null)
      {
        return;
      }
      bound = false;
      Engine.TalentTree.TreeEvaluated -= talentsUpdated;
    }

    private void talentsUpdated(object sender, TalentTreeGraph.TreeEvaluatedEventArgs e)
    {
      availablePoints.text = $"{Engine.TalentTree.PointsToAssign}";
    }

    public void SetAvailableSkillPoints(int points)
    {
      Debug.Log(Engine.SaveToString());
      Engine.AvailableSkillPoints = points;
      Engine.TalentTree.PointsToAssign = points;
      Engine.Evaluate();
      availablePoints.text = $"{points}";
    }

    public void LoadSkills(string skillString, Action<int, string, PlayerCharacter> loadSkills)
    {
      applyCallback = loadSkills;
      Engine.LoadFromString(skillString);
    }

    public void Close()
    {
      applyCallback(Engine.AvailableSkillPoints, Engine.SaveToString(), player);
      Destroy(gameObject);
    }

    public void TabChanged()
    {
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
