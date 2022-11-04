using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace Assets.McCoy.Localization
{
  public class McCoyLocalizedText : MonoBehaviour
  {
    [SerializeField]
    LocalizeStringEvent optionalText = null;

    private LocalizeStringEvent text;

    public void SetText(string key)
    {
      if(text == null)
      {
        if(optionalText != null)
        {
          text = optionalText;
        }
        else
        {
          text = GetComponent<LocalizeStringEvent>();
        }
      }
      text.StringReference.TableEntryReference = key;
      text.RefreshString();
    }
  }
}