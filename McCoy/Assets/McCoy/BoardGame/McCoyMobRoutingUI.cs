using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  class McCoyMobRoutingUI : MonoBehaviour
  {
    [SerializeField]
    TMP_Text instructions = null;

    [SerializeField]
    GameObject zoneSelectPrefab = null;
  }
}
