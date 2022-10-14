using Assets.McCoy.Brawler;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  public class McCoyCharacterBuffManager : MonoBehaviour
  {
    // id > buffStack
    Dictionary<int, McCoyBuffStack> characterBuffs = new Dictionary<int, McCoyBuffStack>();

    public void AddBuff(McCoyBuff buff, int playerId)
    {
      if (!characterBuffs.ContainsKey(playerId))
      {
        McCoyBuffStack buffStack = new McCoyBuffStack();
        characterBuffs[playerId] = buffStack;
      }
    }
    public void Update()
    {
      foreach (var c in characterBuffs)
      {
        c.Value.Update();
      }
    }

    public void ClearPlayer(int playerId)
    {
      if (characterBuffs.ContainsKey(playerId))
      {
        characterBuffs[playerId].Clear();
        characterBuffs.Remove(playerId);
      }
      else
      {
        Debug.Log("Warning: couldn't clear buffs out of player " + playerId);
      }
    }

    public void ClearAllPlayers()
    {
      List<int> keyClone = new List<int>(characterBuffs.Keys);
      foreach(int id in keyClone)
      {
        ClearPlayer(id);
      }
    }
  }
}