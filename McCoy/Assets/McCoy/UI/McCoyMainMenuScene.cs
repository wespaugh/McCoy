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
using Naninovel;
using System.Collections;
using TMPro;

namespace Assets.McCoy.UI
{
  public class McCoyMainMenuScene : DefaultMainMenuScreen, IMcCoyInputManager
  {
    [SerializeField]
    Button continueButton = null;

    [SerializeField]
    Button newGameButton = null;

    [SerializeField]
    Button optionsButton = null;

    [SerializeField]
    Button exitButton = null;

    [SerializeField]
    Button deleteSavesButton = null;

    [SerializeField]
    GameObject mainMenuBuildingsPrefab = null;
    [SerializeField]
    GameObject imageBackground = null;

    List<GameObject> buildings = null;

    enum MainMenuChoices
    {
      Continue,
      NewGame,
      Options,
      Exit
    }
    MainMenuChoices currentSelection;

    McCoy game;
    private bool inputInitialized;
    private McCoyInputManager input;

    public void Awake()
    {
      if(game == null)
      {
        game = FindObjectOfType<McCoy>();

        // deleteSavesButton.gameObject.SetActive(game.Debug);
        if (game.DebugUI)
        {
          // StartCityScene();
        }
      }
      StartCoroutine(initializeNaniNovel());
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
      while (buildings.Count > 0)
      {
        var b = buildings[0];
        buildings.RemoveAt(0);
        Destroy(b);
      }
    }

    private IEnumerator initializeNaniNovel()
    {
      while(!Engine.Initialized)
      {
        yield return null;
      }
      McCoy.GetInstance().SetNaniCam(GameObject.Find("NaniCam(Clone)").GetComponent<Camera>());// GameObject.Find("NaniUICamera(Clone)").GetComponent<Camera>());
      // _ = McCoy.GetInstance().ShowCutsceneAsync("Rex_01_Outro");
    }

      public void StartCityScene()
    {
      McCoyQuestManager.GetInstance().GameLoaded();
      game.LoadScene(McCoy.McCoyScenes.CityMap);
    }
    public void LoadGame()
    {
      McCoy.GetInstance().gameState.Load();
      McCoy.GetInstance().Loading = true;
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
      continueButton.gameObject.SetActive(continueAvailable);
      currentSelection = continueAvailable ? MainMenuChoices.Continue : MainMenuChoices.NewGame;
      updateButtons();
    }

    private void updateButtons()
    {
      switch (currentSelection)
      {
        case MainMenuChoices.Continue:
          continueButton.Select();
          break;
        case MainMenuChoices.NewGame:
          newGameButton.Select();
          break;
        case MainMenuChoices.Options:
          optionsButton.Select();
          break;
        case MainMenuChoices.Exit:
        default:
          exitButton.Select();
          break;
      }
      // buttonText.color = Color.green;
    }

    public override void DoFixedUpdate(
    IDictionary<InputReferences, InputEvents> player1PreviousInputs,
    IDictionary<InputReferences, InputEvents> player1CurrentInputs,
    IDictionary<InputReferences, InputEvents> player2PreviousInputs,
    IDictionary<InputReferences, InputEvents> player2CurrentInputs
      )
    {
      CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }

      public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if (!inputInitialized)
      {
        inputInitialized = true;
        input = new McCoyInputManager();
        input.RegisterButtonListener(ButtonPress.Button2, confirmMenuChoice);
        input.RegisterButtonListener(ButtonPress.Up, navigateUp);
        input.RegisterButtonListener(ButtonPress.Down, navigateDown);
      }
      return input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }

    private void navigateUp()
    {
      switch(currentSelection)
      {
        case MainMenuChoices.Continue:
          currentSelection = MainMenuChoices.Exit;
          break;
        case MainMenuChoices.NewGame:
          currentSelection = continueButton.gameObject.activeInHierarchy ? MainMenuChoices.Continue : MainMenuChoices.Exit;
          break;
        case MainMenuChoices.Options:
          currentSelection = MainMenuChoices.NewGame;
          break;
        case MainMenuChoices.Exit:
          currentSelection = MainMenuChoices.Options;
          break;
      }
      updateButtons();
    }

    private void navigateDown()
    {
      switch (currentSelection)
      {
        case MainMenuChoices.Continue:
          currentSelection = MainMenuChoices.NewGame;
          break;
        case MainMenuChoices.NewGame:
          currentSelection = MainMenuChoices.Options;
          break;
        case MainMenuChoices.Options:
          currentSelection = MainMenuChoices.Exit;
          break;
        case MainMenuChoices.Exit:
          currentSelection = continueButton.gameObject.activeInHierarchy ? MainMenuChoices.Continue : MainMenuChoices.NewGame;
          break;
      }
      updateButtons();
    }

    private void confirmMenuChoice()
    {
      switch (currentSelection)
      {
        case MainMenuChoices.Continue:
          LoadGame();
          break;
        case MainMenuChoices.NewGame:
          DeleteAllSaves();
          StartCityScene();
          break;
        case MainMenuChoices.Options:
          Debug.Log("Options!");
          break;
        case MainMenuChoices.Exit:
          Debug.Log("Exit!");
          break;
      }
    }
  }
}