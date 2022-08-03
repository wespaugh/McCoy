using Assets.McCoy.Brawler.Stages;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Editor
{
  public class BrawlerStageEditor : ScriptableObject
  {
    [MenuItem("Assets/Create/McCoy/Stage Data")]
    static void MakeStageData()
    {
      AssetDatabase.CreateAsset(CreateInstance<BrawlerStageInfo>(), $"{ProjectConstants.RESOURCES_DIRECTORY}{ProjectConstants.STAGEDATA_DIRECTORY}/New Stage.asset");
      AssetDatabase.SaveAssets();
    }

    [MenuItem("Assets/Create/McCoy/Substage Data")]
    static void MakeSubstageData()
    {
      AssetDatabase.CreateAsset(CreateInstance<BrawlerSubstageInfo>(), $"{ProjectConstants.RESOURCES_DIRECTORY}{ProjectConstants.STAGEDATA_DIRECTORY}/New Substage.asset");
      AssetDatabase.SaveAssets();
    }
  }
}