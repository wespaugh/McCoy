using Assets.McCoy.Brawler.Stages;
using Assets.McCoy.RPG;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Editor
{
  public class QuestListAsset : ScriptableObject
  {
    [MenuItem("Assets/Create/McCoy/Quest List")]
    public static void CreateAsset()
    {
      ScriptableObjectUtility.CreateAsset<McCoyQuestListData>();
    }
  }
}