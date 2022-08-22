using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler.Stages
{
  public class McCoyRandomSpriteParallaxItem : StageParallaxItem
  {
    Dictionary<int, int> indexes = new Dictionary<int, int>();

    [SerializeField]
    Sprite[] sprites = null;

    protected override void ItemMoved(GameObject obj, int newIndex)
    {
      debug = true;
      if(!indexes.ContainsKey(newIndex))
      {
        indexes[newIndex] = Random.Range(0, sprites.Length);
      }
      obj.GetComponent<McCoySpriteRandomizer>().Sprite.sprite = sprites[indexes[newIndex]];
    }
  }
}