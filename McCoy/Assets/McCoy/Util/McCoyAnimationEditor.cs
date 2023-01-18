using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

public class McCoyAnimationEditor : EditorWindow
{

  class Styles
  {
    public Styles()
    {
    }
  }
  static Styles s_Styles;

  int numGameObjects = 0;

  protected Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
  protected Dictionary<GameObject, AnimationClip> animationClips = new Dictionary<GameObject, AnimationClip>();
  protected float time = 0.0f;
  protected float animationDuration = 0.0f;
  protected bool lockSelection = false;
  protected bool animationMode = false;
  protected string limbAnimationString = "";
  protected string powershellRoot = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe";
  protected string exportScript = "C:\\Users\\wespa\\Documents\\McCoy\\Utilities\\mirrorAnimClips.ps1";
  protected string animListFileName = "Utilities/animList.txt";

  [MenuItem("McCoy/AnimationEditor", false, 2000)]
  public static void DoWindow()
  {
    GetWindow<McCoyAnimationEditor>();
  }

  public void OnEnable()
  {
  }

  public void OnSelectionChange()
  {
    if (gameObjects.Count != 0)
    {
      return;
    }
    clear();
    if(Selection.activeGameObject == null)
    {
      return;
    }
    for(int i = 0; i < Selection.activeGameObject.transform.childCount; ++i)
    {
      var go = Selection.activeGameObject.transform.GetChild(i).gameObject;
      if(gameObjects.ContainsKey(go.name))
      {
        UnityEngine.Debug.LogWarning("Key Collision: " + go.name);
      }
      gameObjects[go.name] = go;
      ++numGameObjects;
    }
  }

  protected void ExportAlts()
  {
    string path = EditorUtility.OpenFolderPanel("Select Animation Container", "", "");
    string args = exportScript + " ";
    
    StreamWriter fileOut = new StreamWriter(animListFileName);
    // fileOut.WriteLine(path);
    foreach (var a in animationClips.Values)
    {
      fileOut.WriteLine(path +"/"+ a.name + ".anim");
    }
    fileOut.Flush();
    fileOut.Close();
  }

  private IEnumerator export(string path, string args)
  {
    yield return null;
  }

  protected void clear()
  {
    time = 0;
    numGameObjects = 0;
    animationDuration = 0f;
    gameObjects.Clear();
    animationClips.Clear();
  }

  public void OnGUI()
  {
    if (s_Styles == null)
      s_Styles = new Styles();

    GUILayout.BeginHorizontal(EditorStyles.toolbar);

    EditorGUI.BeginChangeCheck();
    GUILayout.Toggle(AnimationMode.InAnimationMode(), "Animate", EditorStyles.toolbarButton);
    if (EditorGUI.EndChangeCheck())
      ToggleAnimationMode();

    GUILayout.FlexibleSpace();
    if (GUILayout.Button("Clear"))
    {
      clear();
    }
    if (GUILayout.Button("Add GameObject"))
    {
      ++numGameObjects;
    }//GUILayout.Toggle(lockSelection, "Add Game Object", EditorStyles.toolbarButton);
    GUILayout.EndHorizontal();

    GUILayout.BeginHorizontal();
    limbAnimationString = GUILayout.TextField(string.IsNullOrEmpty(limbAnimationString) ? "limb animation string" : limbAnimationString);
    if (GUILayout.Button("assign"))
    {
      string[] commands = limbAnimationString.Split(',');
      foreach (var cmd in commands)
      {
        animationDuration = 0f;
        string[] animatorKeys = cmd.Split(':');
        string spriteKey = animatorKeys[0];
        string animKey = animatorKeys.Length > 1 ? animatorKeys[1] : "";
        bool hide = animatorKeys.Length == 0 || animKey == "";
        gameObjects[spriteKey].SetActive(!hide);
        bool found = false;
        foreach (var clip in gameObjects[spriteKey].GetComponent<Animator>().runtimeAnimatorController.animationClips)
        {
          if (clip.name == animKey)
          {
            animationClips[gameObjects[spriteKey]] = clip;
            found = true;
            break;
          }
        }
        if(!found)
        {
          UnityEngine.Debug.Log("unable to find animation named " + animKey + " in controller");
        }
      }
    }
    GUILayout.EndHorizontal();

    EditorGUILayout.BeginVertical();
    if (GUILayout.Button("Create Sprite FlipX's"))
    {
      // string path = EditorUtility.OpenFolderPanel("Export horizontal flips", "", "");
      ExportAlts();
    }
    for (int i = 0; i < numGameObjects; ++i)
    {
      var keys = gameObjects.Keys.ToList();
      GameObject go = i >= keys.Count ? null : gameObjects[keys[i]];
      go = EditorGUILayout.ObjectField(go, typeof(GameObject), true) as GameObject;
      string goName = go?.name;
      if(go != null)
      {
        gameObjects[goName] = go;
      }
      else if(!string.IsNullOrEmpty(goName) && gameObjects.ContainsKey(goName))
      {
        gameObjects.Remove(goName);
      }
      AnimationClip selectedClip = go != null && animationClips.ContainsKey(go) ? animationClips[go] : null;
      selectedClip = EditorGUILayout.ObjectField(selectedClip, typeof(AnimationClip), false) as AnimationClip;
      if (selectedClip != null)
      {
        animationClips[go] = selectedClip;
        if(animationDuration != 0.0f && animationDuration != selectedClip.length)
        {
          UnityEngine.Debug.LogError("clip " + selectedClip.name + " has a incongruent clip time " + selectedClip.length);
        }
        animationDuration = selectedClip.length;
      }
      else if(go != null && animationClips.ContainsKey(go))
      {
        animationClips.Remove(go);
      }
    }
    time = EditorGUILayout.Slider(time, 0, animationClips.Count > 0 ? animationClips.First().Value.length : 0);

    EditorGUILayout.EndVertical();
  }

  void Update()
  {
    
    int idx = 0;
    if(!AnimationMode.InAnimationMode())
    {
      return;
    }

    AnimationMode.BeginSampling();
    foreach (var go in gameObjects)
    {
      //if (idx == 3)
      {
        // there is a bug in AnimationMode.SampleAnimationClip which crash unity if there is no valid controller attached
        Animator animator = go.Value.GetComponent<Animator>();
        if(!animationClips.ContainsKey(go.Value))
        {
          continue;
        }
        AnimationClip animationClip = animationClips[go.Value];
        if (animator != null && animator.runtimeAnimatorController == null)
          return;

        if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode())
        {
          AnimationMode.SampleAnimationClip(go.Value, animationClip, time);
        }
      }
      ++idx;
    }
    AnimationMode.EndSampling();
    SceneView.RepaintAll();
  }

  void ToggleAnimationMode()
  {
    if (AnimationMode.InAnimationMode())
      AnimationMode.StopAnimationMode();
    else
      AnimationMode.StartAnimationMode();
  }
}
