using com.cygnusprojects.TalentTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

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
    public IEnumerator oneFrameDelay()
    {
      yield return null;
      skillName.gameObject.SetActive(true);
      var talent = GetComponent<TalentUI>();
      string talentName = talent.Talent.Name;
      string displayName = ProjectConstants.DisplayStringForSkillName(talentName);
      skillName.text = displayName;
      if (levelIndicator != null)
      {
        levelIndicator.text = $"{talent.Talent.Level}/{talent.Talent.MaxLevel}";
      }
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
    }

    public void OnEnable()
    {
      StartCoroutine(oneFrameDelay());
    }
  }
}
