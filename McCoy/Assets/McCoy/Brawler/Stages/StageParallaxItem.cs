using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler.Stages
{
  public class StageParallaxItem : MonoBehaviour
  {
    [SerializeField]
    GameObject contents = null;

    List<GameObject> instances = new List<GameObject>();
    private void Awake()
    {
      instances.Add(Instantiate(contents, contents.transform.parent));
      contents.SetActive(false);
    }

    private void FixedUpdate()
    {
      foreach(var go in instances)
      {
        var pos = Camera.main.transform.localPosition;
        go.transform.position = new Vector3(pos.x, 8,18);
      }
    }
  }
}