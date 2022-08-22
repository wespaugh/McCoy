using FPLibrary;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler.Stages
{
  public class StageParallaxItem : MonoBehaviour
  {
    [SerializeField]
    public GameObject contents = null;

    [SerializeField]
    private float unitWidth;

    [SerializeField]
    // zero remains entirely static and unmoving
    // one moves perfectly with the camera
    float speed = 1.0f;

    [SerializeField]
    Vector3 cameraPos;

    float cameraWidth;

    // the index l-r to *start* positioning chunks at
    int firstIndex = 0;

    List<GameObject> instances = new List<GameObject>();

    protected bool debug = false;

    [SerializeField]
    int chunkIndex = 0;

    // debug numbers
    [SerializeField]
    float camMinX;
    [SerializeField]
    float firstPosition = 0.0f;

    private void Awake()
    {

      cameraWidth = Camera.main.aspect * Camera.main.orthographicSize * 2;
      int numInstancesNeeded = ((int)(cameraWidth / unitWidth)) + 3;

      for (int i = 0; i < numInstancesNeeded; ++i)
      {
        var next = Instantiate(contents, transform);
        instances.Add(next);
        float index = Camera.main.transform.localPosition.x * (1.0f - speed) + (next.transform.localScale.x * unitWidth * i);
        float chunkWidth = next.transform.localScale.x * unitWidth;
        // Debug.Log("in initialization, item at " + index + " has a chunk index of " + iIndex);
        ItemMoved(next, chunkIndex+i);
      }
    }
    protected virtual void ItemMoved(GameObject item, int index)
    {

    }

    private void FixedUpdate()
    {
      cameraPos = Camera.main.transform.localPosition;
      int i = 0;
      float chunkWidth = contents.transform.localScale.x * unitWidth;
      camMinX = cameraPos.x;

      firstPosition = camMinX * (1.0f - speed);
      chunkIndex = 0;

      while (firstPosition + unitWidth <= camMinX)
      {
        ++chunkIndex;
        firstPosition += unitWidth;
      }

      foreach (var go in instances)
      {
        go.transform.position = new Vector3(firstPosition+(chunkWidth*i) - (cameraWidth / 2.0f) + unitWidth / 2, go.transform.position.y, go.transform.position.z);
        ItemMoved(go, chunkIndex+i);
        ++i;
      }
    }
  }
}