using Assets.McCoy.UI;
using System;
using System.Collections.Generic;
using UFE3D;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.RPG
{
  public class McCoyEquipmentMenu : MonoBehaviour, IMcCoyInputManager
  {
    Animator characterAnimator = null;
    private bool inputInitialized;
    private McCoyInputManager input;

    public void Initialize(Animator characterAnimator)
    {
      this.characterAnimator = characterAnimator;
      Debug.Log("play rex idle");
      characterAnimator.Play("rex_idle");
    }

    public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if (!inputInitialized)
      {
        inputInitialized = true;
        input = new McCoyInputManager();
        input.RegisterButtonListener(ButtonPress.Forward, navRight);
        input.RegisterButtonListener(ButtonPress.Back, navLeft);
        input.RegisterButtonListener(ButtonPress.Up, navUp);
        input.RegisterButtonListener(ButtonPress.Down, navDown);
      }
      bool retVal = input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
      if(retVal)
      {
        Debug.Log("equipment page handled input");
      }
      return retVal;
    }

    private void navRight()
    {
      Debug.Log("Nav Right");
    }

    private void navLeft()
    {
      Debug.Log("Nav Left");
    }

    private void navUp()
    {
      Debug.Log("Nav Up");
    }

    private void navDown()
    {
      Debug.Log("Nav Down");
    }
  }
}