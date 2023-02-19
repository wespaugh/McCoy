using JetBrains.Annotations;
using UnityEngine;

namespace Assets.McCoy.Brawler.Buffs
{
  public class BrawlerDashingBuff : BrawlerBuffDelegate
  {

    bool log = false;

    public override string EditorHelper()
    {
      return "";
    }

    public override void Apply()
    {
      duration = .2f;
      base.Apply();
      player.isDashing = true;
    }

    public override bool Tick(int currentBuffCount)
    {
      // only remove if back at resting
      if (player.currentSubState != SubStates.Resting)
      {
        return false;
      }
      return base.Tick(currentBuffCount);
    }

    public override void Remove(int numAppliedBeforeRemoving)
    {
      base.Remove(numAppliedBeforeRemoving);
      if(numAppliedBeforeRemoving == 1)
      {
        player.isDashing = false;
      }
    }
  }
}
