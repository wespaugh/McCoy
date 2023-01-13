using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.RPG
{
  public class McCoyItemSlot : MonoBehaviour
  {
    [SerializeField]
    Image itemImage = null;
    public void Initialize(McCoyEquipmentItem item)
    {
      if(item == null)
      {
        itemImage.gameObject.SetActive(false);
      }
    }
  }
}