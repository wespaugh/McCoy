using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyGenericTextAccessor : MonoBehaviour
  {
    [SerializeField]
    TMP_Text text = null;

    public TMP_Text Text => text;
  }
}
