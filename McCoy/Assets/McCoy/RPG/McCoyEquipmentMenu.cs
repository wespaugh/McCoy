using Assets.McCoy.BoardGame;
using Assets.McCoy.Localization;
using Assets.McCoy.UI;
using System;
using System.Collections.Generic;
using UFE3D;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.RPG
{
  public class McCoyEquipmentMenu : MonoBehaviour, IMcCoyInputManager
  {
    [SerializeField] 
    McCoyLocalizedText equipmentLabel = null;

    [SerializeField]
    McCoyLocalizedText selectedItemText = null;

    [SerializeField]
    Transform InventoryRoot = null;

    [SerializeField]
    GameObject ItemSlotPrefab = null;

    [SerializeField]
    McCoyItemSlot armsSlot = null;

    [SerializeField]
    McCoyItemSlot accessorySlot = null;

    Animator characterAnimator = null;
    private bool inputInitialized;
    private McCoyInputManager input;

    List<GameObject> inventorySlots = new List<GameObject>();

    McCoyEquipmentLoadout playerEquipment = null;

    // -1 : Equipped Arms
    // -2 : Equipped Accessory
    // 0-11 : An Item
    int selection = 0;
    public void Initialize(Animator characterAnimator, string animation)
    {
      this.characterAnimator = characterAnimator;
      characterAnimator.GetComponent<SpriteRenderer>().flipX = true;
      characterAnimator.Play(animation);
      equipmentLabel.SetText("com.mccoy.rpg.equipmentowner", arguments: new string[] { "Rex" });

      playerEquipment = McCoyGameState.Instance().PlayerEquipment(ProjectConstants.PlayerCharacter.Rex);

      bool create = inventorySlots.Count == 0;
      for(int i = 0; i < 12; ++i)
      {
        GameObject go;
        if(create)
        {
          go = Instantiate(ItemSlotPrefab, InventoryRoot);
          inventorySlots.Add(go);
        }
        else
        {
          go = inventorySlots[i];
        }
        McCoyEquipmentItem item = null;
        if(playerEquipment.Equipment.Count > i)
        {
          item = playerEquipment.Equipment[i];
        }
        go.GetComponent<McCoyItemSlot>().SetItem(item);
      }
      refreshEquipmentSlots();
      updateSelection(selection, true);
    }

    protected void refreshEquipmentSlots()
    {
      accessorySlot.SetItem(playerEquipment.EquippedAccessoryIndex >= 0 ? playerEquipment.Equipment[playerEquipment.EquippedAccessoryIndex] : null);
      armsSlot.SetItem(playerEquipment.EquippedArmsIndex >= 0 ? playerEquipment.Equipment[playerEquipment.EquippedArmsIndex] : null);
      characterAnimator.GetComponent<SpriteSortingScript>().Mod = playerEquipment.EquippedArmsIndex >= 0 ? new SpriteSortingScript.SpriteModifyData("_colossus", ProjectConstants.BLUE_STEEL) : null;
    }

    private void updateSelection(int newIdx, bool force = false)
    {
      if(newIdx == selection && !force)
      {
        return;
      }
      if(newIdx >= inventorySlots.Count)
      {
        Debug.Log("uh oh. " + newIdx);
        return;
      }
      getSlot(selection).SetHighlight(false);
      selection = newIdx;
      getSlot(selection).SetHighlight(true);
      var itemSelected = getSlot(selection).Item;

      selectedItemText.SetTextDirectly(itemSelected?.Name);

    }

    private McCoyItemSlot getSlot(int idx)
    {
      return idx == -1 ? armsSlot : (idx == -2 ? accessorySlot : inventorySlots[idx].GetComponent<McCoyItemSlot>());
    }

    public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if (!inputInitialized)
      {
        inputInitialized = true;
        input = new McCoyInputManager();
        input.RegisterButtonListener(ButtonPress.Forward, navRight);
        input.RegisterButtonListener(ButtonPress.Back, navLeft);
        input.RegisterButtonListener(ButtonPress.Up, navUp);
        input.RegisterButtonListener(ButtonPress.Down, navDown);
        input.RegisterButtonListener(ButtonPress.Button1, selectSlot);
      }
      bool retVal = input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
      return retVal;
    }

    protected void selectSlot()
    {
      var slot = getSlot(selection);
      // arms unequip
      if(selection == -1)
      {
        playerEquipment.Unequip(true);
      }
      // accessory unequip
      else if(selection == -2)
      {
        playerEquipment.Unequip(false);
      }
      else if(slot.Item != null)
      {
        playerEquipment.Equip(selection);
      }
      refreshEquipmentSlots();
    }

    #region Navigation
    private void navRight()
    {
      int newIdx = selection;
      if (selection == -1) newIdx = -2;
      else if (selection == -2) newIdx = -1;
      else
      {
        if(newIdx % 6 == 5)
        {
          newIdx -= 5;
        }
        else
        {
          newIdx++;
        }
      }
      updateSelection(newIdx);
    }

    private void navLeft()
    {
      int newIdx = selection;
      if (selection == -1) newIdx = -2;
      else if (selection == -2) newIdx = -1;
      else
      {
        if (newIdx % 6 == 0)
        {
          newIdx += 5;
        }
        else
        {
          newIdx--;
        }
      }
      updateSelection(newIdx);
    }

    private void navUp()
    {
      int newIdx = selection;
      if (selection == -1) newIdx = 7;
      else if (selection == -2) newIdx = 10;
      else
      {
        if (selection < 3)
        {
          newIdx = -1;
        }
        else if (selection < 6)
        {
          newIdx = -2;
        }
        else
        {
          newIdx = selection - 6;
        }
      }
      updateSelection(newIdx);
    }

    private void navDown()
    {
      int newIdx = selection;
      if (selection == -1) newIdx = 1;
      else if (selection == -2) newIdx = 4;
      else
      {
        if (selection > 8)
        {
          newIdx = -2;
        }
        else if (selection > 5)
        {
          newIdx = -1;
        }
        else
        {
          newIdx = selection + 6;
        }
      }
      updateSelection(newIdx);
    }
    #endregion
  }
}