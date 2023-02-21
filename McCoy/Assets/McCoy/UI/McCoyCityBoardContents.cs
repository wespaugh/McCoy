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
    [SerializeField]
    GameObject cameraAnchor = null;

    [SerializeField]
    Transform cameraBoundsLowerLeft = null;

    [SerializeField]
    Transform cameraBoundsUpperRight = null;

    [SerializeField]
    const float cameraCityFieldOfView = 37.0f;
    [SerializeField]
    Vector3 selectedNodeCameraZoomOffset = new Vector3(3.5f, 0f, 0f);
    [SerializeField]
    float lineStartWidth = 1f;
    [SerializeField]
    float lineEndWidth = 1f;
    [SerializeField]
    int lineSortOrder = 100;

    [SerializeField]
    MeshRenderer map = null;

    [SerializeField]
    Sprite mapTexture = null;

    [SerializeField]
    float cameraMoveTime = 0.5f;

    [SerializeField]
    GameObject stingerPrefab = null;

    [SerializeField]
    Transform stingerTransformRoot = null;

    [SerializeField]
    Vector3 hidePosition = new Vector3(0, 15, 18);

    [SerializeField]
    string lineMaterialShader = "Sprites/Default"; //"Universal Render Pipeline/2D/Sprite-Lit-Default"));

    [SerializeField]
    GameObject NodeParent;

    [SerializeField]
    string dataFile;

    [SerializeField]
    Transform Scaler = null;

    [SerializeField]
    private Color unconnectedLineColor = new Color(227f / 255f, 99f / 255f, 151f / 255f, 128f);
    [SerializeField]
    private Color connectedLineColor = new Color(130f / 255f, 209f / 255f, 115f / 255f, 255f / 255f);

    [SerializeField]
    private McCoyMobMovementLogic mobMovementLogic;

    [SerializeField]
    bool log = false;

    Dictionary<string, MapNode> mapNodeLookup = new Dictionary<string, MapNode>();
    Dictionary<string, GameObject> cityZoneLookup = new Dictionary<string, GameObject>();
    Dictionary<string, McCoyZoneMapMobIndicator> mobIndicatorLookup = new Dictionary<string, McCoyZoneMapMobIndicator>();

    McCoyZoneMapMobIndicator selectedPlayerZone = null;
    McCoyZoneMapMobIndicator highlightedZone = null;

    List<MapNode> mapNodes = new List<MapNode>();
    Dictionary<string, LineRenderer> lineLookup = new Dictionary<string, LineRenderer>();
    public Transform CameraAnchor => cameraAnchor.transform;

    private GameObject selectedNode;

    private Vector3 cameraOrigin;
    private Vector3 cameraDestination;
    private bool lerpingCamera;
    private float cameraStartTime;
    private bool zoomed = false;
    private bool hidden = false;
    
    List<LineRenderer> inactiveConnectionLines = new List<LineRenderer>();
    bool showUnconnectedLines = false;
    bool runningIntro = true;


    public List<MapNode> MapNodes
    {
      get
      {
        if(mapAssetData == null)
        {
          initMapCache();
        }
        return mapNodes;
      }
    }

    MapGraphNodeContainer _mapAssetData;

    MapGraphNodeContainer mapAssetData
    {
      get
      {
        if(_mapAssetData == null)
        {
          initMapCache();
        }
        return _mapAssetData;
      }
    }
    private void Awake()
    {
      Camera.main.transform.localPosition = cameraAnchor.transform.localPosition;
      Camera.main.transform.rotation = cameraAnchor.transform.rotation;
      Camera.main.fieldOfView = cameraCityFieldOfView;

      map.materials[0].mainTexture = mapTexture.texture;

      McCoyCityZonePlacementNode[] nodes = NodeParent.GetComponentsInChildren<McCoyCityZonePlacementNode>();
      // cache out node data for easy lookup
      foreach (var node in nodes)
      {
        cityZoneLookup[node.NodeId] = node.gameObject;
        mobIndicatorLookup[node.NodeId] = node.gameObject.GetComponentInChildren<McCoyZoneMapMobIndicator>();
      }
      initConnectionLines(nodes);
      mobMovementLogic.Initialize(this);
    }

    private void OnDestroy()
    {
      _mapAssetData = null;
      mapNodes.Clear();
    }

    public string NameForNode(string id)
    {
      return mapNodeLookup[id].ZoneName;
    }

    public void SetHighlightedNode(MapNode node)
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
      if(runningIntro)
      {
        return;
      }
      showUnconnectedLines = !showUnconnectedLines;
      updateUnconnectedLinesState();
    }

    public void ToggleZoom()
    {
      ToggleZoom(!zoomed);
    }

    public void ToggleZoom(bool isZoomed)
    {
      if (runningIntro)
      {
        return;
      }
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
          centerCameraOnSelectedNode();
        }
        else
        {
          cameraOrigin = Camera.main.transform.position;
          cameraDestination = cameraAnchor.transform.position;
          cameraStartTime = Time.time;
          if (!lerpingCamera)
          {
            StartCoroutine(LerpCamera(cameraMoveTime));
          }
        }
      }
    }

    public IEnumerator Hide(bool cameraSnap = false)
    {
      if(hidden)
      {
        Debug.Log("alredy hdidenb, returnging");
        yield break;
      }


      zoomed = false;
      Camera.main.transform.position = cameraAnchor.transform.position;
      Camera.main.transform.rotation = cameraAnchor.transform.rotation;
      cameraOrigin = Camera.main.transform.position;
      cameraDestination = cameraAnchor.transform.localPosition;
      LerpCamera(0.5f);
      if(cameraSnap)
      {
        StartCoroutine(lerpBoard(hidePosition, true, 0.1f));
      }
      else
      {
        StartCoroutine(lerpBoard(hidePosition, true, cameraMoveTime));
      }
    }

    public bool Show()
    {
      if(!hidden)
      {
        return false;
      }
      hidden = false;
      // StartCoroutine(lerpBoard(Vector3.zero, false));
      centerCameraOnSelectedNode();
      return true;
    }
    private IEnumerator lerpBoard(Vector3 target, bool hiding, float travelTime = .6f)
    {
      Debug.Log("lerp board: " + hiding);
      Vector3 origin = transform.position;
      if (travelTime == 0f)
      {
        yield return null;
        transform.position = target;
      }
      else
      {
        float startTime = Time.time;
        while (transform.position != target)
        {
          float currentTime = ((Time.time - startTime) / travelTime);
          currentTime = 1f - (float)Math.Pow(1f - currentTime, 3f);
          transform.position = new Vector3(
            Mathf.Lerp(origin.x, target.x, currentTime),
            Mathf.Lerp(origin.y, target.y, currentTime),
            Mathf.Lerp(origin.z, target.z, currentTime));
          yield return null;
        }
      }
      Debug.Log("Setting hidden to: " + hiding);
      hidden = hiding;
    }

    public void showStinger(McCoyStinger.StingerTypes stingerType)
    {
      McCoyStinger stinger = Instantiate(stingerPrefab, stingerTransformRoot).GetComponent<McCoyStinger>();
      stinger.RunStinger(stingerType);
    }

    public void IntroFinished()
    {
      runningIntro = false;
    }

    public void Weekend(Action callback)
    {
      mobMovementLogic.Weekend(callback);
    }


    // Debug function only. Force mechanism location to be found through normal search means (repeated a lot of times)
    public void FindMechanism()
    {
      foreach (var m in MapNodes)
      {
        if (m.MechanismFoundHere)
        {
          m.Search(100, 100);
        }
      }
      mapNodes.Sort((a, b) => { return b.SearchStatus() - a.SearchStatus(); });
    }

    // load map data from resource file
    private void initMapCache()
    {
      if(_mapAssetData != null)
      {
        return;
      }
      var mapCacheObj = Resources.Load($"{MAPDATA_DIRECTORY}/{dataFile}");
      _mapAssetData = mapCacheObj as MapGraphNodeContainer;

      foreach (var assetNodeData in mapAssetData.NodeData)
      {
        assetNodeData.ClearConnectedNodes();
        MapNode assetNode = (MapNode)assetNodeData.Clone();
        mapNodes.Add(assetNode);
        assetNode.connectionIDs.Clear();
        mapNodeLookup.Add(assetNode.NodeID, assetNode);
        assetNode.SearchData.ZoneName = assetNode.ZoneName;
        assetNode.SearchData.NodeID = assetNode.NodeID;
      }

      foreach(var assetLink in mapAssetData.NodeLinks)
      {
        var baseNode = mapNodeLookup[assetLink.BaseNodeGuid];
        var targetNode = mapNodeLookup[assetLink.TargetNodeGuid];
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

    private void redrawLines()
    {
      McCoyCityZonePlacementNode[] nodes = NodeParent.GetComponentsInChildren<McCoyCityZonePlacementNode>();
      foreach(var key in lineLookup.Keys)
      {
        LineRenderer r = lineLookup[key];
        Destroy(r.gameObject);
      }
      initConnectionLines(nodes);
    }

    private void initConnectionLines(McCoyCityZonePlacementNode[] nodesArray)
    {
      List<McCoyCityZonePlacementNode> nodes = new List<McCoyCityZonePlacementNode>(nodesArray);
      GameObject connections = new GameObject();
      connections.transform.SetParent(transform);
      Material lineMaterial = new Material(Shader.Find(lineMaterialShader));
      bool log = false;
      foreach (var assetLink in mapAssetData.NodeLinks)
      {
        var fromID = assetLink.BaseNodeGuid;
        var toID = assetLink.TargetNodeGuid;

        if(mapNodeLookup[fromID].Disabled || mapNodeLookup[toID].Disabled)
        {
          continue;
        }

        List<McCoyCityZonePlacementNode> sourceNodes = nodes.Where(x => x.NodeId == fromID).ToList();
        int fromIdx = nodes.IndexOf(sourceNodes[0]);

        List<McCoyCityZonePlacementNode> destNodes = nodes.Where(x => x.NodeId == toID).ToList();
        int toIdx = nodes.IndexOf(destNodes[0]);

        var line = new GameObject();
        line.transform.SetParent(connections.transform);
        line.name = $"Line {fromIdx}::{toIdx}";

        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineStartWidth;
        lineRenderer.sortingOrder = lineSortOrder;
        lineRenderer.endWidth = lineEndWidth;
        lineRenderer.material = lineMaterial;

        DrawConnections(lineRenderer, sourceNodes[0].transform.position, destNodes[0].transform.position);
        lineLookup[$"{sourceNodes[0].NodeId}{destNodes[0].NodeId}"] = lineRenderer;
      }
    }

    void DrawConnections(LineRenderer lineRenderer, Vector3 p0, Vector3 p2)
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


    public void LoadQuests(List<McCoyQuestData> currentQuests)
    {
      foreach(var quest in currentQuests)
      {
        mobIndicatorLookup[quest.possibleLocations[0]].ShowQuest(quest);
      }
    }

    private void centerCameraOnSelectedNode()
    {
      Vector3 locPosition = selectedNode.transform.position - selectedNodeCameraZoomOffset;//.localPosition;// NodePosition(m);
      if (log)
      {
        Debug.Log($"Clamping {locPosition.z} between: {cameraBoundsLowerLeft.position.z} and {cameraBoundsUpperRight.position.z}");
      }

      cameraDestination = new Vector3(
        Mathf.Clamp(locPosition.x, cameraBoundsLowerLeft.position.x, cameraBoundsUpperRight.position.x), 
        (cameraBoundsLowerLeft.position.y + cameraBoundsUpperRight.position.y)/2, // should always be the same value, but maybe tweening could be fun
        Mathf.Clamp(locPosition.z, cameraBoundsLowerLeft.position.z, cameraBoundsUpperRight.position.z));

      cameraOrigin = Camera.main.transform.position;
      cameraStartTime = Time.time;
      if (!lerpingCamera)
      {
        StartCoroutine(LerpCamera(cameraMoveTime));
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
          centerCameraOnSelectedNode();
        }
      }

      if (refreshLines)
      {
        this.refreshLines(m, validConnections);
      }
    }

    private void refreshLines(MapNode node, List<MapNode> validConnections = null)
    {
      inactiveConnectionLines.Clear();
      foreach (var entry in lineLookup)
      {
        bool isSelectedNow = node == null ? false : entry.Key.Contains(node.NodeID);

        if (isSelectedNow && validConnections != null)
        {
          bool foundOtherEnd = false;
          foreach (var c in validConnections)
          {
            if (entry.Key.Contains(c.NodeID))
            {
              foundOtherEnd = true;
              break;
            }
          }
          isSelectedNow &= foundOtherEnd;
        }
        if (!isSelectedNow)
        {
          inactiveConnectionLines.Add(entry.Value);
        }
        entry.Value.startColor = isSelectedNow ? connectedLineColor : unconnectedLineColor;// Color.grey;
        entry.Value.endColor = isSelectedNow ? connectedLineColor : unconnectedLineColor; // Color.grey;
      }

      updateUnconnectedLinesState();
    }

    private void updateUnconnectedLinesState()
    {
      foreach (var entry in lineLookup)
      {
        entry.Value.gameObject.SetActive(showUnconnectedLines || !inactiveConnectionLines.Contains(entry.Value));
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

    public void UnlockZone(string id)
    {
      mapNodeLookup[id].Disabled = false;
      redrawLines();
      UpdateNodes();
    }

    public void AnimateMobMove(Factions f, MapNode start, MapNode end, float time, Action finishedCallback, bool hideOriginal = true)
    {
      var mobIndicator = mobIndicatorLookup[start.NodeID];
      mobIndicator.AnimateFaction(f, OffsetBetweenNodes(start, end), time, finishedCallback, hideOriginal);
    }

    public void AnimateMobCombat(MapNode location, ProjectConstants.Factions faction)
    {
      mobIndicatorLookup[location.NodeID].AnimateCombat(faction);
    }

    public Vector3 OffsetBetweenNodes(MapNode start, MapNode end)
    {
      return cityZoneLookup[end.NodeID].transform.position - cityZoneLookup[start.NodeID].transform.position;
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
        mobIndicatorLookup[node.NodeID].gameObject.SetActive(!node.Disabled);
        if (node.Disabled)
        {
          continue;
        }
        int playerNum = playerLocs.ContainsKey(node.NodeID) ? playerLocs[node.NodeID] : -1;
        MapNode mechanismLocation = NodeWithID(McCoy.GetInstance().gameState.AntikytheraMechanismLocation);
        bool showMechanism = mechanismLocation != null && mechanismLocation.MechanismFoundHere && mechanismLocation.NodeID == node.NodeID;
        mobIndicatorLookup[node.NodeID].UpdateWithMobs(node.Mobs, playerNum, node.ZoneName, node.SearchPercent, showMechanism);
      }
    }
  }
}