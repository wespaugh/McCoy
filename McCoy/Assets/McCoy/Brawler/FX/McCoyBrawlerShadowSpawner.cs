using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.McCoy.Brawler.FX
{
  public class McCoyBrawlerShadowSpawner : MonoBehaviour
  {
    [SerializeField]
    GameObject shadowPrefab = null;
    GameObject shadow = null;

    public void Start()
    {
      shadow = Instantiate(shadowPrefab);
      shadow.GetComponent<McCoyBrawlerShadow>().ActorTransform = transform;
    }

    private void OnDestroy()
    {
      if(shadow == null)
      {
        return;
      }
      shadow.GetComponent<McCoyBrawlerShadow>().ActorTransform = null;
      Destroy(shadow);
    }
  }
}
