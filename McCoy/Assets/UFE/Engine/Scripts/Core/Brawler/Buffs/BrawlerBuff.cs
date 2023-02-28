using System.Collections.Generic;
using UnityEngine;
using Assets.McCoy.Brawler.Buffs;
using System;

namespace UFE3D.Brawler
{
  [CreateAssetMenu(fileName = "Buff", menuName = "UFE/Buff")]
  [System.Serializable]
  public class BrawlerBuff : ICloneable
  {
    public static BrawlerBuffDelegate DelegateForBuff(BrawlerBuffs buff)
    {
      switch(buff)
      {
        case BrawlerBuffs.StancePhysicsChange:
          return new BrawlerStancePhysicsChangeBuff();
        case BrawlerBuffs.Dash:
          return new BrawlerDashingBuff();
      }
      return null;
    }

    public enum BrawlerBuffs
    {
      Invalid,
      StancePhysicsChange,
      Dash,
    };

    public BrawlerBuffs Buff;
    public int castingFrame = -1;
    public bool casted = false;
    public List<string> stringArgs = new List<string>();
    public List<float> floatArgs = new List<float>();
    public List<int> intArgs = new List<int>();
    public List<bool> boolArgs = new List<bool>();
    public bool IsDebuff;

    [NonSerialized]
    private BrawlerBuffDelegate buffDelegate;

    public void Init(ControlsScript player)
    {
      buffDelegate = DelegateForBuff(Buff);
      buffDelegate.Init(player, stringArgs, floatArgs, intArgs, boolArgs);
    }

    /*
    public void Apply(ControlsScript player)
    {
      buffDelegate.Apply();
    }
    */
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
