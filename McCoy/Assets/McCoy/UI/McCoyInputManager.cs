﻿using System;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyInputManager : IMcCoyInputManager
  {
    Dictionary<ButtonPress, Action> listeners = new Dictionary<ButtonPress, Action>();

    float currentInputLag = 0f;
    float inputLag = .2f;

    public void RegisterButtonListener(ButtonPress b, Action a)
    {
      listeners[b] = a;
    }

    public bool CheckInputs(
        IDictionary<InputReferences, InputEvents> player1PreviousInputs,
        IDictionary<InputReferences, InputEvents> player1CurrentInputs,
        IDictionary<InputReferences, InputEvents> player2PreviousInputs,
        IDictionary<InputReferences, InputEvents> player2CurrentInputs
      )
    {
      if (currentInputLag > 0f)
      {
        currentInputLag -= Time.deltaTime;
        return false;
      }

      bool retVal = false;
      // detect axis inputs for the purpose of lag detection and falling back onto default input
      foreach (KeyValuePair<InputReferences, InputEvents> pair in player1CurrentInputs)
      {
        if ((pair.Key.inputType == InputType.VerticalAxis && pair.Value.axisRaw != 0) || (pair.Key.inputType == InputType.HorizontalAxis && pair.Value.axisRaw != 0) )
        {
          retVal = true;
        }
      }

      foreach (var listener in listeners)
      {
        bool alreadyPressed = false;
        foreach (KeyValuePair<InputReferences, InputEvents> pair in player1PreviousInputs)
        {
          ButtonPress bp = ButtonPress.Start;
          bool buttonPressed = true;
          if(pair.Key.inputType == InputType.Button && pair.Value.button)
          {
            bp = pair.Key.engineRelatedButton;
          }
          else if(pair.Key.inputType == InputType.HorizontalAxis && pair.Value.axisRaw != 0f)
          {
            bp = pair.Value.axisRaw >= 0 ? ButtonPress.Forward : ButtonPress.Back;
          }
          else if (pair.Key.inputType == InputType.VerticalAxis && pair.Value.axisRaw != 0f)
          {
            bp = pair.Value.axisRaw >= 0 ? ButtonPress.Up : ButtonPress.Down;
          }
          else
          { 
            buttonPressed = false;
          }
          if (buttonPressed && bp == listener.Key)
          {
            alreadyPressed = true;
            break;
          }
        }
        bool currentlyPressed = false;
        foreach (KeyValuePair<InputReferences, InputEvents> pair in player1CurrentInputs)
        {
          ButtonPress bp = ButtonPress.Start;
          bool buttonPressed = true;
          if (pair.Key.inputType == InputType.Button && pair.Value.button)
          {
            bp = pair.Key.engineRelatedButton;
          }
          else if (pair.Key.inputType == InputType.HorizontalAxis && pair.Value.axisRaw != 0f)
          {
            bp = pair.Value.axisRaw >= 0 ? ButtonPress.Forward : ButtonPress.Back;
          }
          else if (pair.Key.inputType == InputType.VerticalAxis && pair.Value.axisRaw != 0f)
          {
            bp = pair.Value.axisRaw >= 0 ? ButtonPress.Up : ButtonPress.Down;
          }
          else
          {
            buttonPressed = false;
          }
          if (buttonPressed && bp == listener.Key)
          {
            currentlyPressed = true;
            break;
          }
        }
        if(!alreadyPressed && currentlyPressed)
        {
          retVal = true;
          listener.Value();
        }
      }
      if(retVal)
      {
        currentInputLag = inputLag;
      }
      return retVal;
    }
  }
}
