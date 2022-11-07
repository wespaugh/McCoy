using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  [RequireComponent(typeof(SpriteRenderer))]
  public class McCoyBrawlerCharacterAnimator : MonoBehaviour
  {
    private Dictionary<string, Animator> animators = new Dictionary<string, Animator>();
    
    [SerializeField]
    List<Animator> foregroundAnimators;
    [SerializeField]
    List<Animator> backgroundAnimators;

    private SpriteRenderer _characterSprite = null;
    private SpriteRenderer characterSprite
    {
      get
      {
        if(_characterSprite == null)
        {
          _characterSprite = GetComponent<SpriteRenderer>();
        }
        return _characterSprite;
      }
    }
  }
}