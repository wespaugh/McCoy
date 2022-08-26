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

    public Fix64 tempLeftBoundary;
    public Fix64 tempRightBoundary;
    bool useTempBoundary;
    public void SetTemporaryBoundaries(Fix64 left, Fix64 right)
    {
      Debug.Log("setting temp boundaries to " + left + ", " + right);
      useTempBoundary = true;
      tempLeftBoundary = left;
      tempRightBoundary = right;
    }
    public void UnsetTemporaryBoundaries()
    {
      useTempBoundary = false;
      Debug.Log("resetting temp boundaries");
    }
    public Fix64 LeftBoundary
    {
      get
      {
        if(useTempBoundary)
        {
          return tempLeftBoundary;
        }
        if(stageInfo != null && stageInfo.substages != null && UFE.config != null && UFE.config.currentRound-1 >= 0 && stageInfo.substages.Count > UFE.config.currentRound)
        {
          return stageInfo.substages[UFE.config.currentRound-1].leftBoundary;
        }
        return _leftBoundary;
      }
    }
    public Fix64 RightBoundary
    {
      get
      {
        if(useTempBoundary)
        {
          return tempRightBoundary;
        }
        if (stageInfo != null && stageInfo.substages != null && UFE.config != null && UFE.config.currentRound - 1 >= 0 && stageInfo.substages.Count > UFE.config.currentRound)
        {
          return stageInfo.substages[UFE.config.currentRound-1].rightBoundary;
        }
        return _rightBoundary;
      }
    }
    
    public float GetLevelExit()
    {
      return ((float)_rightBoundary);
    }

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