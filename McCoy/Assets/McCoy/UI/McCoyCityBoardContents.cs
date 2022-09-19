using Assets.McCoy.BoardGame;
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

    private void OnDestroy()
    {
      _mapCache = null;
      mapNodes.Clear();
    }

    public void SetHoverNode(MapNode node)
    {
      if(highlightedZone != null && highlightedZone != selectedPlayerZone)
      {
        highlightedZone.ToggleHover(false);
      }

      if(node == null)
      {
        highlightedZone = null;
        return;
      }

      highlightedZone = mobIndicatorLookup[node.NodeID];
      highlightedZone.ToggleHover(true);
    }

    public Vector3 NodePosition(MapNode node)
    {
      return cityZoneLookup[node.NodeID].transform.position;
    }

    internal void ToggleLines()
    {
      showUnnecessaryLines = !showUnnecessaryLines;
      updateUnnecessaryLinesState();
    }

    public void Weekend(Action callback)
    {
      this.weekendFinishedCallback = callback;
      toRoute.Clear();
      foreach(MapNode node in mapNodes)
      {
        List<McCoyMobData> mobsDefeated = new List<McCoyMobData>();
        for(int i = 0; i < node.Mobs.Count; ++i)
        {
          bool combat = false;
          for(int j = i+1; j < node.Mobs.Count; ++j)
          {
            combat = true;
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
          List<SearchableNode> connections = route.Key.connectedNodes;
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
            mob.FinishedRouting();
            AnimateMobMove(mob.Faction, route.Key, conn, 1.0f, WeekendMobRouted);
          }
        }
      }
    }

    private void WeekendMobRouted()
    {
      --mobsMoving;
      if(mobsMoving == 0)
      {
        advanceDoomsdayClock();
        UpdateNodes();
        if(weekendFinishedCallback != null)
        {
          weekendFinishedCallback();
        }
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
        mapNodeLookup.Add(assetNode.NodeID, assetNode);
      }

      foreach(var assetLink in mapCache.NodeLinks)
      {
        var baseNode = mapNodeLookup[assetLink.BaseNodeGuid];
        var targetNode = mapNodeLookup[assetLink.TargetNodeGuid];

        // weird thing here. Resources.Load seems to be cacheing the loaded objects,
        // meaning making a second board will already have map nodes connected. we can skip that part, if so
        // base is connected to target
        if (!baseNode.connectedNodes.Contains(targetNode))
        {
          baseNode.connectedNodes.Add(targetNode);
        }
        // target is connected to base
        if (!targetNode.connectedNodes.Contains(baseNode))
        {
          targetNode.connectedNodes.Add(baseNode);
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

    public void SelectMapNode(MapNode m, List<MapNode> validConnections)
    {
      if(selectedPlayerZone != null && highlightedZone != selectedPlayerZone)
      {
        selectedPlayerZone.ToggleHover(false);
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
        selectedPlayerZone.ToggleHover(true);

        Vector3 playerLocPosition = selectedNode.transform.localPosition;// NodePosition(m);
        cameraDestination = new Vector3(Mathf.Clamp(playerLocPosition.x, 6.5f, 21), 18, Mathf.Clamp(playerLocPosition.z + 18f, 39, 45));
        cameraOrigin = Camera.main.transform.position;
        cameraStartTime = Time.time;
        if (!lerpingCamera)
        {
          StartCoroutine(LerpCamera(.5f));
        }
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
        Color deselectColor = new Color(227f/255f, 99f/255f, 151f/255f);
        Color selectColor = new Color(130f / 255f, 209f / 255f, 115f / 255f);
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

    public void UpdateNodes()
    {
      Dictionary<MapNode, int> playerLocs = new Dictionary<MapNode, int>();
      for(int i = 1; i <= ProjectConstants.NUM_BOARDGAME_PLAYERS; ++i)
      {
        playerLocs[McCoy.GetInstance().boardGameState.PlayerLocation(i)] = i;
      }
      foreach(var node in mapNodes)
      {
        int playerNum = playerLocs.ContainsKey(node) ? playerLocs[node] : -1;
        MapNode mechanismLocation = McCoy.GetInstance().boardGameState.AntikytheraMechanismLocation;
        bool showMechanism = mechanismLocation != null && mechanismLocation.SearchStatus() == SearchState.CompletelySearched && mechanismLocation.NodeID == node.NodeID;
        mobIndicatorLookup[node.NodeID].UpdateWithMobs(node.Mobs, playerNum, node.ZoneName, node.SearchPercent, showMechanism);
      }
    }
  }
}