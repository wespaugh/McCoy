using UnityEngine;
using System;
using FPLibrary;
using Assets.McCoy.Brawler.Stages;
using UnityEditor;

namespace UFE3D
{
    [System.Serializable]
    public class StageOptions : ICloneable
    {
        public string stageName;
        public StorageMode stageLoadingMethod;
        public string stagePath;
        public Texture2D screenshot;
        public GameObject prefab;
        public AudioClip music;
        public Fix64 _groundFriction = 100;
        public Fix64 _leftBoundary = -38;
        public Fix64 _rightBoundary = 38;
        public FPVector position;
    public string advancedStageData = null;

    public BrawlerStageInfo stageInfo = null;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }

    public void LoadAdvancedLevelInfo()
    {
      if (stageInfo == null)
      {
        // TODO: Be smarter about loading level location
        stageInfo = Resources.Load<BrawlerStageInfo>(advancedStageData);
      }
    }
  }
}