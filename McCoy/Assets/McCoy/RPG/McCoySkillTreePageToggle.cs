using UnityEngine;
using static Assets.McCoy.RPG.McCoySkillTreeMenu;

namespace Assets.McCoy.RPG
{
  public class McCoySkillTreePageToggle : MonoBehaviour
  {
    [SerializeField]
    SkillTreeTab tabToggle;
    public SkillTreeTab Tab
    {
      get => tabToggle;
    }
  }
}
