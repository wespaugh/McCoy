using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using Assets.McCoy.Localization;
using Assets.McCoy.RPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UFE3D;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.UI
{
  public class McCoyCityScreen : UFEScreen, IMcCoyInputManager
  {
    float stingerDuration = 2.0f;

    #region Inspector
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
    GameObject debugFindMechanismButton = null;

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

    [SerializeField]
    GameObject endgameCutscene = null;
    #endregion


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

    [SerializeField]
    List<GameObject> bottomUIElements = new List<GameObject>();

    Dictionary<MapNode, GameObject> zonePanels = new Dictionary<MapNode, GameObject>();
    List<GameObject> zonePanelList = new List<GameObject>();

    private McCoyStageData stageDataToLoad = null;
    bool loadingStage;

    bool mobRouting = false;
    bool weekEnding = false;
    bool mobDying = false;
    bool gameEnding = false;
    private McCoyMobRoutingUI routeMenu;

    private void Awake()
    {
      loadingStage = false;
      debugEndWeekButton.SetActive(McCoy.GetInstance().DebugUI);
      debugFindMechanismButton.SetActive(McCoy.GetInstance().DebugUI);
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

    private void initializeInput()
    {
      inputManager = new McCoyInputManager();
      inputInitialized = true;
      if (gameEnding)
      {
        inputManager.RegisterButtonListener(ButtonPress.Button2, ConfirmPressed);
      }
      else
      {
        inputManager.RegisterButtonListener(ButtonPress.Button3, TransitionToFireside);
        inputManager.RegisterButtonListener(ButtonPress.Forward, NextPlayer);
        inputManager.RegisterButtonListener(ButtonPress.Back, PreviousPlayer);
        inputManager.RegisterButtonListener(ButtonPress.Button4, ToggleZoom);
        inputManager.RegisterButtonListener(ButtonPress.Button1, ToggleLines);
        inputManager.RegisterButtonListener(ButtonPress.Button2, ConfirmPressed);
        inputManager.RegisterButtonListener(ButtonPress.Up, NavigateUp);
        inputManager.RegisterButtonListener(ButtonPress.Down, NavigateDown);
      }
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
        initializeInput();
      }

      if (routeMenu != null && routeMenu.gameObject.activeInHierarchy)
      {
        return routeMenu.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
      }
      else if (fireside.gameObject.activeInHierarchy)
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
            moveCursorSound,
            selectSound,
            null,
            null
        );
      }
      return pushedAButton;
    }

    private void navigateZoneList(int dir)
    {
      if(gameEnding)
      {
        return;
      }
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

    private void ConfirmPressed()
    {
      if (gameEnding)
      {
        UFE.ShowScreen(UFE.GetMainMenuScreen());
      }
      else
      {
        enterCurrentZone();
      }
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
      if (checkGameWon())
      {
        yield break;
      }
      initPlayerStartLocations();
      initSelectedCharacter();
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
        endWeek();
        while(weekEnding)
        {
          yield return null;
        }
      }

      if(delayFireside)
      {
        yield return new WaitForSeconds(1.0f);
      }
      yield return null;
      ShowFireside(!delayFireside);
      saveCity();
      board.IntroFinished();
      McCoy.GetInstance().Loading = false;
    }

    private bool checkGameWon()
    {
      if (McCoyGameState.Instance().FinalBattle && McCoyGameState.Instance().FinalBossHealth <= 0f)
      {
        gameEnding = true;

        // reinitialize input for this specific odd-ball state where the game is over
        initializeInput();
        endgameCutscene.SetActive(true);
        return true;
      }
      endgameCutscene.SetActive(false);
      return false;
    }

    private void initFiresideScene()
    {
      fireside = Instantiate(firesidePrefab, board.CameraAnchor.transform);
      fireside.transform.localPosition = new Vector3(0f, -22.9f, 12.8f);
      fireside.gameObject.SetActive(false);
    }

    private void TransitionToFireside()
    {
      Debug.Log("show fireside");
      ShowFireside(false);
    }

    private void ShowFireside(bool cameraSnap = false)
    {
      StartCoroutine(lerpFireside(cameraSnap));
    }
    private IEnumerator lerpFireside(bool cameraSnap)
    {
      fireside.gameObject.SetActive(true);
      uiRoot.gameObject.SetActive(false);
      yield return board.Hide(cameraSnap);

      fireside.Refresh(this);
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
      board.showStinger(type);
      yield return new WaitForSeconds(stingerDuration);
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

    public void DebugFindMechanism()
    {
      board.FindMechanism();
      refreshBoardAndPanels();
      board.UpdateNodes();
    }

    private void initSelectedCharacter()
    {
      for(int i = 0; i < PlayerCharacters.Length; ++i)
      {
        if(McCoy.GetInstance().gameState.CanPlayerTakeTurn(PlayerCharacters[i]))
        {
          selectedPlayer = PlayerCharacters[i];
          break;
        }
      }
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
      Board.SelectMapNode(node, null, false);
      Board.ToggleZoom(true);
      Board.SetHighlightedNode(node);
    }

    private void applyCauses()
    {
      foreach(McCoyLobbyingCause.LobbyingCause cause in McCoyGameState.Instance().causesLobbiedFor)
      {
        McCoyLobbyingCauseManager.GetInstance().ApplyCause(cause, this, false);
      }
    }

    public void UnlockZone(string id)
    {
      board.UnlockZone(id);
      refreshBoardAndPanels();
    }

    private void endWeek()
    {
      weekEnding = true;
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
      weekEnding = false;
    }

    private void OnDestroy()
    {
      if (board != null)
      {
        Destroy(board.gameObject);
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

    private void generateInitialBoardState()
    {
      McCoy.GetInstance().gameState.Initialize(board.MapNodes);
      foreach (var mapNode in board.MapNodes)
      {
        mapNode.Mobs = McCoyGameState.Instance().GetMobs(mapNode.NodeID);
      }
    }

    private void loadBoardState()
    {
      foreach (var m in board.MapNodes)
      {
        m.Mobs = McCoy.GetInstance().gameState.GetMobs(m.NodeID);
      }
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
      if(board.NodeWithID(McCoy.GetInstance().gameState.AntikytheraMechanismLocation) == null)
      {
        initializeMechanismLocation();
      }
      MapNode antikytheraMechanismLocation = board.NodeWithID(McCoy.GetInstance().gameState.AntikytheraMechanismLocation);
      foreach (var assetNode in board.MapNodes)
      {
        assetNode.SetMechanismLocation(antikytheraMechanismLocation);
      }

      board.UpdateNodes();
      selectedCharacterChanged();
    }

    private void initializeMechanismLocation()
    {
      MapNode loc = board.MapNodes[Random.Range(0, board.MapNodes.Count)];
      loc.HasMechanism = true;
      McCoy.GetInstance().gameState.AntikytheraMechanismLocation = loc.NodeID;
    }

    private void updateWeekText()
    {
      currentWeekText.text = $"New Moon City\nWeek {McCoy.GetInstance().gameState.Week}: {PlayerName(selectedPlayer)}";
    }
    private void selectedCharacterChanged()
    {
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
      refreshPanels(playerLoc);
    }

    private void refreshPanels(MapNode playerLoc)
    {
      MapNode antikytheraMechanismLocation = board.NodeWithID(McCoy.GetInstance().gameState.AntikytheraMechanismLocation);
      bool mechanismFound = antikytheraMechanismLocation == null ? false : antikytheraMechanismLocation.MechanismFoundHere;

      int minDistanceToMechanism = -1;
      foreach (var v in playerLoc.GetConnectedNodes())
      {
        if (minDistanceToMechanism < 0 || (v as MapNode).DistanceToMechanism < minDistanceToMechanism)
        {
          minDistanceToMechanism = (v as MapNode).DistanceToMechanism;
        }
      }

      destroyZonePanels();

      foreach (var node in board.MapNodes)
      {
        if (!node.Disabled)
        {
          zonePanels[node] = Instantiate(ZonePanelPrefab, cityPanelsRoot);
        }
      }

      initSectionHeaders();

      List<MapNode> sortedNodes = new List<MapNode>(zonePanels.Keys);
      sortedNodes.Sort((x, y) => sortMapNodes(x, y, playerLoc, mechanismFound, minDistanceToMechanism));

      List<MapNode> validConnections = new List<MapNode>();

      int siblingIndex = 0;
      sectionHeaders[0].transform.SetSiblingIndex(siblingIndex++);
      // the first few list items are connected nodes. this flag flips when we encounter the first unconnected node
      bool iteratingThroughConnectedNodes = true;
      GameObject firstNode = null;

      // every node on the board needs a list item on the panel
      foreach (MapNode node in sortedNodes)
      {
        bool isConnected = false;
        // connected nodes need flagged
        foreach (var connection in playerLoc.GetConnectedNodes())
        {
          // if IDs match, and either the mechanism hasn't been found yet, or this node is on a fastest route to the mechanism, it's a valid connection
          if (connection.NodeID == node.NodeID && (!mechanismFound || node.DistanceToMechanism <= minDistanceToMechanism))
          {
            validConnections.Add(connection as MapNode);
            isConnected = true;
          }
        }
        // when we get to the first disconnected node, insert the header for disconnected nodes
        if (iteratingThroughConnectedNodes && !isConnected)
        {
          iteratingThroughConnectedNodes = false;
          sectionHeaders[1].transform.SetSiblingIndex(siblingIndex++);
        }

        // init the node panel
        initNodePanel(antikytheraMechanismLocation, siblingIndex, node, isConnected);
        ++siblingIndex;

        // select the first node in the list
        if (firstNode == null)
        {
          firstNode = zonePanels[node];
        }
      }

      if (selectedZonePanel != null)
      {
        selectedZonePanel.Deselect();
      }
      var firstPanel = firstNode.GetComponent<MapCityNodePanel>();
      selectedZonePanel = firstPanel;
      StartCoroutine(selectZonePanelAfterDelay(playerLoc, validConnections));
    }

    private void initNodePanel(MapNode antikytheraMechanismLocation, int siblingIndex, MapNode node, bool isConnected)
    {
      var nodePanel = zonePanels[node].GetComponent<MapCityNodePanel>();
      nodePanel.Initialize(node, this, antikytheraMechanismLocation);
      zonePanelList.Add(nodePanel.gameObject);
      nodePanel.SetInteractable(isConnected || McCoy.GetInstance().DebugUI);
      nodePanel.transform.SetSiblingIndex(siblingIndex);
    }

    private void destroyZonePanels()
    {
      while (zonePanelList.Count > 0)
      {
        var toDestroy = zonePanelList[0];
        zonePanelList.RemoveAt(0);
        Destroy(toDestroy);
      }
      zonePanels.Clear();
    }

    private IEnumerator selectZonePanelAfterDelay(MapNode playerLoc, List<MapNode> validConnections)
    {
      yield return null;
      selectedZonePanel.Select();
      board.SelectMapNode(playerLoc, validConnections);
      board.SetHighlightedNode(selectedZonePanel.Zone);
    }

    private int sortMapNodes(MapNode x, MapNode y, MapNode playerLoc, bool mechanismFound, int minDistanceToMechanism)
    {
      return MapNode.Compare(x, y, playerLoc, mechanismFound, minDistanceToMechanism);
    }

    private void initPlayerStartLocations()
    {
      if (McCoy.GetInstance().gameState.PlayerLocation(PlayerCharacters[0]) != null)
      {
        return;
      }
      McCoyGameState.Instance().InitPlayerStartingLocations(board.MapNodes);
    }

    public void NextPlayer()
    {
      ChangePlayer(1);
    }

    public void PreviousPlayer()
    {
      ChangePlayer(-1);
    }

    public void ChangePlayer(int direction, bool updateMap = true)
    {
      if (selectedPlayer == PlayerCharacters[PlayerCharacters.Length-1] && direction == 1)
      {
        selectedPlayer = PlayerCharacters[0];
      }
      else if(selectedPlayer == PlayerCharacters[0] && direction == -1)
      {
        selectedPlayer = PlayerCharacters[PlayerCharacters.Length - 1];
      }
      else
      {
        selectedPlayer += direction;
      }
      Debug.Log("City: selected player is now " + selectedPlayer);
      if (updateMap)
      {
        board.ToggleZoom(true);
        selectedCharacterChanged();
      }
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
      McCoyGameState.Instance().FinalBattle = node.MechanismFoundHere;

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
        McCoy.GetInstance().LoadBrawlerStage(stageDataToLoad, selectedPlayer);
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
        yield return new WaitForSeconds(1.0f);
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