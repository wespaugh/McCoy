using com.cygnusprojects.TalentTree;
using static Assets.McCoy.ProjectConstants;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.RPG
{
  public class McCoyTalentusSkill : MonoBehaviour
  {
    public enum SkillNavDirection
    {
      Left,
      Down,
      Right,
      Up
    }

    [SerializeField]
    TMP_Text skillName = null;

    [SerializeField]
    TMP_Text levelIndicator = null;

    [SerializeField]
    GameObject highlight = null;

    [SerializeField]
    McCoyTalentusSkill NavUp = null;

    [SerializeField]
    McCoyTalentusSkill NavDown = null;

    [SerializeField]
    McCoyTalentusSkill NavLeft = null;

    [SerializeField]
    McCoyTalentusSkill NavRight = null;

    [SerializeField]
    bool isDefaultSelection = false;
    public bool IsDefaultSelection { get => isDefaultSelection; }

    [SerializeField]
    bool isBottomSkill = false;
    public bool IsBottomSkill { get => isBottomSkill; }

    public void Awake()
    {
      skillName.gameObject.SetActive(false);
      highlight.SetActive(IsDefaultSelection);
      StartCoroutine(oneFrameDelay());
    }
    public void Refresh()
    {
      if (levelIndicator != null)
      {
        levelIndicator.text = $"";
      }
      StartCoroutine(oneFrameDelay());
    }
    public IEnumerator oneFrameDelay()
    {
      yield return null;
      skillName.gameObject.SetActive(true);
      var talent = GetComponent<TalentUI>();
      string talentName = talent.Talent.Name;
      Localize($"com.mccoy.rpg.{talentName}", (displayName) =>
      {
        displayName = ProjectConstants.DisplayStringForSkillName(talentName);
        skillName.text = displayName;
        if (levelIndicator != null)
        {
          string labelText = $"{talent.Talent.Level}/{talent.Talent.MaxLevel}";
          if (!talent.Talent.isValid)
          {
            labelText = $"<color=red>{labelText}</color>";
          }
          levelIndicator.text = labelText;
        }
      });
    }

    public McCoyTalentusSkill Navigate(SkillNavDirection dir)
    {
      switch(dir)
      {
        case SkillNavDirection.Down:
          return NavDown;
        case SkillNavDirection.Left:
          return NavLeft;
        case SkillNavDirection.Right:
          return NavRight;
        default:
          return NavUp;
      }
    }

    public void ToggleHighlight(bool on)
    {
      highlight.SetActive(on);
      if(on)
      {
        highlight.GetComponent<Image>().color = GetComponent<TalentUI>().Talent.isValid ? Color.white : Color.gray;
      }
    }

    public void OnEnable()
    {
      StartCoroutine(oneFrameDelay());
    }
  }
}
