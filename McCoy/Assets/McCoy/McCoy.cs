using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using Assets.McCoy.RPG;
using FPLibrary;
using Naninovel;
using System.Threading.Tasks;
using UFE3D;
using UFE3D.Brawler;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy
{
  public class McCoy : MonoBehaviour
  {
    public bool DebugUI = false;
    public bool DebugSpawnsOnly = false;
    public bool DebugOneHitKills = false;
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

    BrawlerBuffManager buffManager = null;
    public BrawlerBuffManager BuffManager
    {
      get
      {
        if(buffManager == null)
        {
          buffManager = GetComponent<BrawlerBuffManager>();
        }
        return buffManager;
      }
    }

    public void QuestReward(int rewardCredits, int rewardTime, string rewardItem)
    {
      gameState.ReceiveCredits(rewardCredits);
      gameState.UpdatePlayerTimeRemaining(gameState.SelectedPlayer, -rewardTime);
      McCoyEquipmentItem rewardEquipment = null;
      if(!string.IsNullOrEmpty(rewardItem)) 
      {
        if (rewardItem == "Random")
        {
          rewardEquipment = McCoyEquipmentGenerator.GetRandomItem();
        }
      }
      if (rewardEquipment != null)
      {
        gameState.GetPlayerCharacter(gameState.SelectedPlayer).Equipment.AddEquipment(rewardEquipment);
      }
    }

    public bool Loading { get; set; }

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

    public void LoadBrawlerStage(McCoyStageData stageData, PlayerCharacter player)
    {
      currentStage = stageData;
      UFE.config.currentRound = 1;
      var components = stageData.Name.Split('.');
      int pcIndex;
      switch(player)
      {
        case PlayerCharacter.Rex:
        case PlayerCharacter.Vicki:
          pcIndex = 0;
          break;
        default:
          pcIndex = 1;
          break;
      }
      UFE.StartBrawlerMode(components[components.Length-1], pcIndex);
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
      var cameraData = Camera.main.GetUniversalAdditionalCameraData();
      cameraData.cameraStack.Add(c);

      naniCam = c;
      // inactive by default
      naniCam.gameObject.SetActive(false);
    }
    public async Task ShowCutsceneAsync(string scriptName)
    {

      var player = Engine.GetService<IScriptPlayer>();
      await player.PreloadAndPlayAsync(scriptName);

      naniCam.gameObject.SetActive(true);

      var inputManager = Engine.GetService<IInputManager>();
      inputManager.ProcessInput = true;
    }
    public void HideCutsceneAsync()
    {
      naniCam.gameObject.SetActive(false);
      UFE.FireAlert(ProjectConstants.CUTSCENE_FINISHED, null);
    }
  }
}