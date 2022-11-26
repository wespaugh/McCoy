using Assets.McCoy.Brawler.Stages;
using Assets.McCoy.RPG;
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

    [SerializeField]
    GameObject mainMenuBuildingsPrefab = null;
    [SerializeField]
    GameObject imageBackground = null;

    List<GameObject> buildings = null;

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
      McCoyQuestManager.GetInstance().ClearQuestData();
      buildings = new List<GameObject>();
      var bg = Instantiate(imageBackground);
      bg.transform.position = new Vector3(18f, 3.6f, 20f);
      buildings.Add(bg);
      for (int i = 0; i < 4; ++i)
      {
        var building = Instantiate(mainMenuBuildingsPrefab);
        // building.GetComponent<SpriteRenderer>().enabled = i == 2;
        foreach (var sprite in building.GetComponentsInChildren<SpriteRenderer>())
        {
          sprite.sortingOrder = -i;
          float color = .2f + (.2f * (2 - i));
          sprite.color = new Color(color, color, color);
          sprite.transform.position = new Vector3(-.5f, (.7f * i), 20f);
          sprite.transform.localScale = new Vector3(2f + (.1f * i), 2f + (.1f * i), 2f + (.1f * i));
        }
        float scrollSpeed = i == 0 ? 1f : (i == 1 ? .8f : (i == 2 ? 0.6f : .4f));
        // scrollSpeed *= -1;
        building.GetComponent<McCoyRandomSpriteParallaxItem>().autoScrollSpeed = scrollSpeed;
        buildings.Add(building);
      }
    }

    private void OnDestroy()
    {
      while(buildings.Count > 0)
      {
        var b = buildings[0];
        buildings.RemoveAt(0);
        Destroy(b);
      }
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
      McCoyQuestManager.GetInstance().GameLoaded();
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