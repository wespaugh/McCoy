namespace Assets.McCoy.Brawler.Buffs
{
  public class BrawlerStancePhysicsChangeBuff : BrawlerBuffDelegate
  {
    int stanceNumber => intArgs[0];
    int amountDelta => intArgs[1];

    bool log = false;

    public override string EditorHelper()
    {
      return "Stance Number|i,Amount Delta|i";
    }

    public override void Apply()
    {
      base.Apply();
      foreach (MoveSetData moveSetData in player.loadedMoves)
      {
        bool foundStance = false;
        foundStance |= stanceNumber == 1 && moveSetData.combatStance == CombatStances.Stance1;
        foundStance |= stanceNumber == 2 && moveSetData.combatStance == CombatStances.Stance2;
        foundStance |= stanceNumber == 3 && moveSetData.combatStance == CombatStances.Stance3;

        if (foundStance)
        {
          if (log)
          {
            UnityEngine.Debug.Log("Changing " + moveSetData.combatStance + " speed, " + moveSetData.physics._moveForwardSpeedBonus + "=>" + (moveSetData.physics._moveForwardSpeedBonus += amountDelta));
            UnityEngine.Debug.Log("Changing " + moveSetData.combatStance + " speed, " + moveSetData.physics._moveForwardSpeedBonus + "=>" + (moveSetData.physics._moveForwardSpeedBonus += amountDelta));
            UnityEngine.Debug.Log("Changing " + moveSetData.combatStance + " speed, " + moveSetData.physics._moveForwardSpeedBonus + "=>" + (moveSetData.physics._moveForwardSpeedBonus += amountDelta));
            UnityEngine.Debug.Log("Changing " + moveSetData.combatStance + " speed, " + moveSetData.physics._moveForwardSpeedBonus + "=>" + (moveSetData.physics._moveForwardSpeedBonus += amountDelta));
            UnityEngine.Debug.Log("Changing " + moveSetData.combatStance + " speed, " + moveSetData.physics._moveForwardSpeedBonus + "=>" + (moveSetData.physics._moveForwardSpeedBonus += amountDelta));
          }
          moveSetData.physics._moveForwardSpeedBonus += amountDelta;
          moveSetData.physics._moveSidewaysSpeedBonus += amountDelta;
        }
      }
    }
    public override void Remove(int numAppliedBeforeRemoving)
    {
      base.Remove(numAppliedBeforeRemoving);
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
