using System;
using TMPro;
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
      text.StringReference.TableEntryReference = key;
      text.RefreshString();
      this.callback = callback;
    }
    public void SetTextDirectly(string s)
    {
      text.GetComponent<TMP_Text>().text = s;
    }

    public void TextLoaded(string text)
    {
      Debug.Log("text Loaded: " + text);
      if(callback != null)
      {
        callback(text);
      }
    }
  }
}