using Assets.McCoy.BoardGame;
using System;
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

    public void AnimateMobMove(Factions f, MapNode start, MapNode end, float time, Action finishedCallback, bool resetAtEnd = true)
    {
      var mobIndicator = mobIndicatorLookup[start.NodeID];
      mobIndicator.AnimateFaction(f, OffsetBetweenNodes(start, end), time, finishedCallback, resetAtEnd);
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
        lineRenderer.startColor = Color.black;// Color.blue;
        lineRenderer.endColor = Color.black; // Color.green;
        lineRenderer.material = lineMaterial;
        List<Vector3> positions = new List<Vector3>();
        positions.Add(new Vector3(sourceNodes[0].transform.position.x, sourceNodes[0].transform.position.y /* * Scaler.localScale.y*/, sourceNodes[0].transform.position.z));
        positions.Add(new Vector3(destNodes[0].transform.position.x/* * Scaler.localScale.x*/, destNodes[0].transform.position.y /** Scaler.localScale.y*/, destNodes[0].transform.position.z));
        lineRenderer.SetPositions(positions.ToArray());

        lineLookup[$"{sourceNodes[0].NodeId}{destNodes[0].NodeId}"] = lineRenderer;
      }
    }

    public void AnimateMobCombat(MapNode location, ProjectConstants.Factions faction)
    {
      mobIndicatorLookup[location.NodeID].AnimateCombat(faction);
    }

    public void SelectMapNode(MapNode m)
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
      }

      foreach (var entry in lineLookup)
      {
        bool isSelectedNow = m == null ? false : entry.Key.Contains(m.NodeID);
        entry.Value.startColor = isSelectedNow ? Color.green : Color.black;
        entry.Value.endColor = isSelectedNow ? Color.green : Color.black;
      }
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
        mobIndicatorLookup[node.NodeID].UpdateWithMobs(node.Mobs, playerNum, node.ZoneName);
      }
    }
  }
}