using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler.Stages
{
  public class McCoySpriteRandomizer : MonoBehaviour
  {
    [SerializeField]
    SpriteRenderer sprite = null;

    public SpriteRenderer Sprite { get => sprite; }
  }
}