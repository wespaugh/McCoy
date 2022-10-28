using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler.FX
{
  public class MulticolorSpriteCopyTrail : MonoBehaviour
  {
    [SerializeField]
    SpriteRenderer spriteSource = null;
    [SerializeField]
    GameObject spritePrefab = null;
    public void CreateTrailSprite()
    {
      var s = UFE.SpawnGameObject(spritePrefab, spriteSource.transform.position, Quaternion.identity, 20, id : "trail");
      SpriteRenderer effectSprite = s.GetComponent<SpriteRenderer>();
      effectSprite.sprite = spriteSource.sprite;
      effectSprite.flipX = spriteSource.flipX;
      effectSprite.color = new Color(1, 1, 1, .6f);
    }
  }
}