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
    [SerializeField]
    TMP_Text skillName = null;

    [SerializeField]
    TMP_Text levelIndicator = null;

    public void Awake()
    {
      skillName.gameObject.SetActive(false);
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
    public void OnEnable()
    {
      StartCoroutine(oneFrameDelay());
    }
  }
}
