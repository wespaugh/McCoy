using UFE3D;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyCityScreen : UFEScreen
  {
    [SerializeField]
    GameObject boardContents = null;

    GameObject board = null;
    private void Awake()
    {
      if (board == null)
      {
        board = Instantiate(boardContents);
        GameObject.Find("CityCamera").GetComponent<Camera>().enabled = true;
        board.GetComponent<Canvas>().worldCamera = GameObject.Find("CityCamera").GetComponent<Camera>();
      }
    }

    private void OnDestroy()
    {
      if (board != null)
      {
        GameObject.Find("CityCamera").GetComponent<Camera>().enabled = false;
        Destroy(board);
      }
    }
  }
}