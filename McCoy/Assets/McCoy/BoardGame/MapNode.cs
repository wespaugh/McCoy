using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;
using Random = UnityEngine.Random;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class MapNode : SearchableNode
  {
    [NonSerialized]
    public List<McCoyMobData> Mobs = new List<McCoyMobData>();
    public bool HasMechanism
    {
      get;
      set;
    }
    int bonusSearchDice = 0;
    // a value shown to the player indicating the likelihood of a given mob finding the mechanism at this location, if it is there.
    float bonusValue = 0;
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
      // generally, save the average successes as the best search. we don't want to let the player see how well 
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

    public string ZoneName;
    public Vector2 Position;

    public int DistanceToMechanism
    {
      get;
      private set;
    }

    public void SetMechanismLocation(MapNode loc)
    {
      DistanceToMechanism = DistanceTo(loc);
    }

  }
}