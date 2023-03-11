﻿using Assets.McCoy.Brawler;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFE3D.Brawler
{
  public class BrawlerBuffManager : MonoBehaviour
  {
    // id > buffStack
    Dictionary<int, BrawlerBuffStack> characterBuffs = new Dictionary<int, BrawlerBuffStack>();

    public void AddBuff(BrawlerBuff buff, int playerId)
    {
      return;
      if (!characterBuffs.ContainsKey(playerId))
      {
        BrawlerBuffStack buffStack = new BrawlerBuffStack();
        characterBuffs[playerId] = buffStack;
      }
    }
    public void Update()
    {
      return;
      foreach (var c in characterBuffs)
      {
        c.Value.Update();
      }
    }

    public void ClearPlayer(int playerId)
    {
      return;
      if (characterBuffs.ContainsKey(playerId))
      {
        characterBuffs[playerId].Clear();
        characterBuffs.Remove(playerId);
      }
      else
      {
        Debug.LogWarning("Warning: couldn't clear buffs out of player " + playerId);
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