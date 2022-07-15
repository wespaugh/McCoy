using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Cutscene
{
  public class CutsceneFrame : ScriptableObject
  {
    public enum CutsceneAnchor
    {
      BottomFourthsOne,
      BottomFourthsTwo,
      BottomFourthsThree,
      BottomFourthsFour,
      TopLeft,
      TopRight,
      ThreeColumn1,
      ThreeColumn2,
      ThreeColumn3,
      FourColumn1,
      FourColumn2,
      FourColumn3,
      FourColumn4
    };

    public string Text { get; set; }
    [SerializeField]
    Dictionary<CutsceneAnchor, Texture2D> imageDeltas;

    public Dictionary<CutsceneAnchor, Texture2D> ImageDeltas { get => imageDeltas; }
  }
}