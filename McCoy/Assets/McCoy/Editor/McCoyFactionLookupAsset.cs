using Assets.McCoy.Brawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Assets.McCoy.Editor
{
  public class McCoyFactionLookupAsset : ScriptableObject
  {

    [MenuItem("Assets/Create/McCoy/Faction Lookup")]
    static void MakeFactionData()
    {
      AssetDatabase.CreateAsset(CreateInstance<McCoyFactionLookup>(), $"{ProjectConstants.RESOURCES_DIRECTORY}{ProjectConstants.FACTIONLOOKUP_DIRECTORY}/{ProjectConstants.DEFAULT_FACTIONS_FILENAME}.asset");
      AssetDatabase.SaveAssets();
    }
  }
}
