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
    string namename;
    public object Clone()
    {
      return CloneObject.Clone(this);
    }
  }
}