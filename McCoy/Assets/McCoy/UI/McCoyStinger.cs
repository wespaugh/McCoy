using System;
using UnityEngine;

namespace Assets.McCoy.UI
{
  [RequireComponent(typeof(Animator))]
  [RequireComponent(typeof(SpriteRenderer))]
  public class McCoyStinger : MonoBehaviour
  {
    private SpriteRenderer spriteRenderer;
    private Animator stingerAnimator;

    public enum StingerTypes
    {
      BossDefeated,
      StageCleared,
      Escaped,
      RoundOver,
      RoundStart,
      EnemiesRouted,
      WeekEnded,
      SelectZone,
    }

    public void RunStinger(StingerTypes type)
    {
      initSprite();

      string path = "UI/stingers/";
      switch (type)
      {
        case StingerTypes.BossDefeated:
          path += "boss_defeated";
          break;
        case StingerTypes.Escaped:
          path += "escaped";
          break;
        case StingerTypes.StageCleared:
          path += "stage_cleared";
          break;
        case StingerTypes.EnemiesRouted:
          path += "enemies_routed";
          break;
        case StingerTypes.RoundOver:
          path += "round_over";
          break;
        case StingerTypes.RoundStart:
          path += "round_started";
          break;
        case StingerTypes.WeekEnded:
          path += "week_ended";
          break;
        case StingerTypes.SelectZone:
          path += "select_zone";
          break;
      }

      var sprite = Resources.Load<Sprite>(path);
      spriteRenderer.sprite = sprite;
      stingerAnimator.SetTrigger("Sting");
    }

    private void initSprite()
    {
      if(spriteRenderer != null)
      {
        return;
      }

      spriteRenderer = GetComponent<SpriteRenderer>();
      stingerAnimator = GetComponent<Animator>();
    }
  }
}