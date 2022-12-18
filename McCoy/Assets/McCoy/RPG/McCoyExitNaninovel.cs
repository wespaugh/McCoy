using Assets.McCoy;
using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("McCoyEndScene")]
public class SwitchToAdventureMode : Command
{
  [ParameterAlias("reset")]
  public BooleanParameter ResetState = true;

  [ParameterAlias("RewardCredits")]
  public IntegerParameter RewardCredits;

  [ParameterAlias("RewardTime")]
  public IntegerParameter RewardTimerSeconds;

  public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
  {
    // 1. Disable Naninovel input.
    var inputManager = Engine.GetService<IInputManager>();
    inputManager.ProcessInput = false;

    // 2. Stop script player.
    var scriptPlayer = Engine.GetService<IScriptPlayer>();
    scriptPlayer.Stop();

    // 3. Hide text printer.
    var hidePrinter = new HidePrinter();
    hidePrinter.ExecuteAsync(asyncToken).Forget();

    // 4. Reset state (if required).
    if (ResetState)
    {
      var stateManager = Engine.GetService<IStateManager>();
      await stateManager.ResetStateAsync();
    }

    McCoy.GetInstance().QuestReward(RewardCredits.HasValue ? RewardCredits.Value : 0, RewardTimerSeconds.HasValue ? RewardTimerSeconds.Value : 0);

    // 6. Enable character control.
    McCoy.GetInstance().HideCutsceneAsync();
  }
}