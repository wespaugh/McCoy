using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.McCoy.Brawler.McCoyBuff;

namespace Assets.McCoy.Brawler.Buffs
{
  public abstract class McCoyBuffDelegate
  {
    // -1 is indefinite
    float duration = -1f;
    public float AppliedTime;

    // buff will tick this number of times evenly across the duration
    public float tickCount = 1;

    public int numTicks;

    protected List<string> stringArgs = new List<string>();
    protected List<float> floatArgs = new List<float>();
    protected List<int> intArgs = new List<int>();
    protected List<bool> boolArgs = new List<bool>();

    protected ControlsScript player;

    public void Init(ControlsScript player, List<string> stringArgs, List<float> floatArgs, List<int> intArgs, List<bool> boolArgs)
    {
      this.player = player;
      this.stringArgs = stringArgs;
      this.floatArgs = floatArgs;
      this.intArgs = intArgs;
      this.boolArgs = boolArgs;
      Apply();
    }

      /// <summary>
      /// Updates whatever needs updating on this buff
      /// </summary>
      /// <returns></returns>
      public virtual bool Update()
    {
      if (duration < 0)
      {
        return false;
      }

      if (duration > 0)
      {
        float actualNumTicks = Time.time - AppliedTime / (duration / tickCount);
        while (actualNumTicks > numTicks)
        {
          tick();
        }
      }
      bool unapply = duration > 0 && Time.time > AppliedTime + duration;
      if (unapply)
      {
        Remove();
      }
      return unapply;
    }

    protected virtual void tick()
    {
      ++numTicks;
    }

    public virtual void Apply()
    {
      AppliedTime = Time.time;
      numTicks = 0;
    }
    public virtual void Remove()
    {

    }
  }
}
