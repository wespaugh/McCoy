using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.McCoy.Editor
{
  public class MapGraphWindow : EditorWindow
  {
    private MapGraphEditorView _mapGraphView;
    private string filename;

    [MenuItem("McCoy/Maps/Graph")]
    public static void OpenMapGraphWindow()
    {
      var window = GetWindow<MapGraphWindow>();
      if(window == null)
      {
        window = CreateInstance<MapGraphWindow>();
        window.Show();
      }
      else
      {
        window.minSize = new Vector2(100, 150);
      }
      window.titleContent = new GUIContent("McCoy Map Graph");
    }

    private void ConstructGraph()
    {
      _mapGraphView = new MapGraphEditorView(this)
      {
        name = "Map Node Graph"
      };

      _mapGraphView.StretchToParentSize();
      rootVisualElement.Add(_mapGraphView);
    }

    private void GenerateToolbar()
    {
      var toolbar = new Toolbar();
      var nodeCreateButton = new Button(clickEvent: () =>
      {
        var newNode = _mapGraphView.CreateMapNode("New City Zone");
      });
      nodeCreateButton.text = "Create City Zone";
      toolbar.Add(nodeCreateButton);

      TextField filenameTextField = new TextField();
      filenameTextField.label = "Output Filename";
      filename = "NewMoonCity";
      filenameTextField.SetValueWithoutNotify(filename);
      filenameTextField.MarkDirtyRepaint();
      filenameTextField.RegisterValueChangedCallback((evt) =>
      {
        filename = evt.newValue;
      });
      toolbar.Add(filenameTextField);

      var saveButton = new Button(clickEvent: () => RequestDataOperation(true)) { text = "Save Data" };
      var loadButton = new Button(clickEvent: () => RequestDataOperation(false)) { text = "Load Data" };
      toolbar.Add(saveButton);
      toolbar.Add(loadButton);
      rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
      if(string.IsNullOrEmpty(filename))
      {
        EditorUtility.DisplayDialog("NO FILENAME, DUMMY", "ENTER A FILENAME", "OKAY!!!?");
        return;
      }

      var saveUtility = MapSaveUtility.GetInstance(_mapGraphView);
      if(save)
      {
        saveUtility.SaveMap(filename);
      }
      else
      {
        saveUtility.LoadMap(filename);
      }
    }

    private void OnEnable()
    {
      ConstructGraph();
      GenerateToolbar();
    }
    private void OnDisable()
    {
      rootVisualElement.Remove(_mapGraphView);
    }
  }
}