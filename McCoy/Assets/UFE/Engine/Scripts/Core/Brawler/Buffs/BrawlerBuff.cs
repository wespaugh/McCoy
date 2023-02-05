using System.Collections.Generic;
using UnityEngine;
using Assets.McCoy.Brawler.Buffs;
using static Assets.McCoy.Brawler.BrawlerBuff;
using System;
using UFE3D;

namespace Assets.McCoy.Brawler
{
  [CreateAssetMenu(fileName = "Buff", menuName = "UFE/Buff")]
  public class BrawlerBuff : ICloneable
  {
    public static BrawlerBuffDelegate DelegateForBuff(BrawlerBuffs buff)
    {
      switch(buff)
      {
        case BrawlerBuffs.StancePhysicsChange:
          return new BrawlerStancePhysicsChangeBuff();
      }
      return null;
    }

    public enum BrawlerBuffs
    {
      Invalid,
      StancePhysicsChange,
    };

    public BrawlerBuffs Buff;
    public int castingFrame = -1;
    public List<string> stringArgs = new List<string>();
    public List<float> floatArgs = new List<float>();
    public List<int> intArgs = new List<int>();
    public List<bool> boolArgs = new List<bool>();
    public bool IsDebuff;

    private BrawlerBuffDelegate buffDelegate;

    public void Init(ControlsScript player)
    {
      buffDelegate = DelegateForBuff(Buff);
      buffDelegate.Init(player, stringArgs, floatArgs, intArgs, boolArgs);
    }

    public void Apply(ControlsScript player)
    {
      buffDelegate.Apply();
    }
    public bool Tick(int currentBuffCount)
    {
      return buffDelegate.Tick(currentBuffCount);
    }
    public void Remove(int count)
    {
      buffDelegate.Remove(count);
    }

    public object Clone()
    {
      return CloneObject.Clone(this);
    }
  }
}
