﻿using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UFE3D;
using UnityEngine;
using UnityEngine.UI;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.UI
{
  public class McCoyCityScreen : UFEScreen
  {
    [SerializeField]
    GameObject ZonePanelPrefab = null;

    [SerializeField]
    Transform cityPanelsRoot = null;

    [SerializeField]
    McCoyCityBoardContents boardContents = null;

    [SerializeField]
    MapCityNodePanel selectedZonePanel = null;

    [SerializeField]
    List<Sprite> playerIconIndexes = new List<Sprite>();

    [SerializeField]
    Image playerIcon = null;

    [SerializeField]
    TMP_Text selectedCharacterText = null;

    [SerializeField]
    TMP_Text currentZoneText = null;

    [SerializeField]
    TMP_Text currentWeekText = null;

    [SerializeField]
    McCoyMobRoutingUI routingUI = null;

    int selectedPlayer = 1;

    McCoyCityBoardContents board = null;
    public McCoyCityBoardContents Board
    {
      get => board;
    }

    [SerializeField]
    List<GameObject> bottomUIElements = new List<GameObject>();

    Dictionary<MapNode, GameObject> zonePanels = new Dictionary<MapNode, GameObject>();

    private McCoyStageData stageDataToLoad = null;

    bool loadingStage;

    bool mobRouting = false;

    private void Awake()
    {
      loadingStage = false;
      if (board == null)
      {
        board = Instantiate(boardContents);
        currentWeekText.text = $"New Moon City\nWeek {McCoy.GetInstance().boardGameState.Week}";

        StartCoroutine(cityBooySequence());
        Camera.main.orthographic = false;
      }
    }

    private IEnumerator cityBooySequence()
    {
      initPlayerStartLocations();
      initMapPanels();
      checkForMobRouting();

      while(mobRouting)
      {
        yield return null;
      }
      initSelectedCharacter();
    }

    private void initSelectedCharacter()
    {
      if(McCoy.GetInstance().boardGameState.IsEndOfWeek)
      {
        McCoy.GetInstance().boardGameState.EndWeek();
        board.Weekend(weekendAnimationsFinished);
        currentWeekText.text = $"New Moon City\nWeek {McCoy.GetInstance().boardGameState.Week}";
      }

      for(int i = 1; i <= NUM_BOARDGAME_PLAYERS; ++i)
      {
        if(McCoy.GetInstance().boardGameState.CanPlayerTakeTurn(i))
        {
          selectedPlayer = i;
          break;
        }
      }
      selectedCharacterChanged();
    }

    private void weekendAnimationsFinished()
    {
      if (McCoy.GetInstance().boardGameState.AntikytheraMechanismLocation == null)
      {
        foreach (var m in board.MapNodes)
        {
          if (m.SearchPercent >= 100f)
          {
            McCoy.GetInstance().boardGameState.AntikytheraMechanismLocation = m;
            break;
          }
        }
      }

      refreshBoardAndPanels();
    }

    private void OnDestroy()
    {
      if (board != null)
      {
        Destroy(board.gameObject);
      }
    }

    public void generateInitialBoardState()
    {
      foreach (var mapNode in board.MapNodes)
      {
        List<Factions> fs = new List<Factions>();
        do
        {
          if (Random.Range(0, 6) <= 1) fs.Add(Factions.AngelMilitia);
          if (Random.Range(0, 6) <= 1) fs.Add(Factions.CyberMinotaurs);
          if (Random.Range(0, 6) <= 1) fs.Add(Factions.Mages);
        } while (fs.Count == 0);
        List<McCoyMobData> mobData = new List<McCoyMobData>();
        foreach (var f in fs)
        {
          mobData.Add(new McCoyMobData(f));
        }
        mapNode.Mobs = mobData;
        McCoy.GetInstance().boardGameState.SetMobs(mapNode.NodeID, mobData);
      }
      McCoy.GetInstance().boardGameState.Initialized = true;
    }

    private void loadBoardState()
    {
      foreach(var m in board.MapNodes)
      {
        m.Mobs = McCoy.GetInstance().boardGameState.GetMobs(m.NodeID);
      }
    }

    private void initMapPanels()
    {
      if(McCoy.GetInstance().boardGameState.Initialized)
      {
        loadBoardState();
      }
      else
      {
        generateInitialBoardState();
      }

      refreshBoardAndPanels();
    }

    private void refreshBoardAndPanels()
    {
      GameObject[] toDestroy = new GameObject[zonePanels.Count];
      zonePanels.Values.CopyTo(toDestroy,0);
      for(int i = 0; i < toDestroy.Length; ++i)
      {
        Destroy(toDestroy[i]);
      }
      zonePanels.Clear();

      MapNode antikytheraMechanismLocation = McCoy.GetInstance().boardGameState.AntikytheraMechanismLocation;

      foreach (var assetNode in board.MapNodes)
      {
        if (antikytheraMechanismLocation != null)
        {
          assetNode.SetMechanismLocation(antikytheraMechanismLocation);
        }

        var nodePanel = Instantiate(ZonePanelPrefab, cityPanelsRoot);
        zonePanels[assetNode] = nodePanel;
        nodePanel.GetComponent<MapCityNodePanel>().Initialize(assetNode, this);
      }

      board.UpdateNodes();
      // updates panel interactibility
      selectedCharacterChanged();
    }

    private void selectedCharacterChanged()
    {
      MapNode antikytheraMechanismLocation = McCoy.GetInstance().boardGameState.AntikytheraMechanismLocation;

      playerIcon.sprite = playerIconIndexes[selectedPlayer-1];
      MapNode playerLoc = McCoy.GetInstance().boardGameState.PlayerLocation(selectedPlayer);

      selectedCharacterText.text = ProjectConstants.PlayerName(selectedPlayer);
      currentZoneText.text = playerLoc.ZoneName;

      selectedZonePanel.Initialize(playerLoc, null);

      int minDistanceToMechanism = -1;
      foreach (var v in playerLoc.connectedNodes)
      {
        if (minDistanceToMechanism < 0 || (v as MapNode).DistanceToMechanism < minDistanceToMechanism)
        {
          minDistanceToMechanism = (v as MapNode).DistanceToMechanism;
        }
      }

      List<MapNode> sortedNodes = new List<MapNode>(zonePanels.Keys);
      sortedNodes.Sort((x, y) =>
      {
        bool xIsConnected = false;
        bool yIsConnected = false;
        foreach (var connection in playerLoc.connectedNodes)
        {
          if (x.NodeID == connection.NodeID)
          {
            xIsConnected = true;
          }
          if(y.NodeID == connection.NodeID)
          {
            yIsConnected = true;
          }
        }
        bool xIsCloseEnough = antikytheraMechanismLocation == null || x.DistanceToMechanism <= minDistanceToMechanism;
        bool yIsCloseEnough = antikytheraMechanismLocation == null || y.DistanceToMechanism <= minDistanceToMechanism;
        if (xIsConnected)
        {
          // x is connected and close enough. best case
          if (xIsCloseEnough)
          {
            if (yIsConnected && yIsCloseEnough)
            {
              return 0;
            }
            return -1;
          }
          // x is connected but not close enough
          else
          {
            // y not connected, x has priority
            if (!yIsConnected)
            {
              return -1;
            }
            // y is connected, but not close enough. same as X
            if (!yIsCloseEnough)
            {
              return 0;
            }
            // y is connected and close enough. y has priority
            return 1;
          }
        }
        // x is not connected
        else
        {
          // y is connected, y has priority
          if(yIsConnected)
          {
            return 1;
          }
          // neither y or x is connected,
          // x is close enough
          if(xIsCloseEnough)
          {
            // y close enough, equivalent
            if(yIsCloseEnough)
            {
              return 0;
            }
            // x close enough, y not close enough, x has priority
            else
            {
              return -1;
            }
          }
          // x not close enough
          else
          {
            // x not close enough, y close enough, y has priority
            if(yIsCloseEnough)
            {
              return 1;
            }
            else
            {
              // x not close enough, y not close enough, equivalent
              return 0;
            }
          }
        }
      });

      List<MapNode> validConnections = new List<MapNode>();

      int siblingIndex = 0;
      foreach (MapNode node in sortedNodes)
      {
        bool isConnected = false;
        foreach (var connection in playerLoc.connectedNodes)
        {
          if (connection.NodeID == node.NodeID && (antikytheraMechanismLocation == null || node.DistanceToMechanism <= minDistanceToMechanism))
          {
            validConnections.Add(connection as MapNode);
            isConnected = true;
          }
        }
        zonePanels[node].GetComponent<MapCityNodePanel>().SetInteractable(isConnected);
        zonePanels[node].transform.SetSiblingIndex(siblingIndex++);
      }

      board.SelectMapNode(playerLoc, validConnections);
      board.SetHoverNode(null);
    }

    private void initPlayerStartLocations()
    {
      if(McCoy.GetInstance().boardGameState.PlayerLocation(1) != null)
      {
        return;
      }
      List<MapNode> mapNodes = board.MapNodes;
      List<int> indices = new List<int>();
      // add a list of map point indexes to randomly pull from
      for (int i = 0; i < mapNodes.Count; ++i)
      {
        indices.Add(i);
      }
      for (int i = 0; i < ProjectConstants.NUM_BOARDGAME_PLAYERS; ++i)
      {
        int index = Random.Range(0, indices.Count); // the index in a list of numbers to randomly pick
        int randomMapPoint = indices[index]; // the randomly picked number
        McCoy.GetInstance().boardGameState.SetPlayerLocation(i+1,mapNodes[randomMapPoint]); // add the map node at the randomly picked number
        indices.RemoveAt(index); // remove the index so the same map point isn't picked again
      }
    }

    public void NextPlayer()
    {
      changePlayer(1);
    }

    private void changePlayer(int direction)
    {
      if(direction != 1 && direction != -1)
      {
        Debug.LogError("improper use of change player");
      }

      if (selectedPlayer >= NUM_BOARDGAME_PLAYERS && direction == 1)
      {
        selectedPlayer = 1;
      }
      else if(selectedPlayer <= 1 && direction == -1)
      {
        selectedPlayer = NUM_BOARDGAME_PLAYERS;
      }
      else
      {
        selectedPlayer += direction;
      }

      if (McCoy.GetInstance().boardGameState.IsEndOfWeek)
      {
        Debug.LogError("inf recursion, bailing");
        return;
      }

      if (!McCoy.GetInstance().boardGameState.CanPlayerTakeTurn(selectedPlayer))
      {
        changePlayer(direction);
      }
      else
      {
        selectedCharacterChanged();
      }
    }

    public void PreviousPlayer()
    {
      changePlayer(-1);
    }

    public void LoadStage(MapNode node, McCoyStageData stageData)
    {
      board.SelectMapNode(null, null);
      stageDataToLoad = stageData;
      MapNode initialLocation = McCoy.GetInstance().boardGameState.PlayerLocation(selectedPlayer);
      McCoy.GetInstance().boardGameState.SetPlayerLocation(selectedPlayer, node);
      Board.AnimateMobMove(Factions.Werewolves, initialLocation, node, .5f, LoadStageCallback);
    }
    public void LoadStageCallback()
    {
      McCoy.GetInstance().boardGameState.SetPlayerTookTurn(selectedPlayer);
      if (!loadingStage)
      {
        loadingStage = true;
        McCoy.GetInstance().LoadBrawlerStage(stageDataToLoad);
      }
    }

    private void checkForMobRouting()
    {
      Dictionary<MapNode, List<McCoyMobData>> routedMobsInMapNodes = new Dictionary<MapNode, List<McCoyMobData>>();
      foreach(MapNode m in board.MapNodes)
      {
        List<McCoyMobData> mobsToRoute = new List<McCoyMobData>();
        foreach (McCoyMobData mob in m.Mobs)
        {
          if (mob.IsRouted)
          {
            mobsToRoute.Add(mob);
          }
        }
        if(mobsToRoute.Count > 0)
        {
          routedMobsInMapNodes.Add(m, mobsToRoute);
        }
      }
      if (routedMobsInMapNodes.Count > 0)
      {
        mobRouting = true;
        foreach(GameObject toHide in bottomUIElements)
        {
          toHide.SetActive(false);
        }

        var routeMenu = Instantiate(routingUI, transform);
        routeMenu.Initialize(routedMobsInMapNodes, routingFinished, board);
      }
    }

    private void routingFinished(bool routingMenuClosed)
    {
      refreshBoardAndPanels();

      if(routingMenuClosed)
      {
        mobRouting = false;
        foreach(GameObject toShow in bottomUIElements)
        {
          toShow.SetActive(true);
        }
      }
    }

  }
}