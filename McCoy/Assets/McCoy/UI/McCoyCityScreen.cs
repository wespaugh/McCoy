using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using Assets.McCoy.RPG;
using com.cygnusprojects.TalentTree;
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
    float stingerDuration = 2.0f;

    [SerializeField]
    GameObject ZonePanelPrefab = null;

    [SerializeField]
    Transform cityPanelsRoot = null;

    [SerializeField]
    McCoyCityBoardContents boardContents = null;

    [SerializeField]
    MapCityNodePanel selectedZonePanel = null;

    [SerializeField]
    TMP_Text currentZoneText = null;

    [SerializeField]
    TMP_Text currentWeekText = null;

    [SerializeField]
    TMP_Text availableSkillPointsText = null;

    [SerializeField]
    McCoyMobRoutingUI routingUI = null;

    [SerializeField]
    GameObject debugEndWeekButton = null;

    [SerializeField]
    McCoyMapPanelListSectionHeader sectionHeaderPrefab = null;

    [SerializeField]
    AudioClip mapMusic = null;

    [SerializeField]
    AudioClip mobCombat = null;

    [SerializeField]
    GameObject RexSkillTree = null;

    List<McCoyMapPanelListSectionHeader> sectionHeaders = new List<McCoyMapPanelListSectionHeader>();

    PlayerCharacter selectedPlayer = PlayerCharacter.Rex;

    McCoyCityBoardContents board = null;
    McCoySkillTreeMenu talentDelegate = null;
    public McCoyCityBoardContents Board
    {
      get => board;
    }
    public PlayerCharacter Rex { get; private set; }

    [SerializeField]
    List<GameObject> bottomUIElements = new List<GameObject>();

    Dictionary<MapNode, GameObject> zonePanels = new Dictionary<MapNode, GameObject>();

    private McCoyStageData stageDataToLoad = null;

    bool loadingStage;

    bool mobRouting = false;
    bool mobDying = false;

    private void Awake()
    {
      loadingStage = false;
      debugEndWeekButton.SetActive(McCoy.GetInstance().Debug);
      if (board == null)
      {
        board = Instantiate(boardContents);
        updateWeekText();

        StartCoroutine(cityBooySequence());
        Camera.main.orthographic = false;
      }
      UFE.PlayMusic(mapMusic);
    }


    public override void DoFixedUpdate(
        IDictionary<InputReferences, InputEvents> player1PreviousInputs,
        IDictionary<InputReferences, InputEvents> player1CurrentInputs,
        IDictionary<InputReferences, InputEvents> player2PreviousInputs,
        IDictionary<InputReferences, InputEvents> player2CurrentInputs
      )
    {
      bool previousSkillButton = false;
      ButtonPress skillButton = ButtonPress.Button4;

      ButtonPress charForwardButton = ButtonPress.Button5;
      ButtonPress charBackwardButton = ButtonPress.Button6;
      bool previousCycleCharacterForward = false;
      bool previousCycleCharacterBackward = false;

      ButtonPress toggleZoom = ButtonPress.Button3;
      bool previousToggleZoom = false;
      bool currentToggleZoom = false;

      ButtonPress ToggleConnections = ButtonPress.Button1;
      bool previousToggleConnections = false;
      bool currentToggleConnections = false;

      foreach (KeyValuePair<InputReferences, InputEvents> pair in player1PreviousInputs)
      {
        if (pair.Key.inputType == InputType.Button && pair.Value.button)
        {
          if (pair.Key.engineRelatedButton == skillButton)
          {
            previousSkillButton = true;
          }
          else if (pair.Key.engineRelatedButton == charForwardButton)
          {
            previousCycleCharacterForward = true;
          }
          else if (pair.Key.engineRelatedButton == charBackwardButton)
          {
            previousCycleCharacterBackward = true;
          }
          else if(pair.Key.engineRelatedButton == ToggleConnections)
          {
            previousToggleConnections = true;
          }
          else if(pair.Key.engineRelatedButton == toggleZoom)
          {
            previousToggleZoom = true;
          }
        }
      }

      bool currentSkillButton = false;
      bool currentCycleCharacterForward = false;
      bool currentCycleCharacterBackward = false;
      foreach (KeyValuePair<InputReferences, InputEvents> pair in player1CurrentInputs)
      {
        if (pair.Key.inputType == InputType.Button && pair.Value.button)
        {
          if (pair.Key.engineRelatedButton == skillButton)
          {
            currentSkillButton = true;
          }
          else if (pair.Key.engineRelatedButton == charForwardButton)
          {
            currentCycleCharacterForward = true;
          }
          else if (pair.Key.engineRelatedButton == charBackwardButton)
          {
            currentCycleCharacterBackward = true;
          }
          else if( pair.Key.engineRelatedButton == ToggleConnections)
          {
            currentToggleConnections = true;
          }
          else if(pair.Key.engineRelatedButton == toggleZoom)
          {
            currentToggleZoom = true;
          }
        }
      }

      bool askSuperclass = false;

      if (!previousSkillButton && currentSkillButton)
      {
        OpenSkillTree();
      }
      else if(talentDelegate == null)
      {
        if (!previousCycleCharacterBackward && currentCycleCharacterBackward)
        {
          PreviousPlayer();
        }
        else if (!previousCycleCharacterForward && currentCycleCharacterForward)
        {
          NextPlayer();
        }
        else if(!previousToggleConnections && currentToggleConnections)
        {
          ToggleLines();
        }
        else if(!previousToggleZoom && currentToggleZoom)
        {
          ToggleZoom();
        }
        else
        {
          askSuperclass = true;
        }
      }
      else
      {
        askSuperclass = true;
      }
      if(askSuperclass)
      {
        base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
      }
    }

    private void saveCity()
    {
      List<MapNodeSearchData> updatedNodes = new List<MapNodeSearchData>();
      foreach(var node in board.MapNodes)
      {
        updatedNodes.Add(node.SearchData);
      }
      McCoy.GetInstance().gameState.UpdateSearchData(updatedNodes);
      McCoy.GetInstance().gameState.Save();
    }

    private IEnumerator cityBooySequence()
    {
      initPlayerStartLocations();
      initMapPanels();
      saveCity();
      checkForMobRouting();

      while(mobDying)
      {
        yield return null;
      }

      float stingerTime;
      if(mobRouting)
      {
        board.showStinger(McCoyStinger.StingerTypes.EnemiesRouted);
        stingerTime = Time.time;
        while (Time.time < stingerTime + stingerDuration)
        {
          yield return null;
        }
      }

      while(mobRouting)
      {
        yield return null;
      }

      if (McCoy.GetInstance().gameState.IsEndOfWeek)
      {
        board.showStinger(McCoyStinger.StingerTypes.WeekEnded);
      }
      else
      {
        board.showStinger(McCoyStinger.StingerTypes.SelectZone);
      }
      board.SelectMapNode(null, null);
      stingerTime = Time.time;
      while (Time.time < stingerTime + stingerDuration)
      {
        yield return null;
      }

      initSelectedCharacter();
      saveCity();
    }

    public void DebugEndWeek()
    {
      endWeek();
      selectedCharacterChanged();
    }

    private void initSelectedCharacter()
    {
      if(McCoy.GetInstance().gameState.IsEndOfWeek)
      {
        endWeek();
      }

      for(int i = 0; i < PlayerCharacters.Length; ++i)
      {
        if(McCoy.GetInstance().gameState.CanPlayerTakeTurn(PlayerCharacters[i]))
        {
          selectedPlayer = PlayerCharacters[i];
          break;
        }
      }
      selectedCharacterChanged();
    }

    public void ToggleLines()
    {
      board.ToggleLines();
    }

    public void ToggleZoom()
    {
      if (!mobRouting)
      {
        board.ToggleZoom();
      }
    }

    private void loadSkills(int availablePoints, string serializedSkills, PlayerCharacter pc)
    {
      talentDelegate = null;
      McCoy.GetInstance().gameState.playerCharacters[pc].AvailableSkillPoints = availablePoints;
      updateAvailableSkillPointsText();
      McCoy.GetInstance().gameState.UpdateSkills(pc, serializedSkills, availablePoints);
    }

    public void OpenSkillTree()
    {
      if(talentDelegate != null)
      {
        return;
      }
      string skillTreeString = "";
      int availableSkillPoints = 0;
      McCoyPlayerCharacter playerData = null;
      switch (selectedPlayer)
      {
        case PlayerCharacter.Rex:
          talentDelegate = Instantiate(RexSkillTree, transform.parent).GetComponent<McCoySkillTreeMenu>();
          playerData = McCoy.GetInstance().gameState.playerCharacters[PlayerCharacter.Rex];
          break;
      }
      if(playerData != null)
      {
        availableSkillPoints = playerData.AvailableSkillPoints;
        skillTreeString = playerData.SkillTreeString;
      }
      if(talentDelegate != null)
      {
        talentDelegate.SetAvailableSkillPoints(availableSkillPoints);
        if(!string.IsNullOrEmpty(skillTreeString))
        {
          talentDelegate.LoadSkills(skillTreeString, loadSkills);
        }
      }
    }

    private void endWeek()
    {
      McCoy.GetInstance().gameState.EndWeek();
      board.Weekend(weekendAnimationsFinished);
    }

    private void weekendAnimationsFinished()
    {
      refreshBoardAndPanels();
      StartCoroutine(weekendStingerFinished());
    }
    private IEnumerator weekendStingerFinished()
    {
      board.showStinger(McCoyStinger.StingerTypes.SelectZone);

      float stingerStartTime = Time.time;
      while(Time.time < stingerStartTime + stingerDuration)
      {
        yield return null;
      }

      if (!mobRouting)
      {
        board.ToggleZoom(true);
      }
      saveCity();
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
        McCoy.GetInstance().gameState.SetMobs(mapNode.NodeID, mobData);
      }
      McCoy.GetInstance().gameState.Initialize();
      string skillString = RexSkillTree.GetComponent<TalentusEngine>().SaveToString();
      skillString = RexSkillTree.GetComponent<TalentusEngine>().ResetSkillTree();
      loadSkills(1, skillString, Rex);
    }

    private void loadBoardState()
    {
      foreach(var m in board.MapNodes)
      {
        m.Mobs = McCoy.GetInstance().gameState.GetMobs(m.NodeID);
      }
    }

    private void initMapPanels()
    {
      if(McCoy.GetInstance().gameState.Initialized)
      {
        loadBoardState();
      }
      else
      {
        generateInitialBoardState();
      }
      foreach(var m in board.MapNodes)
      {
        m.SearchData = McCoy.GetInstance().gameState.GetSearchData(m.NodeID);
        m.SearchData.NodeID = m.NodeID;
        m.SearchData.ZoneName = m.ZoneName;
        foreach (string conn in m.connectionIDs)
        {
          m.AddConnectedNode(board.NodeWithID(conn));
        }
      }

      refreshBoardAndPanels();
    }

    private void initSectionHeaders()
    {
      if(sectionHeaders.Count != 0)
      {
        return;
      }
      for(int i = 0; i < 2; ++i)
      {
        McCoyMapPanelListSectionHeader header = Instantiate(sectionHeaderPrefab, cityPanelsRoot);
        header.Initialize(i == 0);
        sectionHeaders.Add(header);
      }
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

      MapNode antikytheraMechanismLocation = board.NodeWithID(McCoy.GetInstance().gameState.AntikytheraMechanismLocation);

      if(antikytheraMechanismLocation == null)
      {
        MapNode loc = board.MapNodes[Random.Range(0, board.MapNodes.Count)];
        loc.HasMechanism = true;
        McCoy.GetInstance().gameState.AntikytheraMechanismLocation = loc.NodeID;
        antikytheraMechanismLocation = board.NodeWithID(loc.NodeID);
      }
      MapCityNodePanel firstNode = null;
      foreach (var assetNode in board.MapNodes)
      {
        assetNode.SetMechanismLocation(antikytheraMechanismLocation);

        var nodePanel = Instantiate(ZonePanelPrefab, cityPanelsRoot);
        zonePanels[assetNode] = nodePanel;
        var node = nodePanel.GetComponent<MapCityNodePanel>();
        node.Initialize(assetNode, this, antikytheraMechanismLocation);
        if(firstNode == null)
        {
          firstNode = node;
        }
      }
      firstNode.GetComponent<Button>().Select();

      board.UpdateNodes();
      // updates panel interactibility
      selectedCharacterChanged();
    }

    private void updateWeekText()
    {
      currentWeekText.text = $"New Moon City\nWeek {McCoy.GetInstance().gameState.Week}: {PlayerName(selectedPlayer)}";
    }
    private void selectedCharacterChanged()
    {
      MapNode antikytheraMechanismLocation = board.NodeWithID(McCoy.GetInstance().gameState.AntikytheraMechanismLocation);

      MapNode playerLoc = board.NodeWithID(McCoy.GetInstance().gameState.PlayerLocation(selectedPlayer));

      currentZoneText.text = playerLoc.ZoneName;

      updateWeekText();
      updateAvailableSkillPointsText();

      selectedZonePanel.Initialize(playerLoc, null, antikytheraMechanismLocation);

      bool mechanismFound = antikytheraMechanismLocation == null ? false : antikytheraMechanismLocation.MechanismFoundHere; //.SearchStatus() == SearchState.CompletelySearched;

      int minDistanceToMechanism = -1;
      foreach (var v in playerLoc.GetConnectedNodes())
      {
        if (minDistanceToMechanism < 0 || (v as MapNode).DistanceToMechanism < minDistanceToMechanism)
        {
          minDistanceToMechanism = (v as MapNode).DistanceToMechanism;
        }
      }

      List<MapNode> sortedNodes = new List<MapNode>(zonePanels.Keys);
      sortedNodes.Sort((x, y) => sortMapNodes(x, y, playerLoc, mechanismFound, minDistanceToMechanism));
      
      List<MapNode> validConnections = new List<MapNode>();

      initSectionHeaders();

      int siblingIndex = 0;
      sectionHeaders[0].transform.SetSiblingIndex(siblingIndex++);
      bool iteratingThroughConnectedNodes = true;
      foreach (MapNode node in sortedNodes)
      {
        bool isConnected = false;
        foreach (var connection in playerLoc.GetConnectedNodes())
        {
          if (connection.NodeID == node.NodeID && (!mechanismFound || node.DistanceToMechanism <= minDistanceToMechanism))
          {
            validConnections.Add(connection as MapNode);
            isConnected = true;
          }
        }
        // when we get to the first disconnected node, insert the header for disconnected nodes
        if(iteratingThroughConnectedNodes && !isConnected)
        {
          iteratingThroughConnectedNodes = false;
          sectionHeaders[1].transform.SetSiblingIndex(siblingIndex++);
        }
        zonePanels[node].GetComponent<MapCityNodePanel>().SetInteractable(isConnected);
        zonePanels[node].transform.SetSiblingIndex(siblingIndex++);
      }

      board.SelectMapNode(playerLoc, validConnections);
      board.SetHoverNode(null);
    }

    private void updateAvailableSkillPointsText()
    {
      int value = McCoy.GetInstance().gameState.playerCharacters[selectedPlayer].AvailableSkillPoints;
      availableSkillPointsText.text = $"Skill Points: {value}<sprite name=\"controller_buttons_ps4_1\">";
    }

    private int sortMapNodes(MapNode x, MapNode y, MapNode playerLoc, bool mechanismFound, int minDistanceToMechanism)
    {
      {
        bool xIsConnected = false;
        bool yIsConnected = false;
        foreach (var connection in playerLoc.GetConnectedNodes())
        {
          if (x.NodeID == connection.NodeID)
          {
            xIsConnected = true;
          }
          if (y.NodeID == connection.NodeID)
          {
            yIsConnected = true;
          }
        }
        bool xIsCloseEnough = !mechanismFound || x.DistanceToMechanism <= minDistanceToMechanism;
        bool yIsCloseEnough = !mechanismFound || y.DistanceToMechanism <= minDistanceToMechanism;
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
          if (yIsConnected)
          {
            return 1;
          }
          // neither y or x is connected,
          // x is close enough
          if (xIsCloseEnough)
          {
            // y close enough, equivalent
            if (yIsCloseEnough)
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
            if (yIsCloseEnough)
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
      }
    }

    private void initPlayerStartLocations()
    {
      if(McCoy.GetInstance().gameState.PlayerLocation(Rex) != null)
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
      PlayerCharacter[] players = PlayerCharacters;
      for (int i = 0; i < players.Length; ++i)
      {
        int index = Random.Range(0, indices.Count); // the index in a list of numbers to randomly pick
        int randomMapPoint = indices[index]; // the randomly picked number
        McCoy.GetInstance().gameState.SetPlayerLocation(players[i],mapNodes[randomMapPoint]); // add the map node at the randomly picked number
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

      if (selectedPlayer == PlayerCharacter.Penelope && direction == 1)
      {
        selectedPlayer = PlayerCharacter.Rex;
      }
      else if(selectedPlayer == PlayerCharacter.Rex && direction == -1)
      {
        selectedPlayer = PlayerCharacter.Penelope;
      }
      else
      {
        selectedPlayer += direction;
      }

      if (McCoy.GetInstance().gameState.IsEndOfWeek)
      {
        Debug.LogError("inf recursion, bailing");
        return;
      }

      if (!McCoy.GetInstance().gameState.CanPlayerTakeTurn(selectedPlayer))
      {
        changePlayer(direction);
      }
      else
      {
        board.ToggleZoom(true);
        selectedCharacterChanged();
      }
    }

    public void PreviousPlayer()
    {
      changePlayer(-1);
    }

    public void LoadStage(MapNode node, McCoyStageData stageData)
    {
      board.SelectMapNode(node, null);
      stageDataToLoad = stageData;
      MapNode initialLocation = board.NodeWithID(McCoy.GetInstance().gameState.PlayerLocation(selectedPlayer));
      McCoy.GetInstance().gameState.SetPlayerLocation(selectedPlayer, node);
      Board.AnimateMobMove(Factions.Werewolves, initialLocation, node, .5f, LoadStageCallback);
    }
    public void LoadStageCallback()
    {
      McCoy.GetInstance().gameState.PlayerTakingTurn(selectedPlayer);
      if (!loadingStage)
      {
        loadingStage = true;
        McCoy.GetInstance().LoadBrawlerStage(stageDataToLoad);
      }
    }

    private void checkForMobRouting()
    {
      Dictionary<MapNode, List<McCoyMobData>> routedMobsInMapNodes = new Dictionary<MapNode, List<McCoyMobData>>();
      Dictionary<MapNode, List<McCoyMobData>> killedMobsInMapNodes = new Dictionary<MapNode, List<McCoyMobData>>();
      foreach(MapNode m in board.MapNodes)
      {
        List<McCoyMobData> mobsToRoute = new List<McCoyMobData>();
        List<McCoyMobData> mobsToKill = new List<McCoyMobData>();
        foreach (McCoyMobData mob in m.Mobs)
        {
          if(mob.MarkedForDeath)
          {
            mobsToKill.Add(mob);
          }
          else if (mob.IsRouted)
          {
            mobsToRoute.Add(mob);
          }
        }
        if(mobsToRoute.Count > 0)
        {
          routedMobsInMapNodes.Add(m, mobsToRoute);
        }
        if(mobsToKill.Count > 0)
        {
          killedMobsInMapNodes.Add(m, mobsToKill);
        }
      }

      mobDying = killedMobsInMapNodes.Count > 0;
      if(mobDying)
      {
        UFE.PlaySound(mobCombat);
      }
      // first, remove all dead mobs
      foreach (var kill in killedMobsInMapNodes)
      {
        foreach (var deadMob in kill.Value)
        {
          board.AnimateMobCombat(kill.Key, deadMob.Faction);
          kill.Key.Mobs.Remove(deadMob);
        }
      }

      mobRouting = routedMobsInMapNodes.Count > 0;

      if(mobDying || mobRouting)
      {
        StartCoroutine(showMobRoutingUI(routedMobsInMapNodes));
      }
    }

    private IEnumerator showMobRoutingUI(Dictionary<MapNode, List<McCoyMobData>> routedMobsInMapNodes)
    {
      if(mobDying)
      {
        float startTime = Time.time;
        while(Time.time < startTime + 1.0f)
        {
          yield return null;
        }
        mobDying = false;
        refreshBoardAndPanels();
      }
      if (mobRouting)
      {
        foreach (GameObject toHide in bottomUIElements)
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