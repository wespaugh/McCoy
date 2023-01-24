using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.RPG
{
  [Serializable]
  public class McCoyEquipmentLoadout
  {
    List<McCoyEquipmentItem> equipment = new List<McCoyEquipmentItem>();
    int equippedArmsIndex = -1;
    int equippedAccessoryIndex = -1;
    public List<McCoyEquipmentItem> Equipment
    {
      get => equipment;
    }
    public int EquippedArmsIndex => equippedArmsIndex;
    public int EquippedAccessoryIndex => equippedAccessoryIndex;

    public McCoyEquipmentLoadout()
    {
      Equipment.Add(McCoyEquipmentGenerator.GetRandomItem());
    }

    public void Equip(int selection)
    {
      if (equipment[selection].Type == McCoyEquipmentItem.EquipmentType.Arms)
      {
        equippedArmsIndex = selection;
      }
      else
      {
        equippedAccessoryIndex = selection;
      }
    }
    public void Unequip(bool arms)
    {
      if(arms)
      {
        equippedArmsIndex = -1;
      }
      else
      {
        equippedAccessoryIndex = -1;
      }
    }

    public void AddEquipment(McCoyEquipmentItem equipmentItem)
    {
      Equipment.Add(equipmentItem);
    }
  }
}