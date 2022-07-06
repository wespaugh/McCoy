using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.UI
{
  public class MapCityNodePanelMob : MonoBehaviour
  {
    [SerializeField]
    Image minotaurImage;

    [SerializeField]
    Image militiaImage;

    [SerializeField]
    Image mageImage;

    [SerializeField]
    TMP_Text health;
    [SerializeField]
    TMP_Text strength;

    public void Initialize(Factions f, int str, int hp)
    {
      health.text = $"{hp}x<sprite=1>";
      strength.text = $"{str}x<sprite=0>";

      minotaurImage.gameObject.SetActive(false);
      mageImage.gameObject.SetActive(false);
      militiaImage.gameObject.SetActive(false);
      switch (f)
      {
        case Factions.AngelMilitia: militiaImage.gameObject.SetActive(true); break;
        case Factions.CyberMinotaurs: minotaurImage.gameObject.SetActive(true); break;
        case Factions.Mages: mageImage.gameObject.SetActive(true); break;
      }
    }
  }
}