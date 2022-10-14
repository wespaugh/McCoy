using Assets.McCoy.Brawler.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.McCoy.Brawler.McCoyBuff;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.Brawler
{
  [CreateAssetMenu(fileName = "Buff", menuName = "McCoy/Buff")]
  public class McCoyBuff : ScriptableObject
  {
    public static McCoyBuffDelegate DelegateForBuff(McCoyBuffs buff)
    {
      switch(buff)
      {
        case McCoyBuffs.StancePhysicsChange:
          return new McCoyStancePhysicsChangeBuff();
      }
      return null;
    }

    public enum McCoyBuffs
    {
      StancePhysicsChange,
      Invalid,
    };

    public McCoyBuffs Buff;

    public List<string> stringArgs = new List<string>();
    public List<float> floatArgs = new List<float>();
    public List<int> intArgs = new List<int>();
    public List<bool> boolArgs = new List<bool>();
    public bool IsDebuff;

    private McCoyBuffDelegate buffDelegate;

    public void Init(ControlsScript player)
    {
      buffDelegate = DelegateForBuff(Buff);
      buffDelegate.Init(player, stringArgs, floatArgs, intArgs, boolArgs);
    }

    public void Apply(ControlsScript player)
    {
      buffDelegate.Apply();
    }
    public bool Update()
    {
      return buffDelegate.Update();
    }
    public void Remove()
    {
      buffDelegate.Remove();
    }
  }
  public class McCoyBuffStack
  {
    Dictionary<McCoyBuffs, List<McCoyBuff>> buffStack = new Dictionary<McCoyBuffs, List<McCoyBuff>>();

    public void AddBuff(McCoyBuff stackItem)
    {
      if(!buffStack.ContainsKey(stackItem.Buff))
      {
        buffStack[stackItem.Buff] = new List<McCoyBuff>();
      }
      buffStack[stackItem.Buff].Add(stackItem);
    }

    public void Update()
    {
      List<McCoyBuff> toRemove = null;
      foreach(var b in buffStack)
      {
        toRemove = null;
        foreach(var s in b.Value)
        {
          bool removed = s.Update();
          if(removed)
          {
            if(toRemove == null)
            {
              toRemove = new List<McCoyBuff>();
            }
            toRemove.Add(s);
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
      foreach(var buffStack in buffStack)
      {
        foreach(var b in buffStack.Value)
        {
          b.Remove();
        }
      }
      buffStack.Clear();
    }
  }
}
