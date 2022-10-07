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
    public List<MoveInfo> MovesToEnable = new List<MoveInfo>();
    public List<object> BuffsToAdd = new List<object>();

    public string Name;
    public int Level;
    public int MaxLevel;
  }
}
