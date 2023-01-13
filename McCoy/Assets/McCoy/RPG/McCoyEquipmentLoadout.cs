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
  }
}