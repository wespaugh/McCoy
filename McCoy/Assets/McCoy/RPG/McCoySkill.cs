using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFE3D;

namespace Assets.McCoy.RPG
{
  [Serializable]
  public class McCoySkill
  {
    [NonSerialized]
    bool initialized = false;
    [NonSerialized]
    List<MoveInfo> movesToEnable = new List<MoveInfo>();
    [NonSerialized]
    public List<object> BuffsToAdd = new List<object>();

    public string Name;
    public int Level;
    public int MaxLevel;

    public List<MoveInfo> EnabledMoves
    {
      get
      {
        if(! initialized)
        {
          Initialize();
        }
        return movesToEnable;
      }
    }

    private void Initialize()
    {
      movesToEnable = McCoy.GetInstance().SkillLookup.GetMoveUnlocksForSkill(Name);
      initialized = true;
    }

    public McCoySkill(string name, int level, int maxLevel)
    {
      Name = name;
      Level = level;
      MaxLevel = maxLevel;
    }
  }
}
