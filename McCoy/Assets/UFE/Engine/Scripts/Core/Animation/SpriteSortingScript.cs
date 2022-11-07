using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UFE3D
{
  [RequireComponent(typeof(SpriteRenderer))]
  public class SpriteSortingScript : MonoBehaviour
  {
    [SerializeField]
    List<SpriteRenderer> backgroundSprites = new List<SpriteRenderer>();

    [SerializeField]
    List<SpriteRenderer> foregroundSprites = new List<SpriteRenderer>();

    private Dictionary<string, Animator> animators = new Dictionary<string, Animator>();

    [SerializeField]
    SpriteRenderer bodySprite = null;

    public void Awake()
    {
      foreach(var bgSprite in backgroundSprites)
      {
        animators[bgSprite.gameObject.name] = bgSprite.GetComponent<Animator>();
      }
      foreach (var fgSprite in foregroundSprites)
      {
        animators[fgSprite.gameObject.name] = fgSprite.GetComponent<Animator>();
      }
    }

    public void PlayLimbAnimation(string animCmd)
    {
      string[] animatorKeys = animCmd.Split(':');
      animators[animatorKeys[0]].SetTrigger(bodySprite.flipX ? animatorKeys[1]+"_flip" : animatorKeys[1]);
    }

    public void UpdateSortOrders(int sceneIndex)
    {
      if(bodySprite == null)
      {
        bodySprite = GetComponent<SpriteRenderer>();
      }
      bodySprite.sortingOrder = sceneIndex;

      int offset = -backgroundSprites.Count;
      foreach (var bgSprite in backgroundSprites)
      {
        bgSprite.flipX = bodySprite.flipX;
        bgSprite.sortingOrder = sceneIndex + offset;
        sceneIndex++;
      }

      offset = 1;
      foreach (var fgSprite in foregroundSprites)
      {
        fgSprite.flipX = bodySprite.flipX;
        fgSprite.sortingOrder = sceneIndex + offset;
        ++sceneIndex;
      }
    }
  }
}