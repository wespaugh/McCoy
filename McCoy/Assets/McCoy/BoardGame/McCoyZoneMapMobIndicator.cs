using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  public class McCoyZoneMapMobIndicator : MonoBehaviour
  {
    [SerializeField]
    Transform mageObject = null;

    [SerializeField]
    Transform minotaurObject = null;

    [SerializeField]
    Transform militiaObject = null;

    [SerializeField]
    SpriteRenderer wolfIndicator = null;

    [SerializeField]
    SpriteRenderer zoneIcon = null;
    [SerializeField]
    Sprite zoneIconTexture = null;

    [SerializeField]
    TMP_Text zoneNameLabel = null;

    [SerializeField]
    GameObject hoverIndicator = null;

    public void UpdateWithMobs(List<McCoyMobData> mobs, int playerNum, string zoneName)
    {
      wolfIndicator.gameObject.SetActive(playerNum > 0);

      // zoneIcon.sprite = zoneIconTexture;
      zoneNameLabel.text = zoneName;

      hoverIndicator.SetActive(false);

      switch (playerNum)
      {
        case 1:
          wolfIndicator.color = Color.green;
          break;
        case 2:
          wolfIndicator.color = Color.red;
          break;
        case 3:
          wolfIndicator.color = Color.yellow;
          break;
        case 4:
          wolfIndicator.color = Color.cyan;
          break;
      }

      //zoneIcon.gameObject.SetActive(false);

      McCoyMobData mage = null;
      McCoyMobData minotaur = null;
      McCoyMobData militia = null;
      foreach(var m in mobs)
      {
        if(m.Faction == ProjectConstants.Factions.AngelMilitia)
        {
          militia = m;
        }
        else if(m.Faction == ProjectConstants.Factions.CyberMinotaurs)
        {
          minotaur = m;
        }
        else if(m.Faction == ProjectConstants.Factions.Mages)
        {
          mage = m;
        }
      }
      mageObject.gameObject.SetActive(mage != null);
      minotaurObject.gameObject.SetActive(minotaur != null);
      militiaObject.gameObject.SetActive(militia != null);

      mobs.Sort((x, y) =>
      {
        return y.XP - x.XP;
      });

      float currentY = 0.0f;
      foreach(var mob in mobs)
      {
        Transform building = null;
        switch(mob.Faction)
        {
          case ProjectConstants.Factions.Mages:
            building = mageObject;
            break;
          case ProjectConstants.Factions.CyberMinotaurs:
            building = minotaurObject;
            break;
          case ProjectConstants.Factions.AngelMilitia:
            building = militiaObject;
            break;
        }
        currentY = updateMobObject(mob, building, currentY);
      }
    }

    private float updateMobObject(McCoyMobData mob, Transform building, float currentY)
    {
      float strengthFactor = .5f + ((float)mob.XP) / 10.0f;
      float healthFactor = ((float)mob.Health) * .1f;
      building.localScale = new Vector3(strengthFactor, healthFactor, strengthFactor);
      building.localPosition = new Vector3(building.localPosition.x, currentY + (healthFactor/2.0f), building.localPosition.z);

      Color c;
      switch (mob.Faction)
      {
        case ProjectConstants.Factions.Mages:
          c = new Color(227.0f / 255.0f, 99.0f / 255.0f, 151.0f / 255.0f);
          break;
        case ProjectConstants.Factions.CyberMinotaurs:
          c = new Color(0.0f, 128.0f / 255.0f, 255.0f / 255.0f);
          break;
        case ProjectConstants.Factions.AngelMilitia:
          c = new Color(130.0f/255.0f, 209.0f/255.0f, 115.0f);
          break;
        default:
          c = Color.white;
          break;
      }

      building.GetComponent<MeshRenderer>().materials[0].color = c;

      return currentY;// + (healthFactor * 2.0f);
    }

    public void ToggleHover(bool on)
    {
      hoverIndicator.SetActive(on);
    }
  }
}