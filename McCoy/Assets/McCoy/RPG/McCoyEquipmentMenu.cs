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

    // -1 : Arms
    // -2 : Accessory
    // 0-11 : An Item
    int selection = 0;
    public void Initialize(Animator characterAnimator)
    {
      this.characterAnimator = characterAnimator;
      characterAnimator.Play("rex_idle");
      characterAnimator.GetComponent<SpriteSortingScript>().Mod = new SpriteSortingScript.SpriteModifyData("_colossus", ProjectConstants.PURPLE);
      equipmentLabel.SetText("com.mccoy.rpg.equipmentowner", arguments: new string[] { "Rex" });

      var rexEquipment = McCoyGameState.Instance().PlayerEquipment(ProjectConstants.PlayerCharacter.Rex);

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
        if(rexEquipment.Equipment.Count > i)
        {
          item = rexEquipment.Equipment[i];
        }
        go.GetComponent<McCoyItemSlot>().Initialize(item);
      }

      accessorySlot.Initialize(rexEquipment.EquippedAccessoryIndex >= 0 ? rexEquipment.Equipment[rexEquipment.EquippedAccessoryIndex] : null);
      armsSlot.Initialize(rexEquipment.EquippedArmsIndex >= 0 ? rexEquipment.Equipment[rexEquipment.EquippedArmsIndex] : null);
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
      }
      bool retVal = input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
      return retVal;
    }

    private void navRight()
    {
      Debug.Log("Nav Right");
    }

    private void navLeft()
    {
      Debug.Log("Nav Left");
    }

    private void navUp()
    {
      Debug.Log("Nav Up");
    }

    private void navDown()
    {
      Debug.Log("Nav Down");
    }
  }
}