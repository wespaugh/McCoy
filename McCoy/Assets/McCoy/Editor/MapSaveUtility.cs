using Assets.McCoy.BoardGame;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.McCoy.Editor
{
  public class MapSaveUtility
  {
    private MapGraphEditorView _targetEditorView;
    private MapGraphNodeContainer mapCache;

    private List<Edge> Edges => _targetEditorView.edges.ToList();
    private List<MapGraphNode> Nodes => _targetEditorView.nodes.ToList().Cast<MapGraphNode>().ToList();
    public static MapSaveUtility GetInstance(MapGraphEditorView targetGraphView)
    {
      return new MapSaveUtility()
      {
        _targetEditorView = targetGraphView
      };
    }

    public bool SaveMap(string filename)
    {
      Debug.Log("save map to " + filename);
      var mapContainer = ScriptableObject.CreateInstance<MapGraphNodeContainer>();
      if(! SaveNodes(filename, mapContainer) )
      {
        return false;
      }

      // SaveExposedProperties();
      // SaveCommentBlocks();


      return true;
    }

    public bool SaveNodes(string filename, MapGraphNodeContainer mapNodeContainer)
    {
      if (Edges.Count == 0) return false;      

      var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
      for(int i = 0; i < connectedPorts.Length; ++i)
      {
        var outputNode = connectedPorts[i].output.node as MapGraphNode;
        var inputNode = connectedPorts[i].input.node as MapGraphNode;
        mapNodeContainer.NodeLinks.Add(new MapNodeLinkData()
        {
          BaseNodeGuid = outputNode.guid.ToString(),
          PortName = connectedPorts[i].output.portName,
          TargetNodeGuid = inputNode.guid.ToString()
        });
      }
      foreach(var node in Nodes)
      {
        mapNodeContainer.NodeData.Add(new MapNode
        {
          NodeID = node.guid.ToString(),
          ZoneName = node.zoneName,
          Position = node.GetPosition().position
        });
      }

      if(!AssetDatabase.IsValidFolder($"{ProjectConstants.RESOURCES_DIRECTORY}{ProjectConstants.MAPDATA_DIRECTORY}"))
      {
        AssetDatabase.CreateFolder(ProjectConstants.RESOURCES_DIRECTORY, ProjectConstants.MAPDATA_DIRECTORY);
      }

      AssetDatabase.CreateAsset(mapNodeContainer, $"{ProjectConstants.RESOURCES_DIRECTORY}{ProjectConstants.MAPDATA_DIRECTORY}/{filename}.asset");
      AssetDatabase.SaveAssets();
      Debug.Log("Saved Nodes! to " + $"{ProjectConstants.RESOURCES_DIRECTORY}{ProjectConstants.MAPDATA_DIRECTORY}/{filename}.asset");
      return true;
    }

    public void LoadMap(string filename)
    {
      mapCache = Resources.Load<MapGraphNodeContainer>($"{ProjectConstants.MAPDATA_DIRECTORY}/{filename}");
      if(mapCache == null)
      {
        EditorUtility.DisplayDialog("Uh oh!", "Couldn't read that file, though...", "Whatever");
        return;
      }
      ClearGraph();
      CreateNodes();
      ConnectNodes();

    }

    private void ConnectNodes()
    {
      foreach (var nodeLink in mapCache.NodeLinks)
      {
        foreach(var existingNode in _targetEditorView.nodes)
        {
          if((existingNode as MapGraphNode).guid.ToString() == nodeLink.BaseNodeGuid)
          {
            int i = 0;
            foreach(var connectedNode in _targetEditorView.nodes)
            {
              if((connectedNode as MapGraphNode).guid.ToString() == nodeLink.TargetNodeGuid)
              {
                Debug.Log("Existing node output container count: " + existingNode.outputContainer.childCount);
                Debug.Log("input container count: " + connectedNode.inputContainer.childCount);
                LinkNodes(existingNode.outputContainer[i].Q<Port>(), (Port)connectedNode.inputContainer[0]);
                ++i;
              }
            }
          }
        }
      }
    }
    void LinkNodes(Port p1, Port p2)
    {
      var tempEdge = new Edge()
      {
        output = p1,
        input = p2
      };
      tempEdge?.input.Connect(tempEdge);
      tempEdge?.output.Connect(tempEdge);
      _targetEditorView.Add(tempEdge);
    }
    private void CreateNodes()
    {
      foreach(var nodeData in mapCache.NodeData)
      {
        Debug.Log("creating node data for node with name: " + nodeData.ZoneName);
        var tempNode = _targetEditorView.CreateMapNode(nodeData.ZoneName);
        tempNode.guid = new GUID(nodeData.NodeID);
        tempNode.SetPosition(new Rect(nodeData.Position.x, nodeData.Position.y, 400, 200));
        _targetEditorView.AddElement(tempNode);
      }
    }

    private void ClearGraph()
    {
      foreach (var node in Nodes)
      {
        var nodeInputEdges = Edges.Where(x => x.input.node == node).ToList();
        foreach(var edge in nodeInputEdges)
        {
          _targetEditorView.RemoveElement(edge);
        }
        _targetEditorView.RemoveElement(node);
      }
    }
  }
}