using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  public class McCoyBrawlerCharacterAnimator : MonoBehaviour
  {
    [SerializeField]
    Animator gunAnimator = null;
    public void Shoot()
    {
      gunAnimator.SetTrigger("Shoot");
    }
  }
}