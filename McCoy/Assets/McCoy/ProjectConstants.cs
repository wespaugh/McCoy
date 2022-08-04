using UnityEditor;
using UnityEngine;

namespace Assets.McCoy
{
  public class ProjectConstants : ScriptableObject
  {
    public enum Factions
    {
      None = 0,
      Werewolves,
      CyberMinotaurs,
      Mages,
      AngelMilitia,
      Hunter,
      Neutral
    }

    public const string RESOURCES_DIRECTORY = "Assets/Resources/";
    public const string MAPDATA_DIRECTORY = "MapData";
    public const string STAGEDATA_DIRECTORY = "StageData";
    public const string FACTIONLOOKUP_DIRECTORY = "FactionLookup";
  }
}