using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyProgressBar : MonoBehaviour
  {
    [SerializeField]
    SpriteRenderer sprite = null;

    public void SetFill(float percent)
    {
      sprite.material.SetFloat("_progress", -1.0f + percent);
    }
  }
}