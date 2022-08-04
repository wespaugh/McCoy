using System;
using System.Collections.Generic;
using UFE3D;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.Brawler
{
  [Serializable]
  public class McCoyFactionLookup : ScriptableObject, ICloneable
  {
    public static string DEFAULT_FACTIONS_FILENAME = "FactionsLookup";

    [SerializeField]
    public List<UFE3D.CharacterInfo> mageCharacters = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> minotaurCharacters = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> militiaCharacters = new List<UFE3D.CharacterInfo>();

    private Dictionary<string, UFE3D.CharacterInfo> mageLookup;
    private Dictionary<string, UFE3D.CharacterInfo> minotaurLookup;
    private Dictionary<string, UFE3D.CharacterInfo> militiaLookup;

    [MenuItem("Assets/Create/McCoy/Faction Lookup")]
    static void MakeFactionData()
    {
      AssetDatabase.CreateAsset(CreateInstance<McCoyFactionLookup>(), $"{ProjectConstants.RESOURCES_DIRECTORY}{ProjectConstants.FACTIONLOOKUP_DIRECTORY}/{DEFAULT_FACTIONS_FILENAME}.asset");
      AssetDatabase.SaveAssets();
    }

    static McCoyFactionLookup _instance;
    public static McCoyFactionLookup GetInstance()
    {
      if(_instance == null)
      {
        _instance = Resources.Load<McCoyFactionLookup>($"{ProjectConstants.FACTIONLOOKUP_DIRECTORY}/{DEFAULT_FACTIONS_FILENAME}") as McCoyFactionLookup;
      }
      return _instance;
    }

    public object Clone()
    {
      return CloneObject.Clone(this);
    }

    public void FindCharacterInfo(string enemyName, out UFE3D.CharacterInfo charInfo, out Factions faction)
    {
      charInfo = null;
      faction = Factions.None;

      if(mageLookup == null)
      {
        initLookups();
      }
      if(mageLookup.ContainsKey(enemyName))
      {
        charInfo = mageLookup[enemyName];
        faction = Factions.Mages;
      }
      else if(minotaurLookup.ContainsKey(enemyName))
      {
        charInfo = minotaurLookup[enemyName];
        faction = Factions.CyberMinotaurs;
      }
      else if(militiaLookup.ContainsKey(enemyName))
      {
        charInfo = militiaLookup[enemyName];
        faction = Factions.AngelMilitia;
      }
    }

    private void initLookups()
    {
      mageLookup = new Dictionary<string, UFE3D.CharacterInfo>();
      foreach (var mage in mageCharacters)
      {
        mageLookup[mage.name] = mage;
      }

      minotaurLookup = new Dictionary<string, UFE3D.CharacterInfo>();
      foreach (var minotaur in minotaurCharacters)
      {
        minotaurLookup[minotaur.name] = minotaur;
      }

      militiaLookup = new Dictionary<string, UFE3D.CharacterInfo>();
      foreach (var m in militiaCharacters)
      {
        militiaLookup[m.name] = m;
      }
    }
    public UFE3D.CharacterInfo RandomEnemy(Factions f)
    {
      Dictionary<string, UFE3D.CharacterInfo> collection;
      switch (f)
      {
        case Factions.Mages:
          collection = mageLookup;
          break;
        case Factions.CyberMinotaurs:
          collection = minotaurLookup;
          break;
        case Factions.AngelMilitia:
          collection = militiaLookup;
          break;
        default:
          return null;
      }

      int index = UnityEngine.Random.Range(0, collection.Count);
      var enu = collection.Values.GetEnumerator();
      if (index > 0)
      {
        for (int i = 0; i < index; ++i)
        {
          enu.MoveNext();
        }
      }
      return enu.Current;
    }
  }
}