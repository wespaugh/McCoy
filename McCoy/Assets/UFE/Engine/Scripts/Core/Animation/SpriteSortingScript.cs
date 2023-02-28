using System.Collections.Generic;
using UnityEngine;

namespace UFE3D
{
  [RequireComponent(typeof(SpriteRenderer))]
  public class SpriteSortingScript : MonoBehaviour
  {
    public class SpriteModifyData
    {
      public SpriteModifyData(string animSuffix, Color tint)
      {
        animationSuffix = animSuffix;
        this.tint = tint;
      }
      string animationSuffix;
      public string AnimationSuffix
      {
        get => animationSuffix;
      }
      Color tint;
      public Color Tint
      {
        get => tint;
      }
    }

    private class SpriteSortData
    {
      public SpriteRenderer sprite;
      public bool modify;
      public Animator animator;
    }

    [SerializeField]
    List<SpriteRenderer> backgroundSprites = new List<SpriteRenderer>();

    [SerializeField]
    List<SpriteRenderer> foregroundSprites = new List<SpriteRenderer>();

    [SerializeField]
    List<SpriteRenderer> spritesToModify = new List<SpriteRenderer>();

    private Dictionary<string, SpriteSortData> spriteData = new Dictionary<string, SpriteSortData>();
    public List<SpriteRenderer> SpritesToModify 
    { 
      get => spritesToModify; 
    }
    [SerializeField]
    SpriteRenderer bodySprite = null;

    [SerializeField] string testAnim = null;
    string cachedTest = "";

    SpriteModifyData mod = null;
    public SpriteModifyData Mod
    {
      get => mod;
      set
      {
        mod = value;
        PlayLimbAnimation(cachedCommand);
      }
    }

    bool cachedFlip = false;
    string cachedCommand = "";

    public void Awake()
    {
      foreach(var bgSprite in backgroundSprites)
      {
        spriteData[bgSprite.gameObject.name] = new SpriteSortData(){
          sprite = bgSprite,
          modify = spritesToModify.Contains(bgSprite),
          animator = bgSprite.GetComponent<Animator>()
        };
      }
      foreach (var fgSprite in foregroundSprites)
      {
        spriteData[fgSprite.gameObject.name] = new SpriteSortData()
        {
          sprite = fgSprite,
          modify = spritesToModify.Contains(fgSprite),
          animator = fgSprite.GetComponent<Animator>()
        };
      }
    }

    public void PlayLimbAnimation(string animCmd)
    {
      cachedFlip = bodySprite.flipX;
      cachedCommand = animCmd;
      string[] commands = animCmd.Split(',');
      foreach (var cmd in commands)
      {
        string[] animatorKeys = cmd.Split(':');
        string spriteKey = animatorKeys[0];
        string animKey = animatorKeys.Length > 1 ? animatorKeys[1] : "";
        bool hide = animatorKeys.Length == 0 || animKey == "";
        if(!spriteData.ContainsKey(spriteKey))
        {
          Debug.LogWarning("No Sprite named " + spriteKey + " on sprite " + gameObject.name);
          continue;
        }
        spriteData[spriteKey].animator.gameObject.SetActive(!hide);
        if (mod != null && spriteData[spriteKey].modify)
        {
          spriteData[spriteKey].sprite.color = Mod.Tint;
          animKey += Mod.AnimationSuffix;
        }
        else
        {
          spriteData[spriteKey].sprite.color = Color.white;
        }
        animKey += (bodySprite.flipX ? "_flip" : "");
        if (!hide)
        {
          spriteData[spriteKey].animator.Play(animKey);
        }
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