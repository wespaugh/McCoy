using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using System.Collections.Generic;
using System.Linq;
using UFE3D;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyCityBoardContents : MonoBehaviour
  {
    Dictionary<string, McCoyCityZonePlacementNode> cityZoneLookup = new Dictionary<string, McCoyCityZonePlacementNode>();

    [SerializeField]
    GameObject NodeParent;

    [SerializeField]
    string dataFile;

    [SerializeField]
    GameObject ZonePanelPrefab = null;

    [SerializeField]
    Transform Scaler = null;

    bool loadingStage = false;

    private void Awake()
    {
      loadingStage = false;

      var mapCacheObj = Resources.Load($"{ProjectConstants.MAPDATA_DIRECTORY}/{dataFile}");
      MapGraphNodeContainer mapCache = mapCacheObj as MapGraphNodeContainer;

      McCoyCityZonePlacementNode[] nodes = NodeParent.GetComponentsInChildren<McCoyCityZonePlacementNode>();
      foreach (var node in nodes)
      {
        foreach (var assetNode in mapCache.NodeData)
        {
          if (assetNode.NodeID == node.NodeId)
          {
            var nodePanel = Instantiate(ZonePanelPrefab, node.transform);
            nodePanel.GetComponent<MapCityNodePanel>().Initialize(assetNode, this);
            break;
          }
        }
      }

      GameObject connections = new GameObject();
      connections.transform.SetParent(transform);
      Material lineMaterial = new Material(Shader.Find("Unlit/Texture"));
      foreach (var assetLink in mapCache.NodeLinks)
      {
        var fromID = assetLink.BaseNodeGuid;
        List<McCoyCityZonePlacementNode> sourceNodes = nodes.Where(x => x.NodeId == fromID).ToList();
        int fromIdx = -1;
        for (int i = 0; i < nodes.Length; ++i) if(nodes[i] == sourceNodes[0]) { fromIdx = i; break; }
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
        lineRenderer.startWidth = 4.0f;
        lineRenderer.endWidth = 1.0f;
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.green;
        lineRenderer.material = lineMaterial;
        List<Vector3> positions = new List<Vector3>();
        positions.Add(new Vector3(sourceNodes[0].transform.position.x/* * Scaler.localScale.x*/, sourceNodes[0].transform.position.y /* * Scaler.localScale.y*/, -10));
        positions.Add(new Vector3(destNodes[0].transform.position.x/* * Scaler.localScale.x*/, destNodes[0].transform.position.y /** Scaler.localScale.y*/, -10));
        lineRenderer.SetPositions(positions.ToArray());
      }
    }

    public void LoadStage(McCoyStageData stageData)
    {
      if (!loadingStage)
      {
        loadingStage = true;
        McCoy.GetInstance().LoadBrawlerStage(stageData);
      }
    }
  }
}