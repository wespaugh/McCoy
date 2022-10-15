using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  [RequireComponent(typeof(SpriteRenderer))]
  public class McCoyBrawlerCharacterAnimator : MonoBehaviour
  {
    [SerializeField]
    Animator gunAnimator = null;

    [SerializeField]
    Animator gunAnimatorMirrored = null;

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

    public void Shoot()
    {
      gunAnimator.GetComponent<SpriteRenderer>().flipX = characterSprite.flipX;
      if(characterSprite.flipX)
      {
        gunAnimator.SetTrigger("Shoot");
      }
      else
      {
        gunAnimator.SetTrigger("Shoot");
      }
    }
  }
}