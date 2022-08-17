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

    [SerializeField]
    float camMinX;
    [SerializeField]
    float camMaxX;

    List<GameObject> instances = new List<GameObject>();

    private void Awake()
    {

      float cameraWidth = Camera.main.aspect * Camera.main.orthographicSize * 2;
      int numInstancesNeeded = ((int)(cameraWidth / unitWidth)) + 3;

      for (int i = 0; i < numInstancesNeeded; ++i)
      {
        var next = Instantiate(contents, transform);
        instances.Add(next);
        float index = Camera.main.transform.localPosition.x * (1.0f - speed) + (next.transform.localScale.x * unitWidth * i);
        float chunkWidth = next.transform.localScale.x * unitWidth;
        int iIndex = (int)(index / chunkWidth);
        // Debug.Log("in initialization, item at " + index + " has a chunk index of " + iIndex);
        ItemMoved(next, iIndex);
      }
    }
    protected virtual void ItemMoved(GameObject item, int index)
    {

    }

    private void FixedUpdate()
    {
      cameraPos = Camera.main.transform.localPosition;
      int i = 0;
      foreach (var go in instances)
      {
        go.transform.position = new Vector3(cameraPos.x*(1.0f-speed) + (go.transform.localScale.x*unitWidth*i++),go.transform.position.y,go.transform.position.z);
      }
      foreach(var go in instances)
      {
        float chunkWidth = go.transform.localScale.x * unitWidth;
        camMinX = cameraPos.x - (2.5f * chunkWidth);
        camMaxX = cameraPos.x + (2.5f * chunkWidth);
        if (go.transform.position.x <= camMinX)
        {
          float newX = go.transform.position.x + chunkWidth * instances.Count;
          go.transform.position = new Vector3(newX, go.transform.position.y, go.transform.position.z);
          ItemMoved(go, (int) (go.transform.position.x / (float)chunkWidth));
        }
        else if(go.transform.position.x > camMaxX)
        {
          float newX = go.transform.position.x - chunkWidth * instances.Count;
          go.transform.position = new Vector3(newX, go.transform.position.y, go.transform.position.z);
          ItemMoved(go, (int)(go.transform.position.x / (float)chunkWidth));
        }
      }
    }
  }
}