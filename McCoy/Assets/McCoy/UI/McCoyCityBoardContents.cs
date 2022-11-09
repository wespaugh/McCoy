using Assets.McCoy.BoardGame;
using Assets.McCoy.RPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.UI
{
  public class McCoyCityBoardContents : MonoBehaviour
  {
    Dictionary<string, MapNode> mapNodeLookup = new Dictionary<string, MapNode>();
    Dictionary<string, GameObject> cityZoneLookup = new Dictionary<string, GameObject>();
    Dictionary<string, McCoyZoneMapMobIndicator> mobIndicatorLookup = new Dictionary<string, McCoyZoneMapMobIndicator>();

    McCoyZoneMapMobIndicator selectedPlayerZone = null;
    McCoyZoneMapMobIndicator highlightedZone = null;

    List<MapNode> mapNodes = new List<MapNode>();
    Dictionary<string, LineRenderer> lineLookup = new Dictionary<string, LineRenderer>();

    // mob routing caches
    Dictionary<MapNode, List<McCoyMobData>> toRoute = new Dictionary<MapNode, List<McCoyMobData>>();
    int mobsMoving = 0;

    [SerializeField]
    GameObject cameraAnchor = null;

    [SerializeField]
    const float CAMERA_CITY_FIELD_OF_VIEW = 37.0f;

    [SerializeField]
    float lineStartWidth = .25f;
    [SerializeField]
    float lineEndWidth = .05f;

    [SerializeField]
    MeshRenderer map = null;

    [SerializeField]
    Sprite mapTexture = null;

    [SerializeField]
    bool OnlyStrongestMobsSearch = false;

    [SerializeField]
    GameObject stingerPrefab = null;

    [SerializeField]
    Transform stingerTransformRoot = null;

    [SerializeField]
    AudioClip mobCombatSound = null;

    List<LineRenderer> inactiveConnectionLines = new List<LineRenderer>();
    bool showUnnecessaryLines = false;

    Action weekendFinishedCallback = null;

    public List<MapNode> MapNodes
    {
      get
      {
        if(mapCache == null)
        {
          initMapCache();
        }
        return mapNodes;
      }
    }

    MapGraphNodeContainer _mapCache;

    MapGraphNodeContainer mapCache
    {
      get
      {
        if(_mapCache == null)
        {
          initMapCache();
        }
        return _mapCache;
      }
    }

    [SerializeField]
    GameObject NodeParent;

    [SerializeField]
    string dataFile;

    [SerializeField]
    Transform Scaler = null;
    private GameObject selectedNode;

    private Vector3 cameraOrigin;
    private Vector3 cameraDestination;
    private bool lerpingCamera;
    private float cameraStartTime;
    private bool zoomed = false;

    private void OnDestroy()
    {
      _mapCache = null;
      mapNodes.Clear();
    }

    public void SetHoverNode(MapNode node)
    {
      if(highlightedZone != null && highlightedZone != selectedPlayerZone)
      {
        highlightedZone.SetSelected(false);
      }

      if(node == null)
      {
        highlightedZone = null;
        return;
      }

      highlightedZone = mobIndicatorLookup[node.NodeID];
      highlightedZone.SetSelected(true);
    }

    public Vector3 NodePosition(MapNode node)
    {
      return cityZoneLookup[node.NodeID].transform.position;
    }

    public void ToggleLines()
    {
      showUnnecessaryLines = !showUnnecessaryLines;
      updateUnnecessaryLinesState();
    }

    public void ToggleZoom()
    {
      ToggleZoom(!zoomed);
    }

    public void ToggleZoom(bool isZoomed)
    {
      if (zoomed != isZoomed)
      {
        zoomed = isZoomed;
        if (zoomed)
        {
          if(selectedNode == null)
          {
            zoomed = false;
            return;
          }
          centerCameraOnNode(selectedNode);
        }
        else
        {
          cameraOrigin = Camera.main.transform.position;
          cameraDestination = cameraAnchor.transform.position;
          cameraStartTime = Time.time;
          if (!lerpingCamera)
          {
            StartCoroutine(LerpCamera(0.5f));
          }
        }
      }
    }

    public void showStinger(McCoyStinger.StingerTypes stingerType)
    {
      McCoyStinger stinger = Instantiate(stingerPrefab, stingerTransformRoot).GetComponent<McCoyStinger>();
      stinger.RunStinger(stingerType);
    }

    #region Weekend
    public void Weekend(Action callback)
    {
      weekendFinishedCallback = callback;
      StartCoroutine(runWeekend());
    }
    private IEnumerator runWeekend()
    {
      float startTime = Time.time;
      while (Time.time < startTime + 1.5f)
      {
        yield return null;
      }
      toRoute.Clear();

      bool playCombatSound = false;

      foreach(MapNode node in mapNodes)
      {
        List<McCoyMobData> mobsDefeated = new List<McCoyMobData>();
        for(int i = 0; i < node.Mobs.Count; ++i)
        {
          bool combat = false;
          for(int j = i+1; j < node.Mobs.Count; ++j)
          {
            combat = true;
            playCombatSound = true;
            node.Mobs[i].OffscreenCombat(node.Mobs[j].StrengthForXP());
            AnimateMobCombat(node, node.Mobs[i].Faction);
            node.Mobs[j].OffscreenCombat(node.Mobs[i].StrengthForXP());
            AnimateMobCombat(node, node.Mobs[j].Faction);
            if(node.Mobs[i].IsRouted)
            {
              mobsDefeated.Add(node.Mobs[i]);
            }
            if(node.Mobs[j].IsRouted)
            {
              mobsDefeated.Add(node.Mobs[j]);
            }
          }
          node.Mobs[i].WeekEnded(combat);
        }
        if(mobsDefeated.Count > 0)
        {
          toRoute.Add(node, mobsDefeated);
        }
      }

      if(playCombatSound)
      {
        UFE.PlaySound(mobCombatSound);
      }

      StartCoroutine(waitForWeekendMobConflictsAnim(0.5f));
    }

    private IEnumerator waitForWeekendMobConflictsAnim(float animationTime)
    {
      float startTime = Time.time;
      while(Time.time < startTime + animationTime)
      {
        yield return null;
      }

      if(toRoute.Count == 0)
      {
        mobsMoving = 1;
        WeekendMobRouted();
      }
      foreach (var route in toRoute)
      {
        foreach (var mob in route.Value)
        {
          List<SearchableNode> connections = route.Key.GetConnectedNodes();
          MapNode conn = null;
          while (connections.Count > 0)
          {
            int idx = UnityEngine.Random.Range(0, connections.Count);
            conn = (connections[idx]) as MapNode;
            connections.RemoveAt(idx);
            if (mob.CanRouteTo(conn))
            {
              break;
            }
            conn = null;
          }
          if (conn == null)
          {
            Debug.Log($"couldn't find connection for {mob.Faction}. Ending routing");
            mob.FinishedRouting();
          }
          else
          {
            route.Key.Mobs.Remove(mob);

            McCoyMobData existingMob = null;
            foreach (var mobAtConnection in conn.Mobs)
            {
              if (mobAtConnection.Faction == mob.Faction)
              {
                existingMob = mobAtConnection;
                break;
              }
            }
            if (existingMob == null)
            {
              conn.Mobs.Add(mob);
            }
            else
            {
              if (existingMob.XP > mob.XP)
              {
                existingMob.Absorb(mob);
              }
              else
              {
                conn.Mobs.Remove(existingMob);
                mob.Absorb(existingMob);
                conn.Mobs.Add(mob);
              }
            }
            ++mobsMoving;
            // mob.FinishedRouting();
            AnimateMobMove(mob.Faction, route.Key, conn, 1.0f, WeekendMobRouted);
          }
        }
      }
      if(mobsMoving == 0)
      {
        VoluntaryMovementPhase();
      }
    }

    private void WeekendMobRouted()
    {
      --mobsMoving;
      if(mobsMoving == 0)
      {
        VoluntaryMovementPhase();
      }
    }

    private void VoluntaryMovementPhase()
    {
      if (mobsMoving == 0)
      {
        List<Tuple<McCoyMobData, MapNode>> mobsThatCanStillMove = new List<Tuple<McCoyMobData, MapNode>>();

        foreach(var mapNode in mapNodes)
        {
          foreach(McCoyMobData m in mapNode.Mobs)
          {
            if(!m.IsRouted)
            {
              mobsThatCanStillMove.Add(new Tuple<McCoyMobData, MapNode>(m, mapNode));
            }
          }
        }

        while(mobsThatCanStillMove.Count > 0)
        {
          int idx = UnityEngine.Random.Range(0, mobsThatCanStillMove.Count);
          Tuple<McCoyMobData, MapNode> nodePair = mobsThatCanStillMove[idx];
          mobsThatCanStillMove.RemoveAt(idx);
          decideMobMove(nodePair);
        }

        if(mobsMoving == 0)
        {
          finishWeekEnd();
        }
      }
    }

    private void decideMobMove(Tuple<McCoyMobData, MapNode> nodePair)
    {
      MapNode moveTarget = null;
      McCoyMobData moveSubject = nodePair.Item1;

      foreach(var connection in nodePair.Item2.GetConnectedNodes())
      {
        MapNode neighbor = connection as MapNode;
        // if there's an adjacent place we can divide into, divide into
        if(neighbor.Mobs.Count == 0 && nodePair.Item1.XP >= 8)
        {
          moveTarget = neighbor;
          moveSubject = nodePair.Item1.Split();
          break;
        }
        bool shouldMove = true;
        foreach (var mob in neighbor.Mobs)
        {
          // if there's already a mob of the same faction or if there is a faction too strong to challenge, bail
          if(mob.Faction == nodePair.Item1.Faction || mob.XP * 2 >= nodePair.Item1.XP)
          {
            shouldMove = false;
            break;
          }
        }
        if(shouldMove)
        {
          moveTarget = neighbor;
          break;
        }
      }
      if(moveTarget != null)
      {
        ++mobsMoving;
        bool hideOriginal = false;
        // if the mob is moving (rather than being created from a split)
        if(moveSubject == nodePair.Item1)
        {
          nodePair.Item2.Mobs.Remove(moveSubject);
          hideOriginal = true;
        }
        moveTarget.Mobs.Add(moveSubject);
        AnimateMobMove(moveSubject.Faction, nodePair.Item2, moveTarget, .5f, voluntaryMoveFinished,hideOriginal);
      }
    }

    private void voluntaryMoveFinished()
    {
      --mobsMoving;
      if(mobsMoving == 0)
      {
        finishWeekEnd();
      }
    }

    private void finishWeekEnd()
    {
      foreach(var m in MapNodes)
      {
        foreach(var mob in m.Mobs)
        {
          if(mob.IsRouted)
          {
            mob.FinishedRouting();
          }
        }
      }

      advanceDoomsdayClock();
      UpdateNodes();
      if (weekendFinishedCallback != null)
      {
        weekendFinishedCallback();
      }
    }

    private void advanceDoomsdayClock()
    {
      foreach(var node in mapNodes)
      {
        McCoyMobData strongestMob = null;
        foreach(var mob in node.Mobs)
        {
          if(strongestMob == null || mob.XP > strongestMob.XP)
          {
            strongestMob = mob;
          }
          if(!OnlyStrongestMobsSearch)
          {
            node.Search(mob.XP, (int)mob.Health);
          }
        }
        if(strongestMob == null)
        {
          continue;
        }
        if (OnlyStrongestMobsSearch)
        {
          node.Search(strongestMob.XP, (int)strongestMob.Health);
        }
      }
      mapNodes.Sort((a, b) => { return b.SearchStatus() - a.SearchStatus(); });
      Debug.Log("most searched area is " + mapNodes[0].ZoneName + ", " + mapNodes[0].SearchStatus());
    }

    public void AnimateMobMove(Factions f, MapNode start, MapNode end, float time, Action finishedCallback, bool hideOriginal = true)
    {
      var mobIndicator = mobIndicatorLookup[start.NodeID];
      mobIndicator.AnimateFaction(f, OffsetBetweenNodes(start, end), time, finishedCallback, hideOriginal);
    }

    public Vector3 OffsetBetweenNodes(MapNode start, MapNode end)
    {
      return cityZoneLookup[end.NodeID].transform.position - cityZoneLookup[start.NodeID].transform.position;
    }
    #endregion

    private void initMapCache()
    {
      if(_mapCache != null)
      {
        return;
      }
      var mapCacheObj = Resources.Load($"{ProjectConstants.MAPDATA_DIRECTORY}/{dataFile}");
      _mapCache = mapCacheObj as MapGraphNodeContainer;

      foreach (var assetNode in mapCache.NodeData)
      {
        mapNodes.Add(assetNode);
        assetNode.connectionIDs.Clear();
        assetNode.Mobs.Clear();
        mapNodeLookup.Add(assetNode.NodeID, assetNode);
        assetNode.SearchData.ZoneName = assetNode.ZoneName;
        assetNode.SearchData.NodeID = assetNode.NodeID;
      }

      Debug.Log("There are " + mapCache.NodeLinks.Count + " node links");
      foreach(var assetLink in mapCache.NodeLinks)
      {
        var baseNode = mapNodeLookup[assetLink.BaseNodeGuid];
        var targetNode = mapNodeLookup[assetLink.TargetNodeGuid];

        var nameComps = baseNode.ZoneName.Split(".");
        string baseName = nameComps[nameComps.Length - 1];
        nameComps = targetNode.ZoneName.Split(".");
        string targetName = nameComps[nameComps.Length - 1];
        

        if(baseName.Contains("nimbystreet") || targetName.Contains("nimbystreet"))
        {
          Debug.Log($"Link Between {baseName}-{targetName}");
        }
        if (baseName.Contains("governmentofficebuilding") || targetName.Contains("governmentofficebuilding"))
        {
          Debug.Log($"Link Between {baseName}-{targetName}");
        }

        // weird thing here. Resources.Load seems to be cacheing the loaded objects,
        // meaning making a second board will already have map nodes connected. we can skip that part, if so
        // base is connected to target
        if (!baseNode.ConnectedToNode(targetNode))
        {
          baseNode.AddConnectedNode(targetNode);
        }
        // target is connected to base
        if (!targetNode.ConnectedToNode(baseNode))
        {
          targetNode.AddConnectedNode(baseNode);
        }
      }
    }

    private void Awake()
    {
      Camera.main.transform.localPosition = cameraAnchor.transform.localPosition;
      Camera.main.transform.rotation = cameraAnchor.transform.rotation;
      Camera.main.fieldOfView = CAMERA_CITY_FIELD_OF_VIEW;

      map.materials[0].mainTexture = mapTexture.texture;

      McCoyCityZonePlacementNode[] nodes = NodeParent.GetComponentsInChildren<McCoyCityZonePlacementNode>();
      foreach(var node in nodes)
      {
        cityZoneLookup[node.NodeId] = node.gameObject;
        // Debug.Log($"{node.NodeId}: " + node.gameObject.name);
        mobIndicatorLookup[node.NodeId] = node.gameObject.GetComponentInChildren<McCoyZoneMapMobIndicator>();
      }
      initConnectionLines(nodes);
    }


    private void initConnectionLines(McCoyCityZonePlacementNode[] nodes)
    {
      GameObject connections = new GameObject();
      connections.transform.SetParent(transform);
      Material lineMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
      bool log = false;
      foreach (var assetLink in mapCache.NodeLinks)
      {
        var fromID = assetLink.BaseNodeGuid;
        List<McCoyCityZonePlacementNode> sourceNodes = nodes.Where(x => x.NodeId == fromID).ToList();
        int fromIdx = -1;
        for (int i = 0; i < nodes.Length; ++i) if (nodes[i] == sourceNodes[0]) { fromIdx = i; break; }
        if (sourceNodes == null || sourceNodes.Count == 0) Debug.LogWarning("Found no source node connected to link for ID" + fromID);

        var toID = assetLink.TargetNodeGuid;
        List<McCoyCityZonePlacementNode> destNodes = nodes.Where(x => x.NodeId == toID).ToList();
        int toIdx = -1;
        for (int i = 0; i < nodes.Length; ++i) if (nodes[i] == destNodes[0]) { toIdx = i; break; }
        if (sourceNodes == null || destNodes.Count == 0) Debug.LogWarning("Found no dest node connected to link for ID" + toID);

        var line = new GameObject();
        line.transform.SetParent(connections.transform);
        line.name = $"Line {fromIdx}::{toIdx}";

        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineStartWidth;
        lineRenderer.endWidth = lineEndWidth;
        lineRenderer.material = lineMaterial;

        DrawConnections(lineRenderer, sourceNodes[0].transform.position, destNodes[0].transform.position, log);
        log = false;
        lineLookup[$"{sourceNodes[0].NodeId}{destNodes[0].NodeId}"] = lineRenderer;
      }
    }

    void DrawConnections(LineRenderer lineRenderer, Vector3 p0, Vector3 p2, bool log = false)
    {
      List<Vector3> positions = new List<Vector3>();
      if(log)
      {
        Debug.Log($"drawing line from {p0} to {p2}");
      }
      float segmentCount = 20.0f;
      for (int i = 0; i <= segmentCount; i++)
      {
        // float t = i / segmentCount;
        Vector3 p1 = CalculateArcMidpointBetweenPoints(p0, p2);

        positions.Add(CalculateQuadraticBezierPoint(((float)i)/segmentCount, p0, p1, p2));
        if(log)
        {
          Debug.Log($"Adding position: " + positions[i]);
        }
      }
      lineRenderer.positionCount = positions.Count;
      lineRenderer.SetPositions(positions.ToArray());
    }

    /*
    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
      // (1 - t)2P0 + 2(1 - t)tP1 + t2P2 , 0 < t < 1
      float inverseTime = 1 - t;
      Vector3 retVal = inverseTime * inverseTime * p0;
      retVal = retVal + (2 * inverseTime * t * p1);
      retVal = retVal + t * t * p2;
      return retVal;
    }

    Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
      float u = 1 - t;
      float tt = t * t;
      float uu = u * u;
      float uuu = uu * u;
      float ttt = tt * t;

      Vector3 p = uuu * p0;
      p += 3 * uu * t * p1;
      p += 3 * u * tt * p2;
      p += ttt * p3;

      return p;
    }
    */

    public void AnimateMobCombat(MapNode location, ProjectConstants.Factions faction)
    {
      mobIndicatorLookup[location.NodeID].AnimateCombat(faction);
    }

    public void LoadQuests(List<McCoyQuestData> currentQuests)
    {
      foreach(var quest in currentQuests)
      {
        mobIndicatorLookup[quest.possibleLocations[0]].ShowQuest(quest);
      }
    }

    private void centerCameraOnNode(GameObject node)
    {
      Vector3 locPosition = selectedNode.transform.localPosition;// NodePosition(m);
      cameraDestination = new Vector3(Mathf.Clamp(locPosition.x+3f, 6.5f, 24), 19, Mathf.Clamp(locPosition.z + 18f, 36, 45));
      cameraOrigin = Camera.main.transform.position;
      cameraStartTime = Time.time;
      if (!lerpingCamera)
      {
        StartCoroutine(LerpCamera(.5f));
      }
    }


    public void SelectMapNode(MapNode m, List<MapNode> validConnections, bool refreshLines = true)
    {
      if(selectedPlayerZone != null && highlightedZone != selectedPlayerZone)
      {
        selectedPlayerZone.SetSelected(false);
      }
      if (m == null)
      {
        selectedNode = null;
        selectedPlayerZone = null;
      }
      else
      {
        selectedNode = cityZoneLookup[m.NodeID];
        selectedPlayerZone = mobIndicatorLookup[m.NodeID];
        selectedPlayerZone.SetSelected(true);

        if (zoomed)
        {
          centerCameraOnNode(selectedNode);
        }
      }

      if(!refreshLines)
      {
        return;
      }

      inactiveConnectionLines.Clear();
      foreach (var entry in lineLookup)
      {
        bool isSelectedNow = m == null ? false : entry.Key.Contains(m.NodeID);

        if (isSelectedNow && validConnections != null)
        {
          bool foundOtherEnd = false;
          foreach (var c in validConnections)
          {
            if(entry.Key.Contains(c.NodeID))
            {
              foundOtherEnd = true;
              break;
            }
          }
          isSelectedNow &= foundOtherEnd;
        }
        Color deselectColor = new Color(227f/255f, 99f/255f, 151f/255f, 0f);
        Color selectColor = new Color(130f / 255f, 209f / 255f, 115f / 255f, 128f/255f);
        if(!isSelectedNow)
        {
          inactiveConnectionLines.Add(entry.Value);
        }
        entry.Value.startColor = isSelectedNow ? selectColor : deselectColor;// Color.grey;
        entry.Value.endColor = isSelectedNow ? selectColor : deselectColor; // Color.grey;
      }

      updateUnnecessaryLinesState();
    }

    private void updateUnnecessaryLinesState()
    {
      foreach (var entry in lineLookup)
      {
        entry.Value.gameObject.SetActive(showUnnecessaryLines || !inactiveConnectionLines.Contains(entry.Value));
      }
    }

    private IEnumerator LerpCamera(float travelTime)
    {
      lerpingCamera = true;
      while (Camera.main.transform.position != cameraDestination)
      {
        float currentTime = ((Time.time - cameraStartTime) / travelTime);
        currentTime = 1f - (float)Math.Pow(1f - currentTime, 3f);
        Camera.main.transform.position = new Vector3(
          Mathf.Lerp(cameraOrigin.x, cameraDestination.x, currentTime),
          Mathf.Lerp(cameraOrigin.y, cameraDestination.y, currentTime), 
          Mathf.Lerp(cameraOrigin.z, cameraDestination.z, currentTime));
        yield return null;
      }
      lerpingCamera = false;
    }

    public MapNode NodeWithID(string ID)
    {
      if(string.IsNullOrEmpty(ID) || !mapNodeLookup.ContainsKey(ID))
      {
        return null;
      }
      return mapNodeLookup[ID];
    }

    public void UpdateNodes()
    {
      Dictionary<string, int> playerLocs = new Dictionary<string, int>();
      for(int i = 0; i < PlayerCharacters.Length; ++i)
      {
        playerLocs[McCoy.GetInstance().gameState.PlayerLocation(PlayerCharacters[i])] = i+1;
      }
      foreach(var node in mapNodes)
      {
        int playerNum = playerLocs.ContainsKey(node.NodeID) ? playerLocs[node.NodeID] : -1;
        MapNode mechanismLocation = NodeWithID(McCoy.GetInstance().gameState.AntikytheraMechanismLocation);
        bool showMechanism = mechanismLocation != null && mechanismLocation.MechanismFoundHere && mechanismLocation.NodeID == node.NodeID;
        mobIndicatorLookup[node.NodeID].UpdateWithMobs(node.Mobs, playerNum, node.ZoneName, node.SearchPercent, showMechanism);
      }
    }
  }
}