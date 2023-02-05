using Assets.McCoy.Brawler;
using System.Collections.Generic;
using static Assets.McCoy.Brawler.BrawlerBuff;

public class BrawlerBuffStack
{
  Dictionary<BrawlerBuffs, List<BrawlerBuff>> buffStack = new Dictionary<BrawlerBuffs, List<BrawlerBuff>>();

  public void AddBuff(BrawlerBuff stackItem)
  {
    if (!buffStack.ContainsKey(stackItem.Buff))
    {
      buffStack[stackItem.Buff] = new List<BrawlerBuff>();
    }
    buffStack[stackItem.Buff].Add(stackItem);
  }

  public void Update()
  {
    List<BrawlerBuff> toRemove = null;
    foreach (var b in buffStack)
    {
      toRemove?.Clear();
      int count = b.Value.Count;
      foreach (var buffInstance in b.Value)
      {
        bool removed = buffInstance.Tick(count);
        if (removed)
        {
          if (toRemove == null)
          {
            toRemove = new List<BrawlerBuff>();
          }
          toRemove.Add(buffInstance);
          --count;
        }
      }
      if (toRemove != null)
      {
        foreach (var s in toRemove)
        {
          b.Value.Remove(s);
        }
      }
    }
  }

  public void Clear()
  {
    foreach (var buffStack in buffStack)
    {
      int count = buffStack.Value.Count;
      foreach (var b in buffStack.Value)
      {
        b.Remove(count--);
      }
    }
    buffStack.Clear();
  }
}