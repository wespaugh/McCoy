using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace Assets.McCoy.Localization
{
  public class McCoyLocalizedText : MonoBehaviour
  {
    [SerializeField]
    LocalizeStringEvent optionalText = null;

    private LocalizeStringEvent _text;

    private Action<string> callback;

    private LocalizeStringEvent text
    {
      get
      {
        if (_text == null)
        {
          if (optionalText != null)
          {
            _text = optionalText;
          }
          else
          {
            _text = GetComponent<LocalizeStringEvent>();
          }
        }
        return _text;
      }
    }

    public void SetText(string key, Action<string> callback = null)
    {
      this.callback = callback;
      text.StringReference.TableEntryReference = key;
      text.RefreshString();
    }
    public void SetTextDirectly(string s)
    {
      text.GetComponent<TMP_Text>().text = s;
    }

    public void TextLoaded(string text)
    {
      if(callback != null)
      {
        callback(text);
      }
    }
  }
}