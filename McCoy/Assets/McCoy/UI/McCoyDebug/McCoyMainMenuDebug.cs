using Assets.McCoy.Brawler.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.McCoy.UI.McCoyDebug
{
  public class McCoyMainMenuDebug : MonoBehaviour
  {
    [SerializeField]
    GameObject dbgInterfacePrefab = null;

    GameObject debugUI = null;

    private void OnEnable()
    {
      if(McCoy.GetInstance().DebugUI)
      {
        if(debugUI == null)
        {
          debugUI = Instantiate(dbgInterfacePrefab, transform);
        }
      }
      else
      {
        if(debugUI != null)
        {
          Destroy(debugUI);
          debugUI = null;
        }
      }
    }
  }
}
