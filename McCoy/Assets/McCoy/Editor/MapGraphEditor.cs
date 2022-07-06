using Assets.McCoy.BoardGame;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.McCoy.Editor
{
  public class MapGraphEditorView : GraphView
  {
    List<IManipulator> manipulators;
    GraphElement firstMapNode;
    GridBackground grid = null;

    public MapGraphEditorView(MapGraphWindow editorWindow)
    {
      // styleSheets.Add(Resources.Load<StyleSheet>("NarrativeGraph"));
      SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

      if (manipulators == null)
      {
        manipulators = new List<IManipulator>();
      }
      manipulators.Add(new ContentDragger());
      manipulators.Add(new SelectionDragger());
      manipulators.Add(new RectangleSelector());
      manipulators.Add(new FreehandSelector());

      foreach (var man in manipulators)
      {
        this.AddManipulator(man);
      }

      grid = new GridBackground();
      Insert(0, grid);
      grid.StretchToParentSize();

      if (firstMapNode == null)
      {
        firstMapNode = GetFirstMapNode();
      }

      AddSearchWindow(editorWindow);
    }

    private void AddSearchWindow(MapGraphWindow editorWindow)
    {
      /*
      _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
      _searchWindow.Configure(editorWindow, this);
      nodeCreationRequest = context =>
          SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
      */
    }

    public MapGraphNode CreateMapNode(string name)
    {
      MapGraphNode newNode = new MapGraphNode()
      {
        guid = GUID.Generate(),
      };

      TextField nameText = new TextField();
      newNode.zoneName = name;
      nameText.SetValueWithoutNotify(name);
      nameText.RegisterValueChangedCallback((name) =>
      {
        newNode.zoneName = name.newValue;
      });
      newNode.titleContainer.Add(nameText);

      Port inputPort = generatePort(newNode, Direction.Input);
      inputPort.portName = "Left";
      newNode.inputContainer.Add(inputPort);
      Port outputPort = generatePort(newNode, Direction.Output);
      outputPort.portName = "Right";
      newNode.outputContainer.Add(outputPort);

      newNode.RefreshExpandedState();
      newNode.RefreshPorts();

      newNode.SetPosition(new Rect(Vector2.zero, new Vector2(400, 150)));
      AddElement(newNode);

      return newNode;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
      List<Port> compatiblePorts = new List<Port>();
      foreach(var port in ports)
      {
        compatiblePorts.Add(port);
      }
      return compatiblePorts;
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
      foreach(var man in manipulators)
      {
        this.RemoveManipulator(man);
      }
      manipulators.Clear();
    }

    private Port generatePort(MapGraphNode n, Direction portDirection, Port.Capacity capacity = Port.Capacity.Multi)
    {
      return n.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type: typeof(float));
    }

    private GraphElement GetFirstMapNode()
    {
      return CreateMapNode("City Zone");
    }
  }
}