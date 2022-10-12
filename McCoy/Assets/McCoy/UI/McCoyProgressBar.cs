﻿using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyProgressBar : MonoBehaviour
  {
    [SerializeField]
    SpriteRenderer sprite = null;

    [SerializeField]
    SpriteRenderer spriteBorder = null;

    [SerializeField]
    SpriteRenderer sprite2 = null;

    [SerializeField]
    float overrideHP = -2.0f;

    [SerializeField]
    float animateSpeed = 0.6f;

    [SerializeField]
    Texture2D MainTextureOverride = null;

    //debug fields
    [SerializeField]
    Color Sprite1Color;
    [SerializeField]
    Color Sprite2Color;

    private float amountPerBar;
    private float amountCache = -1;
    private int barCache = -1;

    float currentPercent = 0.0f;
    float targetPercent = 0.0f;

    public void Initialize(int totalAmount, int amountPerBar)
    {
      this.amountPerBar = amountPerBar;
      currentPercent = (totalAmount % amountPerBar) / amountPerBar;
      amountCache = totalAmount;

      if(MainTextureOverride != null)
      {
        sprite.material.SetTexture("_MainTexture", MainTextureOverride);
        sprite.material.SetTexture("_MainTex", MainTextureOverride);
        sprite2.material.SetTexture("_MainTexture", MainTextureOverride);
        sprite2.material.SetTexture("_MainTex", MainTextureOverride);
        sprite2.gameObject.SetActive(false);
      }
    }

    public void SetFill(float barAmount)
    {
      float totalHP = overrideHP > 0 ? overrideHP : barAmount;
      float direction = totalHP == amountCache ? 0 : totalHP < amountCache ? -1.0f : 1.0f;

      float percent = (totalHP % amountPerBar) / amountPerBar;

      int targetBar = (int)(totalHP / amountPerBar);
      if (targetBar > 0 && totalHP % amountPerBar == 0 && ((float)targetBar) * amountPerBar == totalHP)
      {
        --targetBar;
      }
      if (percent == 0 && totalHP != 0)
      {
        percent = 1;
      }

      // be sure to use cache here. it's only updated when animation flips to the right bar
      int currentBar = (int)(amountCache / amountPerBar);
      if(amountCache % amountPerBar == 0 && ((float)currentBar)*amountPerBar == amountCache && currentBar > 0)
      {
        --currentBar;
      }
      bool barFlip = targetBar != currentBar;

      targetPercent = barFlip && direction != 0.0f ? (direction > 0 ? 1.0f : 0.0f) : percent;

      // float direction = currentPercent > targetPercent ? -1.0f : 1.0f;
      float distance = direction * animateSpeed * Time.deltaTime; // speed
      currentPercent += distance;
      if( (direction == 1 && currentPercent >= targetPercent)
        || (direction == -1 && currentPercent <= targetPercent))
      {
        currentPercent = targetPercent;
      }

      // if we don't need to worry about barflip, just set the hp cach
      // if we are worrying about barflip, only cache the XP once we've filled/emptied the bar
      if (!barFlip)
      {
        amountCache = currentBar * amountPerBar + currentPercent * amountPerBar;
      }
      else if(direction > 0 && currentPercent >= .99)
      {
        currentBar = targetBar;
        currentPercent = 0.0f;
        amountCache = amountPerBar*currentBar + .01f;
      }
      else if(direction < 0 && currentPercent <= .01)
      {
        currentBar = targetBar;
        currentPercent = 1.0f;
        amountCache = amountPerBar * currentBar + amountPerBar;
      }

      sprite.material.SetFloat("_progress", -1.0f + currentPercent);

      float p2 = (totalHP > amountPerBar || barFlip ? 1.0f : 0);
      sprite2.material.SetFloat("_progress", -1.0f + p2);
      
      if(currentBar == barCache)
      {
        return;
      }
      barCache = currentBar;
      switch (barCache)
      {
        case 0:
          SetColor(Color.white, Color.clear, Color.white);
          break;
        case 1:
          SetColor(Color.cyan, Color.white, Color.white);
          break;
        case 2:
          SetColor(Color.green, Color.cyan, Color.white);
          break;
        case 3:
          SetColor(Color.blue, Color.green, Color.white);
          break;
        case 4:
          SetColor(Color.black, Color.blue, Color.white);
          break;
        default:
          SetColor(Color.black, Color.black, Color.white);
          break;
      }
    }

    public void FadeIn(float time = 1.0f)
    {
      Fade(0.0f, 1.0f, time);
    }

    public void FadeOut(float time = 1.0f)
    {
      Fade(1.0f, 0.0f, time);
    }

    public void Fade(float start, float end, float time)
    {
      StartCoroutine(fade(start, end, time));
    }

    private IEnumerator fade(float start, float end, float time)
    {
      for (float i = 0; i < time; i += Time.deltaTime)
      {
        float alpha = Mathf.Lerp(start, end, i/time);
        SetAlpha(alpha, true);
        yield return null;
      }
      SetAlpha(end, true);
      yield break;
    }

    private IEnumerator fade(Color start, Color end, float time)
    {
      for (float i = 0; i < time; i += Time.deltaTime)
      {
        Color newColor = new Color(Mathf.Lerp(start.r, end.r, i / time), Mathf.Lerp(start.g, end.g, i / time), Mathf.Lerp(start.b, end.b, i / time), Mathf.Lerp(start.a, end.a, i / time));
        SetColor(newColor, newColor, newColor);
        yield return null;
      }
      SetColor(end, end, end);
      yield break;
    }

    private void SetColor(Color sprite1Color, Color sprite2Color, Color borderColor)
    {
      Sprite1Color = sprite1Color;
      Sprite2Color = sprite2Color;
      sprite.material.SetColor("_Color0", sprite1Color);
      if (borderColor != null && spriteBorder != null)
      {
        spriteBorder.color = borderColor;
      }
      if (sprite2 != null)
      {
        sprite2.material.SetColor("_Color0", sprite2Color);
      }
    }
    private void SetAlpha(float alpha, bool includeBorder = false)
    {
      Color n = sprite.material.GetColor("_Color0");
      n.a = alpha;
      sprite.material.SetColor("_Color0", n);
      if (includeBorder && spriteBorder != null)
      {
        n = spriteBorder.material.color;
        n.a = alpha;
        spriteBorder.color = n;
      }
      if (sprite2 != null)
      {
        n = sprite2.material.GetColor("_Color0");
        n.a = alpha;
        sprite2.material.SetColor("_Color0", n);
      }
    }
  }
}