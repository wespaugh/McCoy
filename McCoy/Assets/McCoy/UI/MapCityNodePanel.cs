using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using Assets.McCoy.Localization;
using Assets.McCoy.RPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UFE3D.Brawler;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.UI
{
  public class MapCityNodePanel : MonoBehaviour
  {
    [SerializeField]
    TMP_Text NodeName = null;

    [SerializeField]
    TMP_Text SearchStatus = null;

    [SerializeField]
    GameObject MobNodeContainer = null;

    [SerializeField]
    TMP_Text MobText = null;

    [SerializeField]
    Image selectionIcon = null;

    [SerializeField]
    McCoyLocalizedText questTitle = null;

    [SerializeField]
    McCoyLocalizedText questSummary = null;

    private ScrollRect scrollRect;
    protected ScrollRect ScrollRect
    {
      get
      {
        if (scrollRect == null)
        {
          scrollRect = GetComponentInParent<ScrollRect>();
        }
        return scrollRect;
      }
    }
    protected RectTransform contentPanel => transform.parent as RectTransform;

    MapNode node = null;
    public MapNode Zone
    {
      get => node;
    }

    List<McCoyMobData> mobs = new List<McCoyMobData>();

    McCoyCityScreen uiRoot = null;

    McCoyQuestData quest = null;

    string titleText = "";

    bool canConnect = false;
    bool isSelected = false;

    bool bound = false;

    private void Awake()
    {
      selectionIcon.gameObject.SetActive(false);
      if(isSelected)
      {
        StartCoroutine(select());
      }
      //bind();
    }

    private void OnDestroy()
    {
      //unbind();
    }

    /*
    private void bind()
    {
      if(bound)
      {
        return;
      }
      bound = true;
    }

    private void unbind()
    {
      if(! bound)
      {
        return;
      }
      Selection.selectionChanged -= selectionChanged;
    }
    */
    public void Select()
    {
      Canvas.ForceUpdateCanvases();
      Debug.Log("Selecting " + node.ZoneName);
      isSelected = true;
      if (gameObject.activeInHierarchy)
      {
        StartCoroutine(select());
      }
    }
    private IEnumerator select()
    {
      var anch = contentPanel.anchoredPosition;
      Vector3 nodePosition = transform.GetSiblingIndex() == 1 ? transform.parent.GetChild(0).transform.position : transform.position;
      while(ScrollRect == null)
      {
        yield return null;
      }
      while(contentPanel == null)
      {
        yield return null;
      }
      while(nodePosition == null)
      {
        yield return null;
      }
      var newPos = (Vector2)ScrollRect.transform.InverseTransformPoint(contentPanel.position)
              - (Vector2)ScrollRect.transform.InverseTransformPoint(nodePosition);
      anch.y = newPos.y;
      contentPanel.anchoredPosition = anch;
      updateIcon();
      ZoneHighlighted();
    }

    private void updateIcon()
    {
      selectionIcon.gameObject.SetActive(isSelected);
      selectionIcon.color = canConnect ? ProjectConstants.GREEN : ProjectConstants.PINK;
    }

    public void Deselect()
    {
      Debug.Log("DESelecting " + node?.ZoneName);
      isSelected = false;
      updateIcon();
    }

    public void Initialize(MapNode node, McCoyCityScreen screen, MapNode mechanismLoc)
    {
      this.node = node;
      NodeName.GetComponent<McCoyLocalizedText>().SetText(node.ZoneName);
      SearchStatus.text = $"{SearchStateDisplay(node.SearchStatus())}";
      if (screen != null && mechanismLoc != null && mechanismLoc.SearchStatus() == SearchState.CompletelySearched)
      {
        SearchStatus.text = $"Distance to goal: {node.DistanceToMechanism}";
      }
      uiRoot = screen;

      canConnect = uiRoot != null;

      selectionIcon.gameObject.SetActive(canConnect);

      this.mobs.Clear();

      string mobString = "";
      foreach(var mobData in node.Mobs)
      {
        mobString += "<sprite name=\"";
        switch (mobData.Faction)
        {
          case Factions.Mages:
            mobString += "mage";
            break;
          case Factions.CyberMinotaurs:
            mobString += "minotaur";
            break;
          case Factions.AngelMilitia:
            mobString += "militia";
            break;
        }
        mobString += "\">";
        mobString += $"<sprite name=\"strength\">{mobData.StrengthForXP()}<sprite name=\"health\">{mobData.Health}\n";
        this.mobs.Add(mobData);
      }
      MobText.text = mobString;

      McCoyQuestData quest = null;
      foreach(var q in McCoy.GetInstance().gameState.availableQuests)
      {
        if(q.possibleLocations[0] == node.NodeID)
        {
          quest = q;
          break;
        }
      }
      SetQuest(quest, screen);
      PlayerChanged();
    }

    private void SetQuest(McCoyQuestData q, McCoyCityScreen screen)
    {
      quest = q;
      questTitle.gameObject.SetActive(q != null);
      questSummary.gameObject.SetActive(q != null);
      if (q == null)
      {
        return;
      }

      questTitle.SetText(q.title, finalizeTitle);
      questSummary.SetText(q.summary);
    }

    public void PlayerChanged()
    {
      finalizeTitle(null);
    }

    private void finalizeTitle(string text)
    {
      if(quest == null || quest.characterRestriction == PlayerCharacter.None)
      {
        return;
      }
      if(string.IsNullOrEmpty(titleText))
      {
        titleText = text;
      }
      string restrictionColor = uiRoot.SelectedPlayer != quest.characterRestriction ? "red" : "green";
      string restriction = titleText + $" <color=\"{restrictionColor}\">({PlayerName(quest.characterRestriction)} only)</color>";
      questTitle.SetTextDirectly(restriction);
    }

    public void ZoneClicked()
    {
      if(uiRoot == null || (!canConnect && !McCoy.GetInstance().Debug))
      {
        return;
      }
      McCoyStageData stageData = new McCoyStageData();
      if(mobs.Count == 0)
      {
        Factions f;
        switch(UnityEngine.Random.Range(1,4))
        {
          case 1:
            f = Factions.Mages;
            break;
          case 2:
            f = Factions.AngelMilitia;
            break;
          case 3:
            f = Factions.CyberMinotaurs;
            break;
          default:
            f = Factions.CyberMinotaurs;
            break;
        }
        mobs.Add(new McCoyMobData(f, 1, 1));
      }
      stageData.Initialize(node.ZoneName, mobs);

      uiRoot.LoadStage(node, stageData);
    }

    public void ZoneHighlighted()
    {
      uiRoot.ZoneHighlighted(this, node);
    }

    public void SetInteractable(bool isConnected)
    {
      canConnect = isConnected;
      selectionIcon.gameObject.SetActive(isConnected && isSelected);
    }
  }
}