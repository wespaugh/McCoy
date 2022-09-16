using UnityEditor;
using UnityEngine;

namespace Assets.McCoy
{
  public class ProjectConstants : ScriptableObject
  {
    public enum Factions
    {
      None = 0,
      Werewolves = 1,
      Mages = 2,
      AngelMilitia = 3,
      CyberMinotaurs = 4,
      Hunter = 5,
      Neutral
    }

    public static string FactionDisplayName(Factions f)
    {
      switch(f)
      {
        case Factions.AngelMilitia:
          return "Angel Militia";
        case Factions.CyberMinotaurs:
          return "Cyber-Minotaurs";
        case Factions.Mages:
          return "Techno-Mages";
        default:
          return "Werewolves";
      }
    }

    public const int NUM_BOARDGAME_PLAYERS = 4;

    public const string RESOURCES_DIRECTORY = "Assets/Resources/";
    public const string MAPDATA_DIRECTORY = "MapData";
    public const string STAGEDATA_DIRECTORY = "StageData";
    public const string FACTIONLOOKUP_DIRECTORY = "FactionLookup";

    public const string PLAYER_1_NAME = "Rex";
    public const string PLAYER_2_NAME = "Vicki";
    public const string PLAYER_3_NAME = "Avalon";
    public const string PLAYER_4_NAME = "Penelope";

    public static string PlayerName(int playerNumber)
    {
      switch(playerNumber)
      {
        case 1:
          return PLAYER_1_NAME;
        case 2:
          return PLAYER_2_NAME;
        case 3:
          return PLAYER_3_NAME;
        default:
          return PLAYER_4_NAME;
      }
    }
  }
}