using System;
using UnityEngine;

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
    private float r, g, b, a;
    [NonSerialized]
    private Color _tint;
    public Color Tint
    {
      get
      {
        return _tint;
      }
      set
      {
        r = value.r;
        g = value.g;
        b = value.b;
        a = value.a;
        _tint = new Color(r, g, b, a);
      }
    }
  }
}