using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyMainMenuScene : DefaultMainMenuScreen
  {
    McCoy game;
    public void Awake()
    {
      if(game == null)
      {
        game = FindObjectOfType<McCoy>();
      }
    }
    public void StartCityScene()
    {
      game.LoadScene(McCoy.McCoyScenes.CityMap);
    }
  }
}