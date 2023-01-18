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
        Name = "Colossus_Arms",
        Type = McCoyEquipmentItem.EquipmentType.Arms
      };
    }
  }
}