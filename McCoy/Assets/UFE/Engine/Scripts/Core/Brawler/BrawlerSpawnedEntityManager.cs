using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UFE3D
{
  namespace Brawler
  {
    public enum Faction
    {
      None = 0,
      Werewolf,
      Mage,
      Bull,
      Angel
    }

    public class BrawlerSpawnedEntityManager
    {
      Dictionary<Faction, List<UFEController>> factionLookup = new Dictionary<Faction, List<UFEController>>();
      Dictionary<int, UFEController> controllers = new Dictionary<int, UFEController>();
      Dictionary<int, ControlsScript> controlsScripts = new Dictionary<int, ControlsScript>();

      public UFEController GetController(int playerId)
      {
        return controllers.ContainsKey(playerId) ? controllers[playerId] : null;
      }
      public void SetController(UFEController c, int id)
      {
        if (controllers.ContainsKey(id) && controllers[id] != null)
        {
          Debug.LogWarning("Controller already exists for player with ID " + id);
        }
        controllers[id] = c;
      }

      public Dictionary<int, UFEController>.ValueCollection GetAllControllers()
      {
        return controllers.Values;
      }

      public void SetControlsScript(ControlsScript cs, int id)
      {
        if(controlsScripts.ContainsKey(id) && controlsScripts[id] != null)
        {
          Debug.LogWarning("Controls Script already exists for player with ID " + id);
        }
        controlsScripts[id] = cs;
      }

      public ControlsScript GetControlsScript(int id)
      {
        return controlsScripts[id];
      }

      public Dictionary<int, ControlsScript> GetAllControlsScripts()
      {
        return controlsScripts;
      }
    }
  }
}