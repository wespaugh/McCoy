using Assets.McCoy.BoardGame;
using Assets.McCoy.RPG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.UI.McCoyDebug
{
  public class McCoyDebugLevelSelect : MonoBehaviour
  {
    [SerializeField]
    TMP_Dropdown characterSelect = null;

    [SerializeField]
    TMP_Dropdown levelSelect = null;

    [SerializeField]
    Button startStage = null;

    List<PlayerCharacter> pcIndexes = new List<PlayerCharacter>();
    List<string> levelIndexes = new List<string>();

    PlayerCharacter selectedCharacter;
    string selectedStage;

    public void OnEnable()
    { 
      var characters = UFE.config.characters;
      List<TMP_Dropdown.OptionData> characterOptions = new List<TMP_Dropdown.OptionData>();

      pcIndexes.Clear();

      foreach(PlayerCharacter c in Enum.GetValues(typeof(PlayerCharacter)))
      {
        string playerName = PlayerName(c);
        foreach (var characterData in UFE.config.characters)
        {
          if (string.Compare(playerName, characterData.characterName, StringComparison.OrdinalIgnoreCase) == 0)
          {
            characterOptions.Add(new TMP_Dropdown.OptionData(playerName));
            pcIndexes.Add(c);
          }
        }
      }
      characterSelect.ClearOptions();
      characterSelect.AddOptions(characterOptions);
      characterSelect.onValueChanged.RemoveAllListeners();
      characterSelect.onValueChanged.AddListener((index) =>
      {
        selectedCharacter = pcIndexes[index];
      });

      levelIndexes.Clear();
      List<TMP_Dropdown.OptionData> levelOptions = new List<TMP_Dropdown.OptionData>();
      foreach(var level in UFE.config.stages)
      {
        levelOptions.Add(new TMP_Dropdown.OptionData(level.stageName));
        levelIndexes.Add(level.stageName);
      }
      levelSelect.ClearOptions();
      levelSelect.AddOptions(levelOptions);
      levelSelect.onValueChanged.RemoveAllListeners();
      levelSelect.onValueChanged.AddListener((index) =>
      {
        selectedStage = levelIndexes[index];
      });
    }

    public void StartStage()
    {

      Brawler.McCoyStageData stageData = new Brawler.McCoyStageData();
      List<McCoyMobData> mobs = new List<McCoyMobData>();
      
      Factions f;
      switch (UnityEngine.Random.Range(1, 4))
      {
        case 1:
          f = Factions.Mages;
          break;
        case 2:
          f = Factions.AngelMilitia;
          break;
        case 3:
          f = Factions.CyberMinotaurs;
          break;
        default:
          f = Factions.CyberMinotaurs;
          break;
      }

      mobs.Add(new McCoyMobData(f, 1, 1));
      stageData.Initialize(selectedStage, mobs);
      McCoyGameState.Instance().Initialize(new List<MapNode>());
      McCoy.GetInstance().LoadBrawlerStage(stageData, selectedCharacter);
    }
  }
}
