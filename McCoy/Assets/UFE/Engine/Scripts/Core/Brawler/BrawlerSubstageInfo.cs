using System;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;

namespace Assets.McCoy.Brawler.Stages
{

  [Serializable]
  public class BrawlerSubstageInfo : ScriptableObject, ICloneable
  {
    public string substageName = null;
    public GameObject stagePrefab = null;
    public List<BrawlerStageBoundary> boundaries;
    public List<Rect> holes;
    public float leftBoundary;
    public float rightBoundary;
    public object Clone()
    {
      return CloneObject.Clone(this);
    }

    public bool IsInHole(float x, float y)
    {
      foreach(Rect r in holes)
      {
        if(x >= r.x && x <= r.x + r.width &&
          y >= r.y && y <= r.y + r.height)
        {
          return true;
        }
      }
      return false;
    }
  }
}