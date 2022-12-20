using Assets.McCoy.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UFE3D;
using UnityEngine;
using UnityEngine.UI;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.BoardGame
{
  class McCoyMobRoutingUI : MonoBehaviour, IMcCoyInputManager
  {
    const string factionAnimatorParam = "Faction";
    [SerializeField]
    TMP_Text alert = null;

    [SerializeField]
    TMP_Text instructions = null;

    [SerializeField]
    McCoyMobRoutingDestination zoneSelectPrefab = null;

    [SerializeField]
    Button leftButton = null;

    [SerializeField]
    Button rightButton = null;

    [SerializeField]
    Transform zoneSelectRoot = null;

    [SerializeField]
    Animator factionIconAnimator = null;

    [SerializeField]
    AudioClip mobCombatSound = null;

    List<Tuple<MapNode, McCoyMobData>> zoneMobs = new List<Tuple<MapNode, McCoyMobData>>();
    int zoneIndex = 0;

    List<McCoyMobRoutingDestination> zoneSelectObjects = new List<McCoyMobRoutingDestination>();

    McCoyCityBoardContents board;

    // combats awaiting animation once a mob move has finished
    Dictionary<MapNode, Factions> pendingCombats = new Dictionary<MapNode, Factions>();

    Action<bool> routingFinishedCallback = null;
    private bool inputInitialized;
    private McCoyInputManager input;

    int selectedZoneIndex = 0;

    public void Initialize(Dictionary<MapNode, List<McCoyMobData>> routedMobsInMapNodes, Action<bool> routingFinished, McCoyCityBoardContents board)
    {
      this.board = board;
      routingFinishedCallback = routingFinished;
      int totalCount = 0;

      foreach (var zone in routedMobsInMapNodes)
      {
        totalCount += zone.Value.Count;
        foreach (McCoyMobData mob in zone.Value)
        {
          // don't let the player route this mob back to its starting location
          mob.AddRoutingLocation(zone.Key);
          zoneMobs.Add(new Tuple<MapNode, McCoyMobData>(zone.Key, mob));
        }
      }

      if (totalCount == 0)
      {
        Debug.LogError("why are we here?");
        return;
      }

      zoneIndex = 0;
      updateZoneIndex();
    }

    private void updateZoneIndex(bool fromCombo = false)
    {
      while(zoneSelectObjects.Count > 0)
      {
        var obj = zoneSelectObjects[0];
        zoneSelectObjects.RemoveAt(0);
        Destroy(obj.gameObject);
      }

      if (zoneMobs.Count <= 1)
      {
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
      }

      alert.text = $"Mobs Routed out of {zoneMobs[zoneIndex].Item1.ZoneName}!";
      if (fromCombo)
      {
        instructions.text = $"Follow-Up Attack! Pick a zone and route {FactionDisplayName(zoneMobs[zoneIndex].Item2.Faction)} even further!";
      }
      else
      { 
        instructions.text = $"{FactionDisplayName(zoneMobs[zoneIndex].Item2.Faction)} Routed! Pick a zone and send them packing!"; 
      }

      List<MapNode> validConnections = this.validConnections();
      board.SelectMapNode(zoneMobs[zoneIndex].Item1, validConnections);

      switch (zoneMobs[zoneIndex].Item2.Faction)
      {
        case Factions.Mages:
          factionIconAnimator.SetInteger(factionAnimatorParam, 2);
          break;
        case Factions.AngelMilitia:
          factionIconAnimator.SetInteger(factionAnimatorParam, 3);
          break;
        case Factions.CyberMinotaurs:
          factionIconAnimator.SetInteger(factionAnimatorParam, 4);
          break;
      }
      factionIconAnimator.SetTrigger("Reset");
      foreach(var mapNode in validConnections)
      {
        var zoneSelect = Instantiate(zoneSelectPrefab, zoneSelectRoot);
        zoneSelect.Initialize(zoneMobs[zoneIndex].Item1, mapNode as MapNode, zoneMobs[zoneIndex].Item2, mobRouted);
        zoneSelectObjects.Add(zoneSelect);
      }
      updateSelection();
    }

    private List<MapNode> validConnections()
    {
      List<MapNode> retVal = new List<MapNode>();

      foreach (var mapNode in zoneMobs[zoneIndex].Item1.GetConnectedNodes())
      {
        if(zoneMobs[zoneIndex].Item2.CanRouteTo(mapNode as MapNode))
        {
          retVal.Add(mapNode as MapNode);
        }
      }

      return retVal;
    }

    private void mobRouted(MapNode originalLocation, MapNode newLocation, McCoyMobData m)
    {
      // players at the destination take attacks of opportunity before the mob enter (and the mob only merges with aligned mobs in the destination zone after these attacks)
      //
      // assume we're done routing, but if player attacks hurt enough, we might get to route again
      m.IsRouted = false;
      for (int i = 0; i < PlayerCharacters.Length; ++i)
      {
        var playerLoc = board.NodeWithID(McCoy.GetInstance().gameState.PlayerLocation(PlayerCharacters[i]));
        if (playerLoc.NodeID == newLocation.NodeID)
        {
          if(!pendingCombats.ContainsKey(newLocation))
          {
            pendingCombats.Add(newLocation, m.Faction);
          }
          m.OffscreenCombat(3);
        }
      }

      McCoyMobData mob = m;

      // if the mob is still alive after any opportunity attacks
      if (!mob.MarkedForDeath)
      {
        // physically move the mob, and perform any combining that might need done in the new location
        mob = moveMob(originalLocation, newLocation, mob);
      }
      // if the mob is dead, just remove it from the board
      else
      {
        originalLocation.Mobs.Remove(mob);
      }

      // don't revisit the new location in subsequent routes
      mob.AddRoutingLocation(newLocation);

      // update the mob (the mob is at a new location now, and the mob being routed previously may have gotten absorbed)
      zoneMobs[zoneIndex] = new Tuple<MapNode, McCoyMobData>(newLocation, mob);

      // being able to continue routing this mob (if requested) requires there be a valid location to route it to
      zoneMobs[zoneIndex].Item2.IsRouted &= validConnections().Count > 0;

      // flag will be set to true if the original location still contains a mob of the relevant faction
      // this is an important animation flag
      bool originalContainsFaction = false;
      foreach(var origMob in originalLocation.Mobs)
      {
        if(origMob.Faction == mob.Faction)
        {
          originalContainsFaction = true;
          break;
        }
      }

      // update the map for the newly moved mob
      board.AnimateMobMove(mob.Faction, originalLocation, newLocation, .5f, mobMoveFinished, !originalContainsFaction);

      // if we're truly done routing this mob...
      if (!mob.IsRouted)
      {
        // reset some routing flags
        mob.FinishedRouting();

        // remove the mob, and if there are no more, close the dialog
        zoneMobs.RemoveAt(zoneIndex);
        if (zoneMobs.Count == 0)
        {
          closeDialog();
          return;
        }
        // but if there are more, move to the next one and continue
        else if (zoneIndex >= zoneMobs.Count)
        {
          --zoneIndex;
        }
        updateZoneIndex();
      }
      // and if we're NOT done routing this mob (like, we pushed it somewhere that 1+ players exist)
      else
      {
        updateZoneIndex(true);
      }
    }

    private void mobMoveFinished()
    {
      if(pendingCombats.Count > 0)
      {
        UFE.PlaySound(mobCombatSound);
      }
      foreach(var pendingCombat in pendingCombats)
      {
        board.AnimateMobCombat(pendingCombat.Key, pendingCombat.Value);
      }
      pendingCombats.Clear();
      routingFinishedCallback(zoneMobs.Count == 0);
    }

    private void closeDialog()
    {
      Destroy(gameObject);
    }

    private McCoyMobData moveMob(MapNode originalLocation, MapNode newLocation, McCoyMobData mob)
    {
      McCoyMobData retVal = mob;
      McCoyMobData existingMob = null;
      foreach (var destMob in newLocation.Mobs)
      {
        if (destMob.Faction == mob.Faction)
        {
          existingMob = destMob;
          break;
        }
      }

      // if there wasn't a mob of the same faction there, just move it
      // also, if we're routing the mob immediately after this, we don't want to absorb it yet
      if (existingMob == null || mob.IsRouted)
      {
        originalLocation.Mobs.Remove(mob);
        if (!mob.IsRouted)
        {
          newLocation.Mobs.Add(mob);
        }
      }
      // otherwise, the stronger mob will absorb the weaker mob
      else
      {
        // if incoming mob is stronger than existing mob, absorb existing mob and replace it
        if (mob.XP > existingMob.XP)
        {
          mob.Absorb(existingMob);

          originalLocation.Mobs.Remove(mob);
          newLocation.Mobs.Remove(existingMob);
          newLocation.Mobs.Add(mob);
        }
        // if incoming mob isn't stronger than existing mob, existing mob will absorb weaker mob
        else
        {
          existingMob.Absorb(mob);
          originalLocation.Mobs.Remove(mob);
          // replace the original mob
          retVal = existingMob;
        }
      }
      return retVal;
    }

    public void NextMob()
    {
      ++zoneIndex;
      if(zoneIndex >= zoneMobs.Count)
      {
        zoneIndex = 0;
      }
      updateZoneIndex();
    }
    public void PreviousMob()
    {
      --zoneIndex;
      if(zoneIndex < 0)
      {
        zoneIndex = zoneMobs.Count - 1;
      }
      updateZoneIndex();
    }

    public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if (!inputInitialized)
      {
        inputInitialized = true;
        input = new McCoyInputManager();
        input.RegisterButtonListener(ButtonPress.Forward, navigateRight);
        input.RegisterButtonListener(ButtonPress.Back, navigateLeft);
        input.RegisterButtonListener(ButtonPress.Button6, NextMob);
        input.RegisterButtonListener(ButtonPress.Button5, PreviousMob);
        input.RegisterButtonListener(ButtonPress.Button2, ConfirmMob);
      }
      return input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2PreviousInputs);
    }

    private void ConfirmMob()
    {
      zoneSelectObjects[selectedZoneIndex].ZoneSelected();
    }

    private void navigateLeft()
    {
      --selectedZoneIndex;
      if(selectedZoneIndex < 0)
      {
        selectedZoneIndex = zoneSelectObjects.Count - 1;
      }
      updateSelection();
    }

    private void navigateRight()
    {
      ++selectedZoneIndex;
      if(selectedZoneIndex >= zoneSelectObjects.Count)
      {
        selectedZoneIndex = 0;
      }
      updateSelection();
    }

    private void updateSelection()
    {
      foreach(var zoneObj in zoneSelectObjects)
      {
        zoneObj.ToggleHighlight(false);
      }
      zoneSelectObjects[selectedZoneIndex].ToggleHighlight(true);
    }
  }
}
