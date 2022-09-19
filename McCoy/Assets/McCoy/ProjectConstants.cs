using System;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy
{
  public class ProjectConstants : ScriptableObject
  {

    public static float SEARCH_COMPLETE_THRESHHOLD = 120.0f;
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

    public static float MOB_ROUTING_HEALTH_THRESHOLD = 0.5f;

    public static string FactionDisplayName(Factions f)
    {
      switch(f)
      {
        case Factions.AngelMilitia:
          return "Militia";
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

    public static Vector3 CalculateArcMidpointBetweenPoints(Vector3 p0, Vector3 p2)
    {
      Vector3 p1 = (p0 + p2) * .5f;
      float height = CalculateHeightBetweenPoints(p0, p2);
      p1.y = height;
      return p1;
    }
    public static float CalculateHeightBetweenPoints(Vector3 p0, Vector3 p2)
    {
      return 3.0f + (Math.Abs(p0.x - p2.x) + Math.Abs(p0.z - p2.z)) / 5.0f;
    }

    public static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
      // (1 - t)2P0 + 2(1 - t)tP1 + t2P2 , 0 < t < 1
      float inverseTime = 1 - t;
      Vector3 retVal = inverseTime * inverseTime * p0;
      retVal = retVal + (2 * inverseTime * t * p1);
      retVal = retVal + t * t * p2;
      return retVal;
    }

    public enum SearchState
    {
      BarelySearched,
      Searching,
      WellSearched,
      CompletelySearched
    }

    public static SearchState SearchProgress(float searchValue)
    {
      if (searchValue < 40)
      {
        return SearchState.BarelySearched;
      }
      else if (searchValue < 80)
      {
        return SearchState.Searching;
      }
      else if (searchValue < SEARCH_COMPLETE_THRESHHOLD)
      {
        return SearchState.WellSearched;
      }
      else
      {
        return SearchState.CompletelySearched;
      }
    }
  }
}