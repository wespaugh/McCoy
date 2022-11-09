using Assets.McCoy.Localization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.RPG
{
  public class McCoyQuestTextUI : MonoBehaviour
  {
    [SerializeField]
    McCoyLocalizedText questTitle;

    [SerializeField]
    McCoyLocalizedText questText;

    [SerializeField]
    List<Button> buttons;

    private void Awake()
    {
      var quest = McCoy.GetInstance().gameState.activeQuest;
      if(quest == null)
      {
        gameObject.SetActive(false);
        return;
      }

      questTitle.SetText(quest.title);
      questText.SetText(quest.introText);
      if(quest.exitChoices.Count == 0)
      {
        buttons[0].gameObject.SetActive(true);
        buttons[0].GetComponent<McCoyLocalizedText>().SetText("com.mccoy.ui.quest.close");
        for(int i = 1; i < buttons.Count; ++i )
        {
          buttons[i].gameObject.SetActive(false);
        }
      }
      else
      {
        int i = 0;
        for (; i < quest.exitChoices.Count; ++i)
        {
          buttons[i].gameObject.SetActive(true);
          McCoyLocalizedText buttonText = buttons[i].GetComponent<McCoyLocalizedText>();
          buttonText.SetText(quest.exitChoices[i].displayText, (text) => {
            Debug.Log("UI callback: " + text);
            string[] components = text.Split(":");
            string resultText = "";
            for(int i = 1; i < components.Length; ++i)
            {
              resultText += components[i];
              if(i != 1 && i != components.Length - 1)
              {
                resultText += ":";
              }
            }
            buttonText.SetTextDirectly(components[0]);
            buttonText.GetComponent<Button>().onClick.AddListener( () =>
            {
              questText.SetTextDirectly(resultText);
            });
          });
        }
        for(; i < buttons.Count; ++i)
        {
          buttons[i].gameObject.SetActive(false);
        }
      }
    }
  }
}
