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
    float camMaxX;
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
      camMinX = cameraPos.x - (cameraWidth/2.0f) + unitWidth/2;
      camMaxX = cameraPos.x + (cameraWidth/2.0f);
      firstPosition = (cameraPos.x - (cameraWidth / 2)) * (1.0f - speed); // (cameraPos.x - (cameraWidth / 2)) * (1.0f - speed); // (int)(camMinX * (1.0f - speed) / chunkWidth); cameraPos.x * (1.0f - speed) + (contents.transform.localScale.x * unitWidth * i++)

      float quoteActualPositionUnquote = (cameraPos.x - (cameraWidth / 2));
      int quoteOffsetQuote = (int)((quoteActualPositionUnquote - firstPosition) / chunkWidth);

      int offset = (int)((camMinX - firstPosition) / chunkWidth);
      offset = quoteOffsetQuote;
      int newFirstIndex = ((int)(firstPosition / chunkWidth)) + offset;
      firstPosition = firstPosition + (offset * chunkWidth);
      int direction = newFirstIndex - firstIndex;
      
      if(firstIndex != newFirstIndex)
      {
        if (debug)
        {
          Debug.Log("Moving from index " + firstIndex + " to index " + newFirstIndex);
        }
        chunkIndex += direction > 0 ? 1 : -1;
        firstIndex = newFirstIndex;
      }
      
      if(direction < 0)
      {
        var last = instances[instances.Count - 1];
        instances.RemoveAt(instances.Count - 1);
        instances.Insert(0, last);
        //ItemMoved(last, firstIndex);
      }
      else if(direction > 0)
      {
        var first = instances[0];
        instances.RemoveAt(0);
        instances.Add(first);
        //ItemMoved(first, firstIndex + instances.Count - 1);
      }
      foreach (var go in instances)
      {
        go.transform.position = new Vector3(firstPosition+(chunkWidth*i), go.transform.position.y, go.transform.position.z); //cameraPos.x*(1.0f-speed) + (go.transform.localScale.x*unitWidth*i++),go.transform.position.y,go.transform.position.z);
        // if (direction != 0)
        {
          if (debug)
          {
            Debug.Log((chunkIndex +i)+ ". i getting moved at x: " + go.transform.position.x);
          }
          ItemMoved(go, chunkIndex+i);

          /*
          if (direction > 0 && i == instances.Count - 1)
          {
            ItemMoved(go, chunkIndex); // (int) ((go.transform.position.x) / chunkWidth));
          }
          else if(direction < 0 && i == 0 )
          {
            ItemMoved(go, chunkIndex);
          }
          */
        }
        ++i;
      }
      /*
      i = 0;
      foreach(var go in instances)
      {
        int previousIndex = (int) (go.transform.position.x / (float)chunkWidth);
        if (go.transform.position.x <= camMinX)
        {
          float newX = go.transform.position.x + chunkWidth * instances.Count;
          go.transform.position = new Vector3(newX, go.transform.position.y, go.transform.position.z);
          Debug.Log(i + ". moving item from " + previousIndex + " to " + (int)(go.transform.position.x / (float)chunkWidth));
          ItemMoved(go, (int) (go.transform.position.x / (float)chunkWidth));
        }
        else if(go.transform.position.x > camMaxX)
        {
          float newX = go.transform.position.x - chunkWidth * instances.Count;
          go.transform.position = new Vector3(newX, go.transform.position.y, go.transform.position.z);
          Debug.Log(i + " moving item from " + previousIndex + " to " + (int)(go.transform.position.x / (float)chunkWidth));
          ItemMoved(go, (int)(go.transform.position.x / (float)chunkWidth));
        }
        ++i;
      }
      */
    }
  }
}