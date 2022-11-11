using Assets.McCoy.Localization;
using System;
using System.Collections;
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

    McCoyQuestData quest;

    private const string questClose = "com.mccoy.ui.quest.close";

    public void BeginQuest(McCoyQuestData quest)
    {
      StartCoroutine(yieldThenPause());
      questTitle.SetText(quest.title);
      questText.SetText(quest.introText);
      buttons[0].GetComponent<McCoyLocalizedText>().SetText(questClose);
      buttons[0].gameObject.SetActive(true);
      buttons[0].onClick.RemoveAllListeners();
      buttons[0].onClick.AddListener(() =>
      {
        UFE.PauseGame(false);
        gameObject.SetActive(false);
      });
      for(int i = 1; i < buttons.Count; ++i)
      {
        buttons[i].transform.parent.gameObject.SetActive(false);
      }
    }

    IEnumerator yieldThenPause()
    {
      float start = Time.time;
      while(Time.time < start + 3.0f)
      {
        yield return null;
      }
      UFE.timeScale = 0f;
    }

    public void QuestEnded()
    {
      questTitle.SetText(quest.title);
      questText.SetText(quest.exitText);
      if (quest.exitChoices.Count == 0)
      {
        buttons[0].gameObject.SetActive(true);
        buttons[0].GetComponent<McCoyLocalizedText>().SetText(questClose);
        for (int i = 1; i < buttons.Count; ++i)
        {
          buttons[i].transform.parent.gameObject.SetActive(false);
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
            string[] components = text.Split(":");
            string resultText = "";
            for (int i = 1; i < components.Length; ++i)
            {
              resultText += components[i];
              if (i != 1 && i != components.Length - 1)
              {
                resultText += ":";
              }
            }
            buttonText.SetTextDirectly(components[0]);
            buttonText.GetComponent<Button>().onClick.RemoveAllListeners();
            buttonText.GetComponent<Button>().onClick.AddListener(() =>
            {
              questText.SetTextDirectly(resultText);
              buttons[0].GetComponent<McCoyLocalizedText>().SetText(questClose);
              buttons[0].onClick.RemoveAllListeners();
              buttons[0].onClick.AddListener(() =>
              {
                UFE.timeScale = 1f;
                gameObject.SetActive(false);
              });
              for(int i = 1; i < buttons.Count; ++i)
              {
                buttons[i].transform.parent.gameObject.SetActive(false);
              }
            });
          });
        }
        for (; i < buttons.Count; ++i)
        {
          buttons[i].transform.parent.gameObject.SetActive(false);
        }
      }
    }
  }
}
