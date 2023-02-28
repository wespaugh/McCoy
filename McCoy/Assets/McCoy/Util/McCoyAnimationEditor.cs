using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using UFE3D;
using System.Text.RegularExpressions;

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
  protected List<GameObject> cyberAnimObjects = new List<GameObject>();
  protected GameObject bodySprite = null;
  protected AnimationClip bodyAnimationClip = null;
  protected float time = 0.0f;
  protected float animationDuration = 0.0f;
  protected bool lockSelection = false;
  protected bool animationMode = false;
  protected string limbAnimationString = "";
  protected string modSuffix = "";
  protected string bodyAnimation = "";
  protected bool flip = false;
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

  public void CaptureGameObject()
  {
    if(Selection.activeGameObject == null)
    {
      return;
    }
    clear();
    SpriteSortingScript sortScript = Selection.activeGameObject.GetComponent<SpriteSortingScript>();
    bodySprite = Selection.activeGameObject;
    for(int i = 0; i < Selection.activeGameObject.transform.childCount; ++i)
    {
      var go = Selection.activeGameObject.transform.GetChild(i).gameObject;
      if(sortScript.SpritesToModify.Contains(go.GetComponent<SpriteRenderer>()))
      {
        cyberAnimObjects.Add(go);
      }
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
    if(string.IsNullOrEmpty(path))
    {
      return;
    }
    string args = exportScript + " ";
    
    StreamWriter fileOut = new StreamWriter(animListFileName);
    // fileOut.WriteLine(path);
    foreach (var kvp in animationClips)
    {
      string line = path + "/" + kvp.Value.name + ".anim";
      if(cyberAnimObjects.Contains(kvp.Key))
      {
        line += " IsCyber=true";
      }
      UnityEngine.Debug.Log("LINE: " + line);
      fileOut.WriteLine(line);
    }
    fileOut.Flush();
    fileOut.Close();

    ProcessStartInfo processinfo = new
    ProcessStartInfo("C:/Windows/System32/WindowsPowerShell/v1.0/powershell.exe");
    processinfo.RedirectStandardOutput = false;
    processinfo.RedirectStandardError = false;
    processinfo.RedirectStandardInput = true;
    processinfo.UseShellExecute = false; //<----Causes program to crash exe when launched, but is required for write console key.
    processinfo.CreateNoWindow = false;
    string utilPath = Application.dataPath.Substring(0, Application.dataPath.Length-6) + "Utilities/mirrorAnimClips.ps1";
    UnityEngine.Debug.Log(utilPath);
    // processinfo.ArgumentList.Add(utilPath);
    Process p = Process.Start("powershell.exe", "-NoExit -Command " + utilPath);
    p.WaitForExit();
    p.Close();
  }

  protected void clear()
  {
    time = 0;
    numGameObjects = 0;
    animationDuration = 0f;
    gameObjects.Clear();
    animationClips.Clear();
    cyberAnimObjects.Clear();
  }

  public void OnGUI()
  {
    if (s_Styles == null)
      s_Styles = new Styles();

    GUILayout.BeginHorizontal(EditorStyles.toolbar);

    EditorGUI.BeginChangeCheck();
    GUILayout.Toggle(AnimationMode.InAnimationMode(), "Animate", EditorStyles.toolbarButton, GUILayout.Width(100));
    if (EditorGUI.EndChangeCheck())
    {
      ToggleAnimationMode();
    }
    GUILayout.EndHorizontal();

    GUILayout.BeginHorizontal();
    //GUILayout.FlexibleSpace();
    if (GUILayout.Button("Capture Limb Sprites", GUILayout.Width(300)))
    {
      CaptureGameObject();
    }//GUILayout.Toggle(lockSelection, "Add Game Object", EditorStyles.toolbarButton);
    if (GUILayout.Button("Clear Limb Sprites", GUILayout.Width(300)))
    {
      clear();
    }
    GUILayout.EndHorizontal();

    /*
    GUILayout.BeginHorizontal();
    GUILayout.Label("Body Animation", GUILayout.Width(200));
    bodyAnimation = GUILayout.TextField(bodyAnimation, GUILayout.Width(400));
    GUILayout.EndHorizontal();
    */
    GUILayout.BeginHorizontal();
    GUILayout.Label("Animation", GUILayout.Width(200));
    limbAnimationString = GUILayout.TextField(limbAnimationString, GUILayout.Width(400));
    GUILayout.EndHorizontal();
    GUILayout.BeginHorizontal();
    GUILayout.Label("Cyber Suffix", GUILayout.Width(200));
    modSuffix = GUILayout.TextField(modSuffix, GUILayout.Width(400));
    GUILayout.EndHorizontal();
    GUILayout.BeginHorizontal();
    flip = GUILayout.Toggle(flip, "Flip", GUILayout.Width(100));
    GUILayout.EndHorizontal();
    GUILayout.BeginHorizontal();
    if (GUILayout.Button("assign", GUILayout.Width(200)))
    {
      string[] commands = limbAnimationString.Split(',');
      foreach (var cmd in commands)
      {
        animationDuration = 0f;
        string[] animatorKeys = cmd.Split(':');
        string spriteKey = animatorKeys[0];
        string animKey = animatorKeys.Length > 1 ? animatorKeys[1] : "";
        if(!string.IsNullOrEmpty(modSuffix) && (spriteKey == "2" || spriteKey == "3"))
        {
          animKey = animKey + "_" + modSuffix;
        }
        if(flip)
        {
          animKey = animKey + "_flip";
        }
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
          UnityEngine.Debug.LogWarning("clip " + selectedClip.name + " has a incongruent clip time " + selectedClip.length);
        }
        if (selectedClip.length > animationDuration)
        {
          animationDuration = selectedClip.length;
        }
      }
      else if(go != null && animationClips.ContainsKey(go))
      {
        animationClips.Remove(go);
      }
    }
    time = EditorGUILayout.Slider(time, 0, animationClips.Count > 0 ? animationClips.First().Value.length : 0);

    EditorGUILayout.EndVertical();

    if (GUILayout.Button("Create Sprite FlipX's", GUILayout.Width(200)))
    {
      // string path = EditorUtility.OpenFolderPanel("Export horizontal flips", "", "");
      ExportAlts();
    }
  }

  void Update()
  {
    
    int idx = 0;
    if(!AnimationMode.InAnimationMode())
    {
      return;
    }

    AnimationMode.BeginSampling();
    // Animator animator = bodySprite.GetComponent<Animator>();
    // AnimationMode.SampleAnimationClip(bodySprite, bodyAnimationClip, time);
    foreach (var go in gameObjects)
    {
      //if (idx == 3)
      {
        // there is a bug in AnimationMode.SampleAnimationClip which crash unity if there is no valid controller attached
        // animator = go.Value.GetComponent<Animator>();
        if(!animationClips.ContainsKey(go.Value))
        {
          continue;
        }
        AnimationClip animationClip = animationClips[go.Value];
        //if (animator != null && animator.runtimeAnimatorController == null)
          //return;

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
    UnityEngine.Debug.Log("ToggleAnimationMode");
    if (AnimationMode.InAnimationMode())
      AnimationMode.StopAnimationMode();
    else
      AnimationMode.StartAnimationMode();
  }
}
