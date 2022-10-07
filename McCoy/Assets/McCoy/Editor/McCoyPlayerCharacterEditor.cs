using Assets.McCoy.Brawler.Stages;
using Assets.McCoy.RPG;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Editor
{
  public class PlayerCharacterEditor: ScriptableObject
  {
    [MenuItem("Assets/Create/McCoy/Player Character")]
    static void MakeStageData()
    {
      AssetDatabase.CreateAsset(CreateInstance<McCoyPlayerCharacter>(), $"{ProjectConstants.RESOURCES_DIRECTORY}{ProjectConstants.PLAYERCHARACTER_DIRECTORY}/New Character.asset");
      AssetDatabase.SaveAssets();
    }
  }
}