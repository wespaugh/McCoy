using Assets.McCoy.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.RPG
{
  public  class McCoyShopItem : MonoBehaviour

  {
    [SerializeField]
    McCoyLocalizedText itemName = null;

    [SerializeField]
    McCoyLocalizedText itemCost = null;

    [SerializeField]
    Image img = null;

    [SerializeField]
    GameObject highlight = null;

    public McCoyEquipmentItem Item
    {
      get; private set;
    }

    public void Initialize(McCoyEquipmentItem item)
    {
      this.Item = item;
      if(item == null)
      {
        itemName.SetTextDirectly("");
        itemCost.SetTextDirectly("");
        img.gameObject.SetActive(false);
        return;
      }
      img.gameObject.SetActive(true);
      itemName.SetTextDirectly(item.Name);
      itemCost.SetTextDirectly("200");
    }

    public void Toggle(bool selected)
    {
      highlight.SetActive(selected);
    }
  }
}
