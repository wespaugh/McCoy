using Assets.McCoy.RPG;
using Assets.McCoy.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.BoardGame
{
  public class McCoyZoneMapMobIndicator : MonoBehaviour
  {
    const string factionAnimatorParam = "Faction";
    [SerializeField]
    Animator mageObject = null;
    [SerializeField]
    TMP_Text mageText = null;

    [SerializeField]
    Animator minotaurObject = null;
    [SerializeField]
    TMP_Text minotaurText = null;

    [SerializeField]
    Animator militiaObject = null;
    [SerializeField]
    TMP_Text militiaText = null;

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

    [SerializeField]
    Animator factionIcon = null;

    [SerializeField]
    MeshRenderer baseMesh = null;

    [SerializeField]
    Animator mechanismIndicator = null;

    [SerializeField]
    McCoyProgressBar searchProgressBar = null;

    [SerializeField]
    SpriteRenderer selectionHighlight = null;

    [SerializeField]
    float deselectAlpha = .85f;

    [SerializeField]
    float selectAlphaMin = .6f;

    [SerializeField]
    float selectAlphaMax = 1f;

    [SerializeField]
    Animator QuestIndicator = null;

    bool selected = false;

    bool pulsing = false;
    float pulseStart = 0f;
    private bool playerColorOverridesMobColor = false;

    public void UpdateWithMobs(List<McCoyMobData> mobs, int playerNum, string zoneName, float searchPercent, bool showMechanism = false)
    {
      wolfIndicator.gameObject.SetActive(playerNum > 0);
      wolfIndicator.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
      wolfIndicator.GetComponent<Animator>().SetInteger(factionAnimatorParam, 1);
      // zoneIcon.sprite = zoneIconTexture;
      zoneNameLabel.text = zoneName;

      ToggleHover(false);

      hoverIndicator.SetActive(selectionHighlight == null);

      searchProgressBar.Initialize(100, 100);
      searchProgressBar.SetFill(searchPercent);

      mechanismIndicator.gameObject.SetActive(showMechanism);
      if(showMechanism)
      {
        mechanismIndicator.SetBool("Boss", true);
      }

      switch (playerNum)
      {
        case 1:
          wolfIndicator.color = Color.green;
          break;
        case 2:
          wolfIndicator.color = Color.red;
          break;
        case 3:
          wolfIndicator.color = new Color(1f, .64f, 0f);
          break;
        case 4:
          wolfIndicator.color = Color.cyan;
          break;
      }

      McCoyMobData strongestMob = null;

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
        if(
          strongestMob == null || // any mob is stronger than no mob
          m.StrengthForXP() > strongestMob.StrengthForXP() || // strongest mob is strongest
          (m.StrengthForXP() == strongestMob.StrengthForXP() && m.Health > strongestMob.Health) || // healthiest mob breaks ties between equally strong mobs
          (m.StrengthForXP() == strongestMob.StrengthForXP() && m.Health == strongestMob.Health && UnityEngine.Random.Range(1,3) == 1) // randomness breaks ties between equally strong, equally healthy mobs
          )
        {
          strongestMob = m;
        }
      }
      initMob(mageObject, mage, 2, mageText);
      initMob(militiaObject, militia, 3, militiaText);
      initMob(minotaurObject, minotaur, 4, minotaurText);

      Color highlightColor = Color.black;
      if (playerNum > 0 && playerColorOverridesMobColor)
      {
        highlightColor = wolfIndicator.color;
      }
      else if(strongestMob != null)
      {
        switch (strongestMob.Faction)
        {
          case Factions.Mages:
            highlightColor = Color.red;
            break;
          case Factions.CyberMinotaurs:
            highlightColor = Color.cyan;
            break;
          case Factions.AngelMilitia:
            highlightColor = Color.yellow;
            break;
        }
      }

      if (selectionHighlight != null)
      {
        selectionHighlight.color = highlightColor;
      }
      else
      {
        baseMesh.material.color = highlightColor;
        baseMesh.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.black;
      }
      SetSelected(false);

      mobs.Sort((x, y) =>
      {
        return y.XP - x.XP;
      });

      float currentY = 0.0f;
      foreach(var mob in mobs)
      {
        Transform building = animatorForFaction(mob.Faction).transform;
        currentY = updateMobObject(mob, building, currentY);
      }
    }

    private void initMob(Animator anim, McCoyMobData m, int factionIndex, TMP_Text text)
    {
      anim.gameObject.SetActive(m != null);
      anim.SetInteger(factionAnimatorParam, factionIndex);
      if (m != null)
      {
        text.gameObject.SetActive(true);
        text.SetText($"{m.StrengthForXP()}<sprite name=\"stat_indicator_icons_1\">{m.Health}<sprite name=\"stat_indicator_icons_0\">");
      }
      else
      {
        text.gameObject.SetActive(false);
      }
    }

    private Animator animatorForFaction(Factions f)
    {
      switch (f)
      {
        case ProjectConstants.Factions.Mages:
          return mageObject;
        case ProjectConstants.Factions.CyberMinotaurs:
          return minotaurObject;
        case ProjectConstants.Factions.AngelMilitia:
          return militiaObject;
        default: 
          return wolfIndicator.GetComponent<Animator>();
      }
    }

    public void AnimateFaction(Factions faction, Vector3 diff, float time, Action callback, bool hideOriginal)
    {
      Animator existingAnimator = animatorForFaction(faction);
      
      Animator meeple = Instantiate(factionIcon, existingAnimator.transform.parent);
      meeple.transform.localPosition = existingAnimator.transform.localPosition;
      meeple.SetInteger(factionAnimatorParam, (int)faction);
      meeple.GetComponent<SpriteRenderer>().color = existingAnimator.GetComponent<SpriteRenderer>().color;

      if (hideOriginal)
      {
        existingAnimator.gameObject.SetActive(false);
      }
      StartCoroutine(updateAnimation(meeple.gameObject, diff, time, callback));
    }

    private IEnumerator updateAnimation(GameObject meeple, Vector3 diff, float totalTime, Action callback)
    {
      Vector3 startPos = meeple.transform.localPosition;
      Vector3 endPos = startPos + diff;

      float startTime = Time.time;
      meeple.transform.localPosition = startPos;
      while(Time.time  < startTime + totalTime)
      {
        meeple.transform.localPosition = CalculateQuadraticBezierPoint((Time.time - startTime) / totalTime, startPos, CalculateArcMidpointBetweenPoints(startPos, endPos), endPos); // Vector3.Lerp(startPos, endPos, (Time.time - startTime) / totalTime);
        yield return null;
      }
      meeple.transform.position = endPos;
      Destroy(meeple);
      callback();
    }

    private float updateMobObject(McCoyMobData mob, Transform building, float currentY)
    {
      float strengthFactor = 3f;// + ((float)mob.XP) / 2.0f;
      float healthFactor = ((float)mob.Health) * .1f;
      building.localScale = new Vector3(strengthFactor, strengthFactor, strengthFactor);
      building.localPosition = new Vector3(building.localPosition.x, currentY + (healthFactor/2.0f), building.localPosition.z);

      /*
      Color c;
      switch (mob.Faction)
      {
        case ProjectConstants.Factions.Mages:
          c = new Color(227.0f / 255.0f, 99.0f / 255.0f, 151.0f / 255.0f);
          break;
        case ProjectConstants.Factions.CyberMinotaurs:
          c = Color.white;// c = new Color(0.0f, 128.0f / 255.0f, 255.0f / 255.0f);
          break;
        case ProjectConstants.Factions.AngelMilitia:
          c = new Color(0.0f, 255.0f / 255.0f, 128.0f / 255.0f); // c = new Color(130.0f/255.0f, 209.0f/255.0f, 115.0f);
          break;
        default:
          c = Color.white;
          break;
      }

      building.GetComponent<SpriteRenderer>().materials[0].color = c;
      */
      return currentY;// + (healthFactor * 2.0f);
    }

    public void SetSelected(bool select)
    {
      if(selectionHighlight == null)
      {
        ToggleHover(select);
      }
      else
      {
        Color c = selectionHighlight.color;
        c.a = select ? selectAlphaMin : deselectAlpha;
        selectionHighlight.color = c;
      }
      if(select && ! selected && ! pulsing)
      {
        StartCoroutine(pulse());
      }
      selected = select;
    }

    private IEnumerator pulse()
    {
      if(selectionHighlight == null)
      {
        yield break;
      }
      pulsing = true;
      while (true)
      {
        pulseStart = Time.time;
        float pulseDuration = 1.0f;
        while (Time.time < pulseStart + pulseDuration)
        {
          yield return null;
          Color c = selectionHighlight.color;
          if (!selected)
          {
            c.a = deselectAlpha;
            selectionHighlight.color = c;
            pulsing = false;
            yield break;
          }
          c.a = Mathf.Lerp(selectAlphaMin, selectAlphaMax, Time.time - pulseStart / pulseDuration);
          selectionHighlight.color = c;
        }
        pulseStart = Time.time;
        while (Time.time < pulseStart + pulseDuration)
        {
          yield return null;
          Color c = selectionHighlight.color;
          if (!selected)
          {
            c.a = deselectAlpha;
            selectionHighlight.color = c;
            pulsing = false;
            yield break;
          }
          c.a = Mathf.Lerp(selectAlphaMax, selectAlphaMin, Time.time - pulseStart / pulseDuration);
          selectionHighlight.color = c;
        }
      }
    }

    public void ToggleHover(bool on)
    {
      hoverIndicator.transform.localScale = on ? Vector3.one : new Vector3(.5f, .5f, .5f);
    }

    public void AnimateCombat(Factions faction)
    {
      var factionAnim = animatorForFaction(faction);
      factionAnim.gameObject.SetActive(true);
      factionAnim.SetTrigger("FIGHT");

      var werewolf = animatorForFaction(Factions.Werewolves);
      if (werewolf.gameObject.activeInHierarchy)
      {
        werewolf.SetTrigger("FIGHT");
      }
    }

    public void ShowQuest(McCoyQuestData quest)
    {
      QuestIndicator.gameObject.SetActive(true);
      string animKey = "WorldQuest";
      switch(quest.characterRestriction)
      {
        case PlayerCharacter.Rex:
          animKey = "RexQuest";
          break;
        case PlayerCharacter.Vicki:
          animKey = "VickiQuest";
          break;
        case PlayerCharacter.Avalon:
          animKey = "AvalonQuest";
          break;
        case PlayerCharacter.Penelope:
          animKey = "PenelopeQuest";
          break;
      }
      QuestIndicator.SetTrigger(animKey);
    }
  }
}