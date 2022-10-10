using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.UI
{
  public class McCoyMainMenuScene : DefaultMainMenuScreen
  {
    [SerializeField]
    Button continueButton = null;

    [SerializeField]
    Button deleteSavesButton = null;

    McCoy game;
    public void Awake()
    {
      if(game == null)
      {
        game = FindObjectOfType<McCoy>();

        deleteSavesButton.gameObject.SetActive(game.Debug);
        if (game.Debug)
        {
          // StartCityScene();
        }
      }
      updateMenuItems();
    }
    public void StartCityScene()
    {
      game.LoadScene(McCoy.McCoyScenes.CityMap);
    }
    public void LoadGame()
    {
      McCoy.GetInstance().gameState.Load();
      StartCityScene();
    }

    public void DeleteAllSaves()
    {
      if (File.Exists(ProjectConstants.SaveFilename(1)))
      {
        // If file found, delete it    
        File.Delete(ProjectConstants.SaveFilename(1));
      }
      updateMenuItems();
    }

    private void updateMenuItems()
    {
      continueButton.gameObject.SetActive(File.Exists(ProjectConstants.SaveFilename(1)));
    }
  }
}