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
        float healthPreview = kills.Key.HealthPreview();
        status += $"{ProjectConstants.FactionDisplayName(kills.Key.Faction)}: {labelForHealth(healthPreview)}\n";
      }
      mobStatus.text = status;
    }

    private string labelForHealth(float value)
    {
      if (value >= 4f)
      {
        return $"{(int)value}";// "Healthy";
      }
      else if (value >= 3f)
      {
        return $"{(int)value}"; // "Shaken";
      }
      else if (value >= 2f)
      {
        return $"{(int)value}"; // "Weakened";
      }
      else if (value >= ProjectConstants.MOB_ROUTING_HEALTH_THRESHOLD)
      {
        return $"{(int)value}"; // "Weakened and Running Scared";
      }
      return "Weakened and Routed";
    }
  }
}