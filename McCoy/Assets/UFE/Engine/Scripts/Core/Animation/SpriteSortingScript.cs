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

    bool cachedFlip = false;
    string cachedCommand = "";

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
      cachedFlip = bodySprite.flipX;
      cachedCommand = animCmd;
      Debug.Log("play limb animation: " + animCmd);
      string[] commands = animCmd.Split(',');
      foreach (var cmd in commands)
      {
        string[] animatorKeys = cmd.Split(':');
        bool hide = animatorKeys.Length == 0 || animatorKeys[1] == "";
        animators[animatorKeys[0]].gameObject.SetActive(!hide);
        // animators[animatorKeys[0]].Play(bodySprite.flipX ? animatorKeys[1] + "_flip" : animatorKeys[1]);
        animators[animatorKeys[0]].SetTrigger(bodySprite.flipX ? animatorKeys[1] + "_flip" : animatorKeys[1]);
      }
    }

    public void UpdateSortOrders(int sceneIndex)
    {
      if(bodySprite == null)
      {
        bodySprite = GetComponent<SpriteRenderer>();
      }
      if(bodySprite.flipX != cachedFlip && !string.IsNullOrEmpty(cachedCommand))
      {
        PlayLimbAnimation(cachedCommand);
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