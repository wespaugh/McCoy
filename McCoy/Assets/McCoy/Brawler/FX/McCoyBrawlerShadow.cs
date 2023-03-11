using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.McCoy.Brawler.FX
{
  public class McCoyBrawlerShadow : MonoBehaviour
  {
    [SerializeField]
    SpriteRenderer shadow;

    public Transform ActorTransform{ get; set; }

    private void Update()
    {
      if(ActorTransform == null)
      {
        return;
      }
      gameObject.transform.position = new Vector3(ActorTransform.position.x, ActorTransform.position.z, ActorTransform.position.z);
    }
  }
}
