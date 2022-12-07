﻿using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using Assets.McCoy.RPG;
using FPLibrary;
using Naninovel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UFE3D;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy
{
  public class McCoy : MonoBehaviour
  {
    public bool Debug = true;
    public bool debugCheatWin = false;
    public bool levelAllPlayersEvenly = false;
    public UFEScreen cityScene;
    private Camera naniCam;

    public enum McCoyScenes
    {
      CityMap,
      Brawler
    }

    static McCoy instance = null;

    McCoySkillMoveInfoLookup skillLookup;
    public McCoySkillMoveInfoLookup SkillLookup
    {
      get
      {
        if(skillLookup == null)
        {
          skillLookup = GetComponent<McCoySkillMoveInfoLookup>();
        }
        return skillLookup;
      }
    }

    McCoyCharacterBuffManager buffManager = null;
    public McCoyCharacterBuffManager BuffManager
    {
      get
      {
        if(buffManager == null)
        {
          buffManager = GetComponent<McCoyCharacterBuffManager>();
        }
        return buffManager;
      }
    }

    public McCoyStageData currentStage = null;
    public McCoyGameState gameState = null;

    public static McCoy GetInstance()
    {
      if(instance == null)
      {
        instance = GameObject.Find("UFE Manager").GetComponent<McCoy>();
      }
      return instance;
    }

    public void LoadBrawlerStage(McCoyStageData stageData)
    {
      currentStage = stageData;
      UFE.config.currentRound = 1;
      UFE.StartBrawlerMode(stageData.Name);
    }

    public void LoadScene( McCoyScenes scene, float fadeTime = 2.0f)
    {
      if (UFE.currentScreen != null && UFE.currentScreen.hasFadeOut)
      {
        UFE.eventSystem.enabled = false;
        CameraFade.StartAlphaFade(
            UFE.config.gameGUI.screenFadeColor,
            false,
            fadeTime / 2f,
            0f
        );
        UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; loadScene(scene, fadeTime / 2f); }, (Fix64)fadeTime / 2);
      }
      else
      {
        loadScene(scene, fadeTime / 2f);
      }
    }

    private void loadScene(McCoyScenes scene, float fadeTime)
    {
      UFE.HideScreen(UFE.currentScreen);
      switch (scene)
      {
        case McCoyScenes.CityMap:
          UFE.ShowScreen(cityScene);
          break;
      }
      CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
    }

    public void SetNaniCam(Camera c)
    {
      naniCam = c;
      // inactive by default
      naniCam.gameObject.SetActive(false);
    }
    public async Task ShowCutsceneAsync(string scriptName)
    {
      var player = Engine.GetService<IScriptPlayer>();
      await player.PreloadAndPlayAsync(scriptName);

      naniCam.gameObject.SetActive(true);
    }
    public async Task HideCutsceneAsync()
    {
      // 1. Disable Naninovel input.

      // 2. Stop script player.
      var scriptPlayer = Engine.GetService<IScriptPlayer>();
      scriptPlayer.Stop();

      // 3. Reset state.
      var stateManager = Engine.GetService<IStateManager>();
      await stateManager.ResetStateAsync();

      // 4. Switch cameras.
      naniCam.enabled = false;
    }
  }
}