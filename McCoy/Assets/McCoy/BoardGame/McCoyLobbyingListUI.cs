using Assets.McCoy.Localization;
using static Assets.McCoy.ProjectConstants;
using Assets.McCoy.UI;
using System;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.BoardGame
{
  public class McCoyLobbyingListUI : MonoBehaviour, IMcCoyInputManager
  {
    [SerializeField]
    GameObject lobbyingListItemPrefab = null;

    [SerializeField]
    RectTransform lobbyingContent = null;

    [SerializeField]
    ScrollRect scrollRect = null;

    [SerializeField]
    McCoyLocalizedText fundsText = null;

    List<GameObject> listItems = new List<GameObject>();
    int selectionIndex = 0;
    McCoyLobbyingListItem currentSelection = null;

    Action closeCallback = null;

    McCoyCityScreen city;
    private bool inputInitialized;
    private McCoyInputManager input;

    public void Initialize(McCoyCityScreen cityScreen, Action onClose)
    {
      city = cityScreen;
      closeCallback = onClose;
      while (listItems.Count > 0)
      {
        var item1 = listItems[0];
        listItems.RemoveAt(0);
        Destroy(item1);
      }
      var causes = McCoyLobbyingCauseManager.GetInstance().LobbyingCauses;
      causes.Sort((x, y) =>
      {
        return x.cost == y.cost ? 0 : (x.cost < y.cost ? -1 : 1);
      });
      Debug.Log("causes has " + causes.Count);
      foreach (var cause in causes)
      {
        var obj = Instantiate(lobbyingListItemPrefab, lobbyingContent);
        listItems.Add(obj);
        obj.GetComponent<McCoyLobbyingListItem>().Initialize(cause, city);
      }
      updateSelection();
    }

    public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if (!inputInitialized)
      {
        inputInitialized = true;
        input = new McCoyInputManager();
        input.RegisterButtonListener(ButtonPress.Button2, lobbyForCause);
        input.RegisterButtonListener(ButtonPress.Button3, close);
        input.RegisterButtonListener(ButtonPress.Up, MoveUp);
        input.RegisterButtonListener(ButtonPress.Down, MoveDown);
      }
      return input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }

    private void close()
    {
      closeCallback();
    }

    private void MoveUp()
    {
      --selectionIndex;
      if(selectionIndex < 0)
      {
        selectionIndex = listItems.Count - 1;
      }
      updateSelection();
    }

    private void MoveDown()
    {
      ++selectionIndex;
      if (selectionIndex >= listItems.Count)
      {
        selectionIndex = 0;
      }
      updateSelection();
    }

    private void updateSelection()
    {
      if (currentSelection != null)
      {
        currentSelection.ToggleHighlight(false);
      }
      currentSelection = listItems[selectionIndex].GetComponent<McCoyLobbyingListItem>();
      currentSelection.ToggleHighlight(true);

      Vector3 selectionPosition = listItems[selectionIndex].transform.position;
      var anch = lobbyingContent.anchoredPosition;
      var newPos = (Vector2)scrollRect.transform.InverseTransformPoint(lobbyingContent.position)
              - (Vector2)scrollRect.transform.InverseTransformPoint(selectionPosition);
      anch.y = newPos.y;
      lobbyingContent.anchoredPosition = anch;
      updateFundsText();
    }

    private void lobbyForCause()
    {
      currentSelection.LobbyForCause();
      updateFundsText();
    }

    private void updateFundsText()
    {
      Debug.Log(McCoyGameState.Instance().Credits);
      Localize("com.mccoy.boardgame.availablefunds", (labelText) =>
      {
        string costLabel = $"{McCoyGameState.Instance().Credits}";
        fundsText.SetTextDirectly(labelText + ": " + costLabel);
      });
    }
  }
}
