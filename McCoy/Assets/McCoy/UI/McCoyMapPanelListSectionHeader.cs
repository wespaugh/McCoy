using TMPro;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyMapPanelListSectionHeader : MonoBehaviour
  {
    [SerializeField]
    TMP_Text text = null;

    public void Initialize(bool connectedHeader)
    {
      if(connectedHeader)
      {
        text.text = "Connected Zones";
      }
      else
      {
        text.text = "Other Zones";
      }
    }
  }
}
