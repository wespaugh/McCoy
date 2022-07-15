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

      public void PrintDebugger()
      {
        Debug.Log("Welcome to the Controls Scripts Debugger. Let's hope this never need to debug FluxCapacitor");
        Debug.Log("#UFEControllers: " + controllers.Count);
        Debug.Log("#ControlsScripts: " + controlsScripts.Count);

        foreach(var c in controllers)
        {
          if(!controlsScripts.ContainsKey(c.Key))
          {
            Debug.Log("UFEControllers retained a key " + c.Key + " that controllers doesn't have. Is it player 1?");
          }
          if( c.Value == null )
          {
            Debug.Log("UFEController at " + c.Key + " was null");
            if(controlsScripts[c.Key] != null)
            {
              Debug.Log("even weirder, the corresponding controller WASN'T null!");
            }
          }
        }
        foreach (var c in controlsScripts)
        {
          // don't reconsider places we've already examined in controllers
          if (controllers.ContainsKey(c.Key)) continue;
          Debug.Log("ID " + c.Key + " was found in controlScripts that didn't exist in controllers. You didn't expect this!");
          if(c.Value == null)
          {
            Debug.Log("AND! It was null. Wait, that's good, right? is it good?");
          }
          else
          {
            Debug.Log("But it wasn't null! That's probably bad, right?");
          }
        }
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