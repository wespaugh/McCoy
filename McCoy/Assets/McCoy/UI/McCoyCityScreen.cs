using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
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

    int selectedPlayer = 1;

    McCoyCityBoardContents board = null;

    Dictionary<MapNode, GameObject> zonePanels = new Dictionary<MapNode, GameObject>();

    bool loadingStage;

    private void Awake()
    {
      loadingStage = false;
      if (board == null)
      {
        board = Instantiate(boardContents);
        currentWeekText.text = $"New Moon City\nWeek {McCoy.GetInstance().boardGameState.Week}";
        initPlayerStartLocations();
        initMapPanels();
        initSelectedCharacter();
        Camera.main.orthographic = false;
      }
    }

    private void initSelectedCharacter()
    {
      if(McCoy.GetInstance().boardGameState.IsEndOfWeek)
      {
        Debug.Log("END THE WEEK!");
        McCoy.GetInstance().boardGameState.EndWeek();
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

      foreach (var assetNode in board.MapNodes)
      {
        var nodePanel = Instantiate(ZonePanelPrefab, cityPanelsRoot);
        zonePanels[assetNode] = nodePanel;
        nodePanel.GetComponent<MapCityNodePanel>().Initialize(assetNode, this);
      }
      board.UpdateNodes();
    }

    private void selectedCharacterChanged()
    {
      playerIcon.sprite = playerIconIndexes[selectedPlayer-1];
      MapNode p1Loc = McCoy.GetInstance().boardGameState.PlayerLocation(selectedPlayer);
      board.SelectMapNode(p1Loc);

      selectedCharacterText.text = ProjectConstants.PlayerName(selectedPlayer);
      currentZoneText.text = p1Loc.ZoneName;

      selectedZonePanel.Initialize(p1Loc, this);

      List<MapNode> sortedNodes = new List<MapNode>(zonePanels.Keys);
      sortedNodes.Sort((x, y) =>
      {
        foreach (var v in p1Loc.connectedNodes)
        {
          if (x.NodeID == v.NodeID) return -1;
          if (y.NodeID == v.NodeID) return 1;
        }
        return 0;
      });

      int siblingIndex = 0;
      foreach (MapNode node in sortedNodes)
      {
        bool isConnected = false;
        foreach (var connection in p1Loc.connectedNodes)
        {
          if (connection.NodeID == node.NodeID)
          {
            isConnected = true;
            break;
          }
        }
        zonePanels[node].GetComponent<Button>().interactable = isConnected;
        zonePanels[node].transform.SetSiblingIndex(siblingIndex++);
      }
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
      McCoy.GetInstance().boardGameState.SetPlayerLocation(selectedPlayer, node);
      McCoy.GetInstance().boardGameState.SetPlayerTookTurn(selectedPlayer);
      if (!loadingStage)
      {
        loadingStage = true;
        McCoy.GetInstance().LoadBrawlerStage(stageData);
      }
    }
  }
}