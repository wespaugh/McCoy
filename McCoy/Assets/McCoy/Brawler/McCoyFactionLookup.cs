using System;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.Brawler
{
  [Serializable]
  public class McCoyFactionLookup : ScriptableObject, ICloneable
  {

    [SerializeField]
    public List<UFE3D.CharacterInfo> mageLookup1 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> mageLookup2 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> mageLookup3 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> mageLookup4 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> mageLookup5 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> mageLookup6 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> minotaurCharacters1 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> minotaurCharacters2 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> minotaurCharacters3 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> minotaurCharacters4 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> minotaurCharacters5 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> minotaurCharacters6 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> militiaCharacters1 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> militiaCharacters2 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> militiaCharacters3 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> militiaCharacters4 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> militiaCharacters5 = new List<UFE3D.CharacterInfo>();

    [SerializeField]
    public List<UFE3D.CharacterInfo> militiaCharacters6 = new List<UFE3D.CharacterInfo>();

    private Dictionary<string, Tuple<int, UFE3D.CharacterInfo>> mageLookup;
    private Dictionary<string, Tuple<int, UFE3D.CharacterInfo>> minotaurLookup;
    private Dictionary<string, Tuple<int, UFE3D.CharacterInfo>> militiaLookup;



    static McCoyFactionLookup _instance;
    public static McCoyFactionLookup GetInstance()
    {
      if(_instance == null)
      {
        _instance = Resources.Load<McCoyFactionLookup>($"{FACTIONLOOKUP_DIRECTORY}/{DEFAULT_FACTIONS_FILENAME}") as McCoyFactionLookup;
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

      initLookups();
      if(mageLookup.ContainsKey(enemyName))
      {
        charInfo = mageLookup[enemyName].Item2;
        faction = Factions.Mages;
      }
      else if(minotaurLookup.ContainsKey(enemyName))
      {
        charInfo = minotaurLookup[enemyName].Item2;
        faction = Factions.CyberMinotaurs;
      }
      else if(militiaLookup.ContainsKey(enemyName))
      {
        charInfo = militiaLookup[enemyName].Item2;
        faction = Factions.AngelMilitia;
      }
    }

    private void initLookups()
    {
      if(mageLookup != null)
      {
        return;
      }
      mageLookup = new Dictionary<string, Tuple<int, UFE3D.CharacterInfo>>();
      populateLookup(mageLookup, mageLookup1, 1);
      populateLookup(mageLookup, mageLookup2, 2);
      populateLookup(mageLookup, mageLookup3, 3);
      populateLookup(mageLookup, mageLookup4, 4);
      populateLookup(mageLookup, mageLookup5, 5);
      populateLookup(mageLookup, mageLookup6, 6);


      minotaurLookup = new Dictionary<string, Tuple<int, UFE3D.CharacterInfo>>();
      populateLookup(minotaurLookup, minotaurCharacters1, 1);
      populateLookup(minotaurLookup, minotaurCharacters2, 2);
      populateLookup(minotaurLookup, minotaurCharacters3, 3);
      populateLookup(minotaurLookup, minotaurCharacters4, 4);
      populateLookup(minotaurLookup, minotaurCharacters5, 5);
      populateLookup(minotaurLookup, minotaurCharacters6, 6);

      militiaLookup = new Dictionary<string, Tuple<int, UFE3D.CharacterInfo>>();
      populateLookup(militiaLookup, militiaCharacters1, 1);
      populateLookup(militiaLookup, militiaCharacters2, 2);
      populateLookup(militiaLookup, militiaCharacters3, 3);
      populateLookup(militiaLookup, militiaCharacters4, 4);
      populateLookup(militiaLookup, militiaCharacters5, 5);
      populateLookup(militiaLookup, militiaCharacters6, 6);
    }

    void populateLookup(Dictionary<string, Tuple<int, UFE3D.CharacterInfo>> lookup, List<UFE3D.CharacterInfo> characters, int strength)
    {
      foreach(var character in characters)
      {
        if(lookup.ContainsKey(character.characterName))
        {
          continue;
        }
        lookup[character.characterName] = new Tuple<int, UFE3D.CharacterInfo>(strength, character);
      }
    }

    public UFE3D.CharacterInfo RandomEnemy(Factions f, int strength = 1)
    {
      if (mageLookup == null)
      {
        initLookups();
      }

      Dictionary<string, Tuple<int, UFE3D.CharacterInfo>> collection;
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

      if(collection == null)
      {
        Debug.Log("NULL COLLECTION FOR " + f);
      }

      int index = UnityEngine.Random.Range(0, collection.Count);
      var enu = collection.Values.GetEnumerator();
      enu.MoveNext();
      if (index > 0)
      {
        for (int i = 0; i < index; ++i)
        {
          enu.MoveNext();
        }
      }
      return enu.Current.Item2;
    }

    public int XPForMonster(string monsterName)
    {
      initLookups();
      int retVal = 0;
      if(mageLookup.ContainsKey(monsterName))
      {
        retVal = mageLookup[monsterName].Item1;
      }
      else if(minotaurLookup.ContainsKey(monsterName))
      {
        retVal = minotaurLookup[monsterName].Item1;
      }
      else if(militiaLookup.ContainsKey(monsterName))
      {
        retVal = militiaLookup[monsterName].Item1;
      }
      retVal *= 2;
      return retVal;
    }
  }
}