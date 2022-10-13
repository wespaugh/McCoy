using com.cygnusprojects.TalentTree;
using System;
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
    [SerializeField]
    TMP_Text skillName = null;

    [SerializeField]
    TMP_Text levelIndicator = null;

    public void Awake()
    {
      var talent = GetComponent<TalentUI>();
      string talentName = talent.Talent.Name;
      skillName.text = ProjectConstants.DisplayStringForSkillName(talentName);
      if (levelIndicator != null)
      {
        levelIndicator.text = $"{talent.Talent.Level}/{talent.Talent.MaxLevel}";
      }
    }
  }
}
