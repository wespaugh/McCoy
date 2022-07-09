using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UFE3D
{
  namespace Brawler
  {

    public class BrawlerSpawnedEntityManager
    {
      Dictionary<int, UFEController> controllers = new Dictionary<int, UFEController>();
      Dictionary<int, ControlsScript> controlsScripts = new Dictionary<int, ControlsScript>();
      List<int> availableIDs = new List<int>();

      public int GetAvailableID()
      {
        if (availableIDs.Count > 0)
        {
          return availableIDs[0];
        }
        // (plus 1 because 0 isn't an index ever)
        else return controlsScripts.Count + 1;
      }

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
        availableIDs.Remove(id);
        controllers[id] = c;
      }

      public Dictionary<int, UFEController>.ValueCollection GetAllControllers()
      {
        return controllers.Values;
      }

      public void SetControlsScript(ControlsScript cs, int id)
      {
        if (controlsScripts.ContainsKey(id) && controlsScripts[id] != null)
        {
          Debug.LogWarning("Controls Script already exists for player with ID " + id);
        }
        availableIDs.Remove(id);
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

      public int GetNumLivingEntities()
      {
        int count = 0;
        foreach(var c in controlsScripts)
        {
          if (c.Value != null && ! c.Value.isDead) ++count;
        }
        return count;
      }

      public void ReleaseController(int id)
      {
        availableIDs.Add(id);
        if (controlsScripts.ContainsKey(id))
        {
          controlsScripts[id] = null;
        }
        if (controllers.ContainsKey(id))
        {
          controllers[id] = null;
        }
      }
    }
  }
}