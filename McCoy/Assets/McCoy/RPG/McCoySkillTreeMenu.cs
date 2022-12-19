using Assets.McCoy.UI;
using com.cygnusprojects.TalentTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UFE3D;
using UnityEngine;
using UnityEngine.UI;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.RPG
{
  public class McCoySkillTreeMenu : MonoBehaviour, IMcCoyInputManager
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

    [SerializeField]
    Button buyButton = null;

    [SerializeField]
    Button cancelButton = null;

    Action<int, string, PlayerCharacter> applyCallback = null;

    private SkillTreeTab currentPage = SkillTreeTab.SkillTreeArbiter;
    PlayerCharacter player;
    private bool bound = false;

    // skills on the currently active page
    McCoyTalentusSkill[] currentSkills = null;
    private bool inputInitialized;
    private McCoyInputManager input;
    private McCoyTalentusSkill selectedSkill = null;
    private bool cancelHighlighted = false;
    private bool buyHighlighted = false;

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
      applyCallback(Engine.TalentTree.PointsToAssign, Engine.SaveToString(), player);
      // Destroy(gameObject);
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
      cancelHighlighted = false;
      buyHighlighted = false;
      getTabPage(currentPage).SetActive(false);
      currentPage = page;
      var skillMenu = getTabPage(currentPage);
      skillMenu.SetActive(true);
      currentSkills = skillMenu.GetComponentsInChildren<McCoyTalentusSkill>();
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

    public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if (!inputInitialized)
      {
        Debug.Log("initializing input for skill menu");
        inputInitialized = true;
        input = new McCoyInputManager();
        input.RegisterButtonListener(ButtonPress.Forward, MoveRight);
        input.RegisterButtonListener(ButtonPress.Back, MoveLeft);
        input.RegisterButtonListener(ButtonPress.Up, MoveUp);
        input.RegisterButtonListener(ButtonPress.Down, MoveDown);
        input.RegisterButtonListener(ButtonPress.Button3, Back);
        input.RegisterButtonListener(ButtonPress.Button2, Confirm);
        input.RegisterButtonListener(ButtonPress.Button4, ResetSkill);
      }
      // if (talentDelegate != null) return false;
      return input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }

    private void ResetSkill()
    {
      if(selectedSkill != null)
      {
        selectedSkill.GetComponent<TalentUI>().DoRevert();
      }
    }

    private void Confirm()
    {
      if(buyHighlighted)
      {
        GetComponent<TalentusEngine>().Apply();
        return;
      }
      if(cancelHighlighted)
      {
        Back();
      }
      if (selectedSkill != null)
      {
        selectedSkill.GetComponent<TalentUI>().DoBuy();
      }
    }

    private void Back()
    {
      Debug.Log("Skills Menu Back!");
      GetComponent<TalentusEngine>().Revert();
      Close();
    }

    private void selectBuy()
    {
      //cancel may or may not be selected in this state
      buyHighlighted = true;
      buyButton.enabled = true;
    }
    private void selectCancel()
    {
      cancelHighlighted = true;
      cancelButton.enabled = true;
    }

    private void deselectButtons()
    {
      buyHighlighted = false;
      buyButton.enabled = false;
      cancelHighlighted = false;
      cancelButton.enabled = false;
    }

    private void selectSkill(McCoyTalentusSkill nextSkill)
    {
      if (selectedSkill != null)
      {
        selectedSkill.ToggleHighlight(false);
      }
      selectedSkill = nextSkill;
      selectedSkill.ToggleHighlight(true);
    }

    private void navigate(McCoyTalentusSkill.SkillNavDirection dir)
    {
      Debug.Log("Navigate! " + dir);
      McCoyTalentusSkill nextSkill = null;
      if (cancelHighlighted || buyHighlighted)
      {
        if (dir == McCoyTalentusSkill.SkillNavDirection.Down || dir == McCoyTalentusSkill.SkillNavDirection.Up)
        {
          foreach (var skill in currentSkills)
          {
            if ((dir == McCoyTalentusSkill.SkillNavDirection.Down && skill.IsDefaultSelection) || 
              (dir == McCoyTalentusSkill.SkillNavDirection.Up && skill.IsBottomSkill))
            {
              deselectButtons();
              nextSkill = skill;
              break;
            }
          }
        }
        else
        {
          if(cancelHighlighted)
          {
            selectBuy();
          }
          else
          {
            selectCancel();
          }
        }
      }
      else if(selectedSkill != null)
      {
        nextSkill = selectedSkill.Navigate(dir);
      }
      if (nextSkill == null)
      {
        selectedSkill = null;
        selectBuy();
      }
      else
      {
        selectSkill(nextSkill);
      }
    }

    private void MoveDown()
    {
      navigate(McCoyTalentusSkill.SkillNavDirection.Down);
    }

    private void MoveLeft()
    {
      navigate(McCoyTalentusSkill.SkillNavDirection.Left);
    }

    private void MoveUp()
    {
      navigate(McCoyTalentusSkill.SkillNavDirection.Up);
    }

    private void MoveRight()
    {
      navigate(McCoyTalentusSkill.SkillNavDirection.Right);
    }
  }
}
