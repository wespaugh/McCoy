using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UFE3D;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
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

        // deleteSavesButton.gameObject.SetActive(game.Debug);
        if (game.Debug)
        {
          // StartCityScene();
        }
      }
      updateMenuItems();
    }

    public override void DoFixedUpdate(
  IDictionary<InputReferences, InputEvents> player1PreviousInputs,
  IDictionary<InputReferences, InputEvents> player1CurrentInputs,
  IDictionary<InputReferences, InputEvents> player2PreviousInputs,
  IDictionary<InputReferences, InputEvents> player2CurrentInputs
)
    {
      base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
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
      bool continueAvailable = File.Exists(ProjectConstants.SaveFilename(1));
      // continueButton.gameObject.SetActive(continueAvailable);
    }
  }
}