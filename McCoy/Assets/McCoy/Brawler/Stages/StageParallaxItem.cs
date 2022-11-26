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
    private float unitSize;

    [SerializeField]
    // zero remains entirely static and unmoving
    // one moves perfectly with the camera
    float speed = 1.0f;

    [SerializeField]
    public float autoScrollSpeed = 0.0f;

    [SerializeField]
    Vector3 cameraPos;

    float cameraSize;

    // the index l-r to *start* positioning chunks at
    int firstIndex = 0;

    List<GameObject> instances = new List<GameObject>();

    protected bool debug = false;

    [SerializeField]
    int chunkIndex = 0;

    [SerializeField]
    bool horizontal = true;

    // debug numbers
    [SerializeField]
    float camMinPos;
    [SerializeField]
    float firstPosition = 0.0f;
    [SerializeField]
    float autoScrollOffset = 0.0f;

    private float scrollStartTime;

    private void Awake()
    {
      scrollStartTime = Time.time;

      cameraSize = Camera.main.aspect * Camera.main.orthographicSize * 2;
      if (!horizontal)
      {
        cameraSize = cameraSize / Camera.main.aspect; // cameraSize/aspect = height
      }
      if(unitSize == 0)
      {
        Debug.LogError("Parallax Item Cannot Be 0 units wide");
        return;
      }
      int numInstancesNeeded = ((int)(cameraSize / unitSize)) + 3;

      for (int i = 0; i < numInstancesNeeded; ++i)
      {
        var next = Instantiate(contents, transform);
        instances.Add(next);
        /*
        float index = Camera.main.transform.localPosition.x * (1.0f - speed) + (next.transform.localScale.x * unitSize * i);
        float chunkWidth = next.transform.localScale.x * unitSize;
        // Debug.Log("in initialization, item at " + index + " has a chunk index of " + iIndex);
        ItemMoved(next, chunkIndex+i);
        */
      }
    }
    protected virtual void ItemMoved(GameObject item, int index)
    {

    }

    private void FixedUpdate()
    {
      cameraPos = Camera.main.transform.localPosition;
      int i = 0;
      float chunkSize = horizontal ? contents.transform.localScale.x : contents.transform.localScale.y;
      chunkSize *= unitSize;
      autoScrollOffset = ((Time.time - scrollStartTime) * autoScrollSpeed) * speed;
      camMinPos = horizontal ? cameraPos.x : cameraPos.y; 
      camMinPos -= autoScrollOffset;
      if (!horizontal)//autoScrollSpeed != 0f)
      {
        firstPosition = -autoScrollSpeed * (Time.time - scrollStartTime);
      }
      else
      {
        firstPosition = camMinPos * (1.0f - speed) - (autoScrollSpeed * (Time.time - scrollStartTime));
      }
      chunkIndex = 0;

      while (firstPosition + unitSize <= camMinPos + autoScrollOffset)
      {
        ++chunkIndex;
        firstPosition += unitSize;
      }

      foreach (var go in instances)
      {
        if (horizontal)
        {
          go.transform.position = new Vector3(firstPosition + (chunkSize * i) - (cameraSize / 2.0f) + unitSize / 2, go.transform.position.y, go.transform.position.z);
        }
        else
        {
          go.transform.position = new Vector3(go.transform.position.x, firstPosition + (chunkSize * i) - (cameraSize / 2.0f) + unitSize / 2, go.transform.position.z);
        }
        ItemMoved(go, chunkIndex+i);
        ++i;
      }
    }
  }
}