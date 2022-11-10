using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Assets.McCoy.ProjectConstants;
using Random = UnityEngine.Random;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class MapNodeSearchData
  {
    public string NodeID;
    public string ZoneName;
    public int bonusSearchDice = 0;
    public float bonusValue = 0f;
    public bool hasMechanism;
    public int distanceToMechanism;
  }
  [Serializable]
  public class MapNode : SearchableNode
  {
    public List<McCoyMobData> Mobs = new List<McCoyMobData>();

    MapNodeSearchData searchData = null;
    public MapNodeSearchData SearchData
    {
      get
      {
        if(searchData == null)
        {
          searchData = new MapNodeSearchData();
        }
        return searchData;
      }
      set
      {
        searchData = value;
      }
    }
    public bool HasMechanism
    {
      get => SearchData.hasMechanism;
      set
      {
        SearchData.hasMechanism = value;
      }
    }

    public string ZoneName;

    [NonSerialized]
    public Vector2 Position;

    int bonusSearchDice
    {
      get => SearchData.bonusSearchDice;
      set
      {
        SearchData.bonusSearchDice = value;
      }
    }
    // a value shown to the player indicating the likelihood of a given mob finding the mechanism at this location, if it is there.
    float bonusValue
    {
      get => SearchData.bonusValue;
      set
      {
        SearchData.bonusValue = value;
      }
    }
    public void LoadSearchData(MapNodeSearchData mapNodeSearchData)
    {
      SearchData = mapNodeSearchData;
      SearchData.NodeID = NodeID;
      SearchData.ZoneName = ZoneName;
    }

    public void Search(int strongestMobStrength, int strongestMobHealth)
    {
      int totalBonuses = bonusSearchDice + Math.Max(0, McCoy.GetInstance().gameState.Week - 5);
      int numD10s = strongestMobStrength + totalBonuses;
      int numD6s = strongestMobHealth;
      int rollTotal = roll(10, numD10s);
      rollTotal += roll(6, numD6s);

      if(rollTotal > 60 + (bonusSearchDice*5))
      {
        bonusSearchDice += 2;
      }

      // 6.1 is roughly the average roll per d10 when rerolling 10s
      float averageSuccess = (totalBonuses * 6.1f);
      // generally, save the average successes as the best search. we don't want to let the player see how well the area was searched exactly, just how easy it is to search in future
      if(averageSuccess > bonusValue)
      {
        bonusValue = averageSuccess;
      }
      // if somebody rolled high enough to succeed on the whole search, replace our best result with the actual total
      if(rollTotal > SEARCH_COMPLETE_THRESHHOLD)
      {
        bonusValue = rollTotal;
      }
    }

    public float SearchPercent
    {
      get
      {
        return 100*bonusValue/SEARCH_COMPLETE_THRESHHOLD;
      }
    }

    public bool MechanismFoundHere => SearchPercent >= 100f && HasMechanism;

    public SearchState SearchStatus()
    {
      return SearchProgress(bonusValue);
    }

    private int roll(int die, int number, bool explode = true)
    {
      int retVal = 0;
      string sb = "";

      bool exploding = false;
      for(int i = 0; i < number; ++i)
      {
        int nextRoll = Random.Range(1, die+1);

        // done exploding
        if(exploding && nextRoll != die)
        {
          sb += "}";
          exploding = false;
        }

        // crits explode, granting an additional die
        if(nextRoll == die)
        {
          --i;
          if(!exploding)
          {
            sb += "{";
          }
          exploding = true;
        }
        sb += nextRoll;
        retVal += nextRoll;
      }
      Debug.Log($"rolled {number} d{die}s:");
      Debug.Log(sb);
      return retVal;
    }

    public int DistanceToMechanism
    {
      get => SearchData.distanceToMechanism;
      private set
      {
        SearchData.distanceToMechanism = value;
      }
    }

    public void SetMechanismLocation(MapNode loc)
    {
      DistanceToMechanism = DistanceTo(loc);
    }

    public static int Compare(MapNode x, MapNode y, MapNode playerLoc, bool mechanismFound, int minDistanceToMechanism)
    {
      bool xIsConnected = false;
      bool yIsConnected = false;
      foreach (var connection in playerLoc.GetConnectedNodes())
      {
        if (x.NodeID == connection.NodeID)
        {
          xIsConnected = true;
        }
        if (y.NodeID == connection.NodeID)
        {
          yIsConnected = true;
        }
      }
      bool xIsCloseEnough = !mechanismFound || x.DistanceToMechanism <= minDistanceToMechanism;
      bool yIsCloseEnough = !mechanismFound || y.DistanceToMechanism <= minDistanceToMechanism;
      if (xIsConnected)
      {
        // x is connected and close enough. best case
        if (xIsCloseEnough)
        {
          if (yIsConnected && yIsCloseEnough)
          {
            return 0;
          }
          return -1;
        }
        // x is connected but not close enough
        else
        {
          // y not connected, x has priority
          if (!yIsConnected)
          {
            return -1;
          }
          // y is connected, but not close enough. same as X
          if (!yIsCloseEnough)
          {
            return 0;
          }
          // y is connected and close enough. y has priority
          return 1;
        }
      }
      // x is not connected
      else
      {
        // y is connected, y has priority
        if (yIsConnected)
        {
          return 1;
        }
        // neither y or x is connected,
        // x is close enough
        if (xIsCloseEnough)
        {
          // y close enough, equivalent
          if (yIsCloseEnough)
          {
            return 0;
          }
          // x close enough, y not close enough, x has priority
          else
          {
            return -1;
          }
        }
        // x not close enough
        else
        {
          // x not close enough, y close enough, y has priority
          if (yIsCloseEnough)
          {
            return 1;
          }
          else
          {
            // x not close enough, y not close enough, equivalent
            return 0;
          }
        }
      }
    }
  }
}