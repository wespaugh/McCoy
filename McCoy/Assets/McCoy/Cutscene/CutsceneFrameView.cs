using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Assets.McCoy.Cutscene.CutsceneFrame;

namespace Assets.McCoy.Cutscene
{
  public class CutsceneFrameView : MonoBehaviour
  {
    public Image MainView = null;

    public Dictionary<CutsceneAnchor, Image> hooks = new Dictionary<CutsceneAnchor, Image>();

    List<CutsceneFrame> frames = new List<CutsceneFrame>();

    int currentFrame = 0;
    public void Initialize(List<CutsceneFrame> frames)
    {
      this.frames = frames;
      currentFrame = 0;
    }

    public void NextFrame()
    {
      if (currentFrame == frames.Count - 1) Debug.Log("End of Cutscene!");
      CutsceneFrame prev = frames[currentFrame];
      CutsceneFrame next = frames[++currentFrame];
      foreach(var element in next.ImageDeltas)
      {
        if(hooks[element.Key].isActiveAndEnabled)
        {
          hooks[element.Key].CrossFadeAlpha(0, 1.0f, false);
        }
        Debug.Log("ASSIGN SPRITE HERE");
      }
    }

    public void GotoPreviousFrame()
    {
      CutsceneFrame nextFrame = null;
      if(currentFrame == 0)
      {

      }
    }
  }
}