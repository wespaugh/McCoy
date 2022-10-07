using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using FPLibrary;
using System;
using System.Collections.Generic;
using UFE3D;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy
{
  public class McCoy : MonoBehaviour
  {
    public bool Debug = true;
    public bool debugCheatWin = false;
    public UFEScreen cityScene;
    public enum McCoyScenes
    {
      CityMap,
      Brawler
    }

    static McCoy instance = null;

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
      UFE.StartBrawlerMode();
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
  }
}