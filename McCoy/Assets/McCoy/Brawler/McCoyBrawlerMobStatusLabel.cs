using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  public class McCoyBrawlerMobStatusLabel : MonoBehaviour, IMobChangeDelegate
  {
    [SerializeField] TMP_Text mobStatus;

    public void MobsChanged(Dictionary<ProjectConstants.Factions, float> currentMobPopulations)
    {
      string status = "";
      foreach (var pop in currentMobPopulations)
      {
        status += $"{pop.Key}: {labelForPercent(pop.Value)}\n";
      }
      mobStatus.text = status;
    }

    private string labelForPercent(float value)
    {
      if (value > .8f)
      {
        return "Healthy";
      }
      else if (value > .6f)
      {
        return "Shaken";
      }
      else if (value > .4f)
      {
        return "Weakened";
      }
      else if (value > .2f)
      {
        return "Weakened and Running Scared";
      }
      return "Weakened and Routed";
    }
  }
}