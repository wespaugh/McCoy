using FPLibrary;
using System;
using UFE3D;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy
{
  public class McCoy : MonoBehaviour
  {
    public UFEScreen cityScene;
    public enum McCoyScenes
    {
      CityMap,
      Brawler
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