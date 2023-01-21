using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.RPG
{
  public class McCoyEquipmentGenerator
  {
    public static McCoyEquipmentItem GetRandomItem()
    {
      // Guaranteed to be random
      return new McCoyEquipmentItem()
      {
        Name = "Eschelon 621-X",
        Type = McCoyEquipmentItem.EquipmentType.Arms,
        Tint = ProjectConstants.BLUE_STEEL
      };
    }
  }
}