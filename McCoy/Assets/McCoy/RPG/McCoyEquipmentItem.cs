using System;

namespace Assets.McCoy.RPG
{
  [Serializable]
  public class McCoyEquipmentItem
  {
    public enum EquipmentType
    {
      Arms,
      Accessory,
      Consumable
    };

    public string Name;
    public EquipmentType Type;
  }
}