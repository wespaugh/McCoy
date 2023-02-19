using Assets.McCoy.Localization;
using static Assets.McCoy.ProjectConstants;
using Assets.McCoy.UI;
using System;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;
using UnityEngine.UI;
using Assets.McCoy.RPG;

namespace Assets.McCoy.BoardGame
{
  // TODO: Unify this with LobbyingList
  public class McCoyShopListUI : MonoBehaviour, IMcCoyInputManager
  {
    [SerializeField]
    GameObject shopItemPrefab = null;

    [SerializeField]
    RectTransform shopContent = null;

    [SerializeField]
    ScrollRect scrollRect = null;

    [SerializeField]
    McCoyLocalizedText fundsText = null;

    List<GameObject> listItems = new List<GameObject>();
    int selectionIndex = 0;
    McCoyShopItem currentSelection = null;

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

      List<McCoyEquipmentItem> shopItems = new List<McCoyEquipmentItem>();
      for(int i = 0; i < 4; ++i)
      {
        shopItems.Add(McCoyEquipmentGenerator.GetRandomItem());
      }

      for(int i = 0; i < 6; ++i)
      {
        var equipment = i < shopItems.Count ? shopItems[i] : null;
        var obj = Instantiate(shopItemPrefab, shopContent);
        listItems.Add(obj);
        obj.GetComponent<McCoyShopItem>().Initialize(equipment);
      }
      updateSelection();
    }

    public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if (!inputInitialized)
      {
        inputInitialized = true;
        input = new McCoyInputManager();
        input.RegisterButtonListener(ButtonPress.Button2, buyItem);
        input.RegisterButtonListener(ButtonPress.Button3, close);
        input.RegisterButtonListener(ButtonPress.Up, MoveUp);
        input.RegisterButtonListener(ButtonPress.Down, MoveDown);
        input.RegisterButtonListener(ButtonPress.Back, MoveLeft);
        input.RegisterButtonListener(ButtonPress.Forward, MoveRight);
      }
      return input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }

    private void close()
    {
      closeCallback();
    }

    private void MoveUp()
    {
      selectionIndex -= 3;
      if (selectionIndex < 0)
      {
        selectionIndex = listItems.Count + selectionIndex;
      }
      updateSelection();
    }

    private void MoveDown()
    {
      selectionIndex += 3;
      if (selectionIndex >= listItems.Count)
      {
        selectionIndex -= listItems.Count;
      }
      updateSelection();
    }

    private void MoveRight()
    {
      if (selectionIndex % 3 == 2)
      {
        selectionIndex -= 2;
      }
      else
      {
        selectionIndex++;
      }
      updateSelection();
    }

    private void MoveLeft()
    {
      if(selectionIndex % 3 == 0)
      {
        selectionIndex += 2;
      }
      else
      {
        selectionIndex--;
      }
      updateSelection();
    }

    private void updateSelection()
    {
      if (currentSelection != null)
      {
        currentSelection.Toggle(false);
      }
      currentSelection = listItems[selectionIndex].GetComponent<McCoyShopItem>();
      currentSelection.Toggle(true);

      Vector3 selectionPosition = listItems[selectionIndex].transform.position;
      var anch = shopContent.anchoredPosition;
      var newPos = (Vector2)scrollRect.transform.InverseTransformPoint(shopContent.position)
              - (Vector2)scrollRect.transform.InverseTransformPoint(selectionPosition);
      anch.y = newPos.y;
      shopContent.anchoredPosition = anch;
      updateFundsText();
    }

    private void buyItem()
    {
      McCoyGameState.Instance().BuyItem(currentSelection.Item);
      currentSelection.Initialize(null);
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
