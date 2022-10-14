using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.McCoy.Brawler.Buffs
{
  public class McCoyStancePhysicsChangeBuff : McCoyBuffDelegate
  {
    int stanceNumber => intArgs[0];
    int amountDelta => intArgs[1];


    // TODO:: these need to not modify the physics values directly as they will be modifying the RAW save data. made deltas alongside the raw data that get cleared at the start of each round.
    public override void Apply()
    {
      base.Apply();
      foreach (MoveSetData moveSetData in player.loadedMoves)
      {
        bool foundStance = false;
        foundStance |= stanceNumber == 1 && moveSetData.combatStance == CombatStances.Stance1;
        foundStance |= stanceNumber == 2 && moveSetData.combatStance == CombatStances.Stance2;
        foundStance |= stanceNumber == 3 && moveSetData.combatStance == CombatStances.Stance3;

        if(foundStance)
        {
          moveSetData.physics._moveForwardSpeedBonus += amountDelta;
          moveSetData.physics._moveSidewaysSpeedBonus += amountDelta;
        }
      }
    }
    public override void Remove()
    {
      base.Remove();
      foreach (MoveSetData moveSetData in player.loadedMoves)
      {
        bool foundStance = false;
        foundStance |= stanceNumber == 1 && moveSetData.combatStance == CombatStances.Stance1;
        foundStance |= stanceNumber == 2 && moveSetData.combatStance == CombatStances.Stance2;
        foundStance |= stanceNumber == 3 && moveSetData.combatStance == CombatStances.Stance3;

        if (foundStance)
        {
          moveSetData.physics._moveForwardSpeed -= amountDelta;
          moveSetData.physics._moveSidewaysSpeed -= amountDelta;
        }
      }
    }
  }
}
