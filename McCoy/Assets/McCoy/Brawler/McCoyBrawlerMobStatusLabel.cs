using Assets.McCoy.BoardGame;
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

    public void MobsChanged(Dictionary<McCoyMobData, int> killDict)
    {
      string status = "";
      foreach (var kills in killDict)
      {
        float healthPreview = kills.Key.HealthPreview(kills.Value);
        Debug.Log("HealthPreview for " + kills.Key.Faction + ": " + healthPreview);
        status += $"{kills.Key.Faction}: {labelForHealth(healthPreview)}\n";
      }
      mobStatus.text = status;
    }

    private string labelForHealth(float value)
    {
      if (value >= 4f)
      {
        return "Healthy";
      }
      else if (value >= 3f)
      {
        return "Shaken";
      }
      else if (value >= 2f)
      {
        return "Weakened";
      }
      else if (value >= 1f)
      {
        return "Weakened and Running Scared";
      }
      return "Weakened and Routed";
    }
  }
}