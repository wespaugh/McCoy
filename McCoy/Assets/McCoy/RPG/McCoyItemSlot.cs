using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.RPG
{
  public class McCoyItemSlot : MonoBehaviour
  {
    [SerializeField]
    Image itemImage = null;

    [SerializeField]
    Image highlightImage = null;

    McCoyEquipmentItem itemInSlot = null;
    public McCoyEquipmentItem Item
    {
      get => itemInSlot;
    }
    public void SetItem(McCoyEquipmentItem item)
    {
      itemImage.gameObject.SetActive(item != null);
      itemInSlot = item;
    }
    public void SetHighlight(bool on)
    {
      highlightImage.gameObject.SetActive(on);
    }
  }
}