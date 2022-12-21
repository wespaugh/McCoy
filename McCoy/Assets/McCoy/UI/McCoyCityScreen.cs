using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using Assets.McCoy.Localization;
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
  public class McCoyCityScreen : UFEScreen, IMcCoyInputManager
  {
    float stingerDuration = 2.0f;

    [SerializeField]
    GameObject ZonePanelPrefab = null;

    [SerializeField]
    Transform cityPanelsRoot = null;

    [SerializeField]
    McCoyCityBoardContents boardContents = null;

    [SerializeField]
    GameObject uiRoot = null;

    [SerializeField]
    McCoyFiresideScene firesidePrefab = null;

    [SerializeField]
    MapCityNodePanel selectedZonePanel = null;

    [SerializeField]
    McCoyLocalizedText currentZoneText = null;

    [SerializeField]
    TMP_Text currentWeekText = null;

    [SerializeField]
    TMP_Text availableSkillPointsText = null;

    [SerializeField]
    McCoyMobRoutingUI routingUIPrefab = null;

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

    [SerializeField]
    AudioClip selectSound = null;

    [SerializeField]
    AudioClip moveCursorSound = null;

    McCoyInputManager inputManager;
    bool inputInitialized = false;

    List<McCoyMapPanelListSectionHeader> sectionHeaders = new List<McCoyMapPanelListSectionHeader>();

    PlayerCharacter selectedPlayer = PlayerCharacter.Rex;
    public PlayerCharacter SelectedPlayer
    {
      get => selectedPlayer;
    }

    McCoyFiresideScene fireside = null;

    McCoyCityBoardContents board = null;
    public McCoyCityBoardContents Board
    {
      get => board;
    }
    // TODO: Get rid of this, right? Right?
    public PlayerCharacter Rex { get; private set; }

    [SerializeField]
    List<GameObject> bottomUIElements = new List<GameObject>();

    Dictionary<MapNode, GameObject> zonePanels = new Dictionary<MapNode, GameObject>();
    List<GameObject> zonePanelList = new List<GameObject>();

    private McCoyStageData stageDataToLoad = null;
    bool loadingStage;

    bool mobRouting = false;
    bool mobDying = false;
    private McCoyMobRoutingUI routeMenu;

    private void Awake()
    {
      loadingStage = false;
      debugEndWeekButton.SetActive(McCoy.GetInstance().Debug);
      if (board == null)
      {
        board = Instantiate(boardContents);
        updateWeekText();

        StartCoroutine(cityBootSequence());
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
      CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }

    public bool CheckInputs(
      IDictionary<InputReferences, InputEvents> player1PreviousInputs,
      IDictionary<InputReferences, InputEvents> player1CurrentInputs,
      IDictionary<InputReferences, InputEvents> player2PreviousInputs,
      IDictionary<InputReferences, InputEvents> player2CurrentInputs
    )
    {

       if (!inputInitialized)
      {
        inputManager = new McCoyInputManager();
        inputInitialized = true;
        inputManager.RegisterButtonListener(ButtonPress.Button3, TransitionToFireside);
        inputManager.RegisterButtonListener(ButtonPress.Forward, NextPlayer);
        inputManager.RegisterButtonListener(ButtonPress.Back, PreviousPlayer);
        inputManager.RegisterButtonListener(ButtonPress.Button4, ToggleZoom);
        inputManager.RegisterButtonListener(ButtonPress.Button1, ToggleLines);
        inputManager.RegisterButtonListener(ButtonPress.Button2, enterCurrentZone);
        inputManager.RegisterButtonListener(ButtonPress.Up, NavigateUp);
        inputManager.RegisterButtonListener(ButtonPress.Down, NavigateDown);
      }

       if(routeMenu != null && routeMenu.gameObject.activeInHierarchy)
      {
        return routeMenu.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
      }
      else if(fireside.gameObject.activeInHierarchy)
      {
        return fireside.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
      }

      bool pushedAButton = inputManager.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

      if (pushedAButton)
      {
        this.DefaultNavigationSystem(
            player1PreviousInputs,
            player1CurrentInputs,
            player2PreviousInputs,
            player2CurrentInputs,
            this.moveCursorSound,
            this.selectSound,
            null,
            null
        );
      }
      return pushedAButton;
    }

    private void navigateZoneList(int dir)
    {
      int selectionIdx = 0;
      if (selectedZonePanel != null)
      {
        selectedZonePanel.Deselect();
        selectionIdx = zonePanelList.IndexOf(selectedZonePanel.gameObject);
        selectionIdx += dir;
        if(selectionIdx >= zonePanelList.Count)
        {
          selectionIdx = 0;
        }
        else if(selectionIdx < 0)
        {
          selectionIdx = zonePanelList.Count - 1;
        }
      }
      selectedZonePanel = zonePanelList[selectionIdx].GetComponent<MapCityNodePanel>();
      selectedZonePanel.Select();
    }

    private void NavigateDown()
    {
      navigateZoneList(1);
    }

    private void NavigateUp()
    {
      navigateZoneList(-1);
    }

    void enterCurrentZone()
    {
      selectedZonePanel?.ZoneClicked();
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

    private IEnumerator cityBootSequence()
    {
      initPlayerStartLocations();
      initQuests();
      initMapPanels();
      saveCity();
      checkForMobRouting();
      applyCauses();
      initFiresideScene();

      bool delayFireside = false;
      while(mobDying)
      {
        delayFireside = true;
        yield return null;
      }

      if(mobRouting)
      {
        delayFireside = true;
        yield return RunStinger(McCoyStinger.StingerTypes.EnemiesRouted);
      }

      while(mobRouting)
      {
        delayFireside = true;
        yield return null;
      }
      board.SelectMapNode(null, null);
      if(McCoy.GetInstance().gameState.IsEndOfWeek)
      {
        delayFireside = true;
        yield return RunStinger(McCoyStinger.StingerTypes.WeekEnded);
      }

      if(delayFireside)
      {
        yield return new WaitForSeconds(1.0f);
      }

      ShowFireside(!delayFireside);
      initSelectedCharacter();
      saveCity();
      board.IntroFinished();
      McCoy.GetInstance().Loading = false;
    }

    private void initFiresideScene()
    {
      fireside = Instantiate(firesidePrefab, board.CameraAnchor.transform);
      fireside.transform.localPosition = new Vector3(0f, -22.9f, 12.8f);
      fireside.gameObject.SetActive(false);
    }

    private void TransitionToFireside()
    {
      ShowFireside(false);
    }

    private void ShowFireside(bool cameraSnap=false)
    {
      if(fireside.gameObject.activeInHierarchy || ! board.Hide(cameraSnap))
      {
        return;
      }
      fireside.gameObject.SetActive(true);
      uiRoot.gameObject.SetActive(false);
      Dictionary<string, List<PlayerCharacter>> pcGroups = new Dictionary<string, List<PlayerCharacter>>();
      foreach(var pc in PlayerCharacters)
      {
        string zoneId = McCoy.GetInstance().gameState.PlayerLocation(pc);
        if(!pcGroups.ContainsKey(zoneId))
        {
          pcGroups[zoneId] = new List<PlayerCharacter>();
        }
        pcGroups[zoneId].Add(pc);
      }
      fireside.UpdateWithPCGroups(this, pcGroups);
      fireside.SelectPlayer(selectedPlayer);
    }

    public void CloseFireside()
    {
      if (!fireside.gameObject.activeInHierarchy || ! board.Show())
      {
        return;
      }
      fireside.gameObject.SetActive(false);
      uiRoot.gameObject.SetActive(true);
    }

    private IEnumerator RunStinger(McCoyStinger.StingerTypes type)
    {
      float stingerTime = Time.time;
      board.showStinger(type);
      while(Time.time < stingerTime + stingerDuration)
      {
        yield return null;
      }
    }

    private void initQuests()
    {
      if (!McCoy.GetInstance().Loading)
      {
        McCoyQuestManager.GetInstance().CityLoaded();
      }
      board.LoadQuests(McCoy.GetInstance().gameState.availableQuests);
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

    public void ZoneHighlighted(MapCityNodePanel panel, MapNode node)
    {
      this.selectedZonePanel = panel;
      Board.SelectMapNode(node, null, false);
      Board.ToggleZoom(true);
      Board.SetHoverNode(node);
    }

    private void applyCauses()
    {
      foreach(McCoyLobbyingCause.LobbyingCause cause in McCoyGameState.Instance().causesLobbiedFor)
      {
        McCoyLobbyingCauseManager.GetInstance().ApplyCause(cause, this);
      }
    }

    public void UnlockZone(string id)
    {
      board.UnlockZone(id);
      refreshBoardAndPanels();
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
        m.LoadSearchData(McCoy.GetInstance().gameState.GetSearchData(m.NodeID));
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
      zonePanelList.Clear();

      MapNode antikytheraMechanismLocation = board.NodeWithID(McCoy.GetInstance().gameState.AntikytheraMechanismLocation);

      if(antikytheraMechanismLocation == null)
      {
        MapNode loc = board.MapNodes[Random.Range(0, board.MapNodes.Count)];
        loc.HasMechanism = true;
        McCoy.GetInstance().gameState.AntikytheraMechanismLocation = loc.NodeID;
        antikytheraMechanismLocation = board.NodeWithID(loc.NodeID);
      }

      foreach (var assetNode in board.MapNodes)
      {
        if(assetNode.Disabled)
        {
          continue;
        }
        assetNode.SetMechanismLocation(antikytheraMechanismLocation);

        var nodePanel = Instantiate(ZonePanelPrefab, cityPanelsRoot);
        zonePanels[assetNode] = nodePanel;
        zonePanelList.Add(nodePanel);
        var node = nodePanel.GetComponent<MapCityNodePanel>();
        node.Initialize(assetNode, this, antikytheraMechanismLocation);
      }

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

      string dayKey = DayForSecondsRemaining(McCoy.GetInstance().gameState.TurnTimeRemaining(selectedPlayer));
      Localize(dayKey, (day) => 
      {
        Localize(playerLoc.ZoneName, (zone) =>
        {
          currentZoneText.SetTextDirectly(zone + "\n" + day);
        });
      });

      updateWeekText();

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
      GameObject firstNode = null;

      bool showDisconnectedNodes = false;

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
        var nodePanel = zonePanels[node].GetComponent<MapCityNodePanel>();
        nodePanel.SetInteractable(isConnected || McCoy.GetInstance().Debug);
        nodePanel.PlayerChanged();
        zonePanels[node].transform.SetSiblingIndex(siblingIndex++);
        // select the first node in the list
        if(firstNode == null)
        {
          firstNode = zonePanels[node];
          firstNode.GetComponent<Button>().Select();
        }
      }

      if(selectedZonePanel)
      {
        selectedZonePanel.Deselect();
      }
      firstNode.GetComponent<MapCityNodePanel>().Select();
      board.SelectMapNode(playerLoc, validConnections);
      board.SetHoverNode(null);
    }

    private int sortMapNodes(MapNode x, MapNode y, MapNode playerLoc, bool mechanismFound, int minDistanceToMechanism)
    {
      return MapNode.Compare(x, y, playerLoc, mechanismFound, minDistanceToMechanism);
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
        while (true)
        {
          int index = Random.Range(0, indices.Count); // the index in a list of numbers to randomly pick
          int randomMapPoint = indices[index]; // the randomly picked number
          MapNode startLoc = mapNodes[randomMapPoint];
          if(startLoc.Disabled)
          {
            continue;
          }
          McCoy.GetInstance().gameState.SetPlayerLocation(players[i], mapNodes[randomMapPoint]); // add the map node at the randomly picked number
          indices.RemoveAt(index); // remove the index so the same map point isn't picked again
          break;
        }
      }
    }

    public void NextPlayer()
    {
      ChangePlayer(1);
    }

    public void ChangePlayer(int direction, bool updateMap = true)
    {
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

      if (updateMap)
      {
        board.ToggleZoom(true);
        selectedCharacterChanged();
      }
    }

    public void PreviousPlayer()
    {
      ChangePlayer(-1);
    }

    public void LoadStage(MapNode node, McCoyStageData stageData)
    {
      // look for a quest at the zone we're heading to. if it's there, tee up the quest for the zone
      foreach(var quest in McCoy.GetInstance().gameState.availableQuests)
      {
        if(quest.possibleLocations[0] == node.NodeID)
        {
          McCoy.GetInstance().gameState.activeQuest = quest;
          break;
        }
      }
      board.SelectMapNode(node, null);
      stageDataToLoad = stageData;
      MapNode initialLocation = board.NodeWithID(McCoy.GetInstance().gameState.PlayerLocation(selectedPlayer));
      McCoy.GetInstance().gameState.SetPlayerLocation(selectedPlayer, node);
      Board.AnimateMobMove(Factions.Werewolves, initialLocation, node, .5f, LoadStageCallback);
    }
    public void LoadStageCallback()
    {
      if (!loadingStage)
      {
        loadingStage = true;
        McCoyGameState.Instance().SelectedPlayer = selectedPlayer;
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
        routeMenu = Instantiate(routingUIPrefab, transform);
        routeMenu.Initialize(routedMobsInMapNodes, routingFinished, board);
      }
    }

    private void routingFinished(bool routingMenuClosed)
    {
      refreshBoardAndPanels();

      if(routingMenuClosed)
      {
        mobRouting = false;
        routeMenu = null;
        foreach(GameObject toShow in bottomUIElements)
        {
          toShow.SetActive(true);
        }
      }
    }

  }
}