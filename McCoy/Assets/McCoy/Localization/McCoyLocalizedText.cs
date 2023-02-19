using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace Assets.McCoy.Localization
{
  public class McCoyLocalizedText : MonoBehaviour
  {
    [SerializeField]
    LocalizeStringEvent optionalReference = null;

    private LocalizeStringEvent _text;

    private Action<string> callback;

    private LocalizeStringEvent text
    {
      get
      {
        if (_text == null)
        {
          if (optionalReference != null)
          {
            _text = optionalReference;
          }
          else
          {
            _text = GetComponent<LocalizeStringEvent>();
          }
        }
        return _text;
      }
    }

    public void SetText(string key, Action<string> callback = null, object[] arguments = null)
    {
      this.callback = callback;
      text.StringReference.TableEntryReference = key;
      text.StringReference.Arguments = arguments;
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