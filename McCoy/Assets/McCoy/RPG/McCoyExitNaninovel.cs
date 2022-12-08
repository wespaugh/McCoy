using Assets.McCoy;
using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("McCoyEndScene")]
public class SwitchToAdventureMode : Command
{
  [ParameterAlias("reset")]
  public BooleanParameter ResetState = true;

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

    // 6. Enable character control.
    McCoy.GetInstance().HideCutsceneAsync();
  }
}