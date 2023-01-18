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
    public void Initialize(McCoyEquipmentItem item)
    {
      if(item == null)
      {
        itemImage.gameObject.SetActive(false);
      }
    }
    public void SetHighlight(bool on)
    {
      highlightImage.gameObject.SetActive(on);
    }
  }
}