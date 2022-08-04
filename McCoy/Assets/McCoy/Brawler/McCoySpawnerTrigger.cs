using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  public class McCoySpawnerTrigger : MonoBehaviour
  {
    [SerializeField]
    public string EnemyName;

    public bool Fired { get; set; }

    public void OnDestroy()
    {
      Debug.Log("GETTING DESTROYED HERE SOMEHOW");
    }
  }
}