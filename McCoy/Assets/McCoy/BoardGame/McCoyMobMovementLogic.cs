using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using Assets.McCoy.UI;
using UnityEngine.ProBuilder.Shapes;

namespace Assets.McCoy.BoardGame
{
  public class McCoyMobMovementLogic : MonoBehaviour
  {
    [SerializeField]
    bool OnlyStrongestMobsSearch = false;
    [SerializeField]
    AudioClip mobCombatSound = null;
    [SerializeField]
    float weekendStartDelay = 1.5f;
    [SerializeField]
    float delayBetweenCombatAndRouting = 0.5f;

    Action weekendFinishedCallback = null;

    // mob routing caches
    Dictionary<MapNode, List<McCoyMobData>> toRoute = new Dictionary<MapNode, List<McCoyMobData>>();
    int mobsMoving = 0;

    McCoyCityBoardContents board;
    List<MapNode> MapNodes => board.MapNodes;

    public void Initialize(McCoyCityBoardContents board)
    {
      this.board = board;
    }

    #region Weekend
    public void Weekend(Action callback)
    {
      weekendFinishedCallback = callback;
      StartCoroutine(runWeekend());
    }
    private IEnumerator runWeekend()
    {
      yield return new WaitForSeconds(weekendStartDelay);
      toRoute.Clear();

      bool playCombatSound = false;

      foreach (MapNode node in MapNodes)
      {
        if(node.Disabled)
        {
          continue;
        }
        List<McCoyMobData> mobsDefeated = new List<McCoyMobData>();
        for (int i = 0; i < node.Mobs.Count; ++i)
        {
          bool combat = false;
          for (int j = i + 1; j < node.Mobs.Count; ++j)
          {
            combat = true;
            playCombatSound = true;
            // i fights j
            node.Mobs[i].OffscreenCombat(node.Mobs[j].StrengthForXP());
            board.AnimateMobCombat(node, node.Mobs[i].Faction);
            // j fights i
            node.Mobs[j].OffscreenCombat(node.Mobs[i].StrengthForXP());
            board.AnimateMobCombat(node, node.Mobs[j].Faction);

            if (node.Mobs[i].IsRouted && !mobsDefeated.Contains(node.Mobs[i]))
            {
              mobsDefeated.Add(node.Mobs[i]);
            }
            if (node.Mobs[j].IsRouted && !mobsDefeated.Contains(node.Mobs[i]))
            {
              mobsDefeated.Add(node.Mobs[j]);
            }
          }
          node.Mobs[i].WeekEnded(combat);
        }
        if (mobsDefeated.Count > 0)
        {
          toRoute.Add(node, mobsDefeated);
        }
      }

      if (playCombatSound)
      {
        UFE.PlaySound(mobCombatSound);
      }

      StartCoroutine(waitForWeekendMobConflictsAnim());
    }

    private IEnumerator waitForWeekendMobConflictsAnim()
    {
      yield return new WaitForSeconds(delayBetweenCombatAndRouting);

      // if all the conflicts happened but there's no one left to route, tell WeekendMobRouted that the last mob did route
      if (toRoute.Count == 0)
      {
        mobsMoving = 1;
        WeekendMobRouted();
      }
      foreach (var routingOriginMobsPair in toRoute)
      {
        foreach (var mob in routingOriginMobsPair.Value)
        {
          MapNode conn = getRandomConnection(routingOriginMobsPair.Key, mob);
          // if somehow the mob is cornered with no valid location to move to, just finish
          if (conn == null)
          {
            mob.FinishedRouting();
          }
          else
          {
            moveMobToConnectedLocation(routingOriginMobsPair.Key.Mobs, mob, conn);
            ++mobsMoving;
            board.AnimateMobMove(mob.Faction, routingOriginMobsPair.Key, conn, 1.0f, WeekendMobRouted);
          }
        }
      }
      if (mobsMoving == 0)
      {
        VoluntaryMovementPhase();
      }
    }

    private void moveMobToConnectedLocation(List<McCoyMobData> routingOriginMobs, McCoyMobData mob, MapNode connection)
    {
      routingOriginMobs.Remove(mob);

      McCoyMobData existingMob = null;
      foreach (var mobAtConnection in connection.Mobs)
      {
        if (mobAtConnection.Faction == mob.Faction)
        {
          existingMob = mobAtConnection;
          break;
        }
      }
      // if there was no mob of the same faction there, we can just add it straight there
      if (existingMob == null)
      {
        connection.Mobs.Add(mob);
      }
      // otherwise, combine the weaker mob into the stronger mob
      else
      {
        if (existingMob.XP > mob.XP)
        {
          existingMob.Absorb(mob);
        }
        else
        {
          connection.Mobs.Remove(existingMob);
          mob.Absorb(existingMob);
          connection.Mobs.Add(mob);
        }
      }
    }

    private MapNode getRandomConnection(MapNode origin, McCoyMobData mob)
    {
      MapNode retVal = null;
      List<SearchableNode> connections = origin.GetConnectedNodes();
      while (connections.Count > 0)
      {
        int idx = UnityEngine.Random.Range(0, connections.Count);
        retVal = (connections[idx]) as MapNode;
        connections.RemoveAt(idx);
        if (!retVal.Disabled && !mob.CanRouteTo(retVal))
        {
          retVal = null;
        }
        else
        {
          break;
        }
      }
      return retVal;
    }

    private void WeekendMobRouted()
    {
      --mobsMoving;
      if (mobsMoving == 0)
      {
        VoluntaryMovementPhase();
      }
    }

    private void VoluntaryMovementPhase()
    {
      if (mobsMoving == 0)
      {
        List<Tuple<McCoyMobData, MapNode>> mobsThatCanStillMove = new List<Tuple<McCoyMobData, MapNode>>();

        foreach (var mapNode in MapNodes)
        {
          if(mapNode.Disabled)
          {
            continue;
          }
          foreach (McCoyMobData m in mapNode.Mobs)
          {
            if (!m.IsRouted)
            {
              mobsThatCanStillMove.Add(new Tuple<McCoyMobData, MapNode>(m, mapNode));
            }
          }
        }

        while (mobsThatCanStillMove.Count > 0)
        {
          int idx = UnityEngine.Random.Range(0, mobsThatCanStillMove.Count);
          Tuple<McCoyMobData, MapNode> nodePair = mobsThatCanStillMove[idx];
          mobsThatCanStillMove.RemoveAt(idx);
          decideMobMove(nodePair);
        }

        if (mobsMoving == 0)
        {
          finishWeekEnd();
        }
      }
    }

    private void decideMobMove(Tuple<McCoyMobData, MapNode> nodePair)
    {
      MapNode moveTarget = null;
      McCoyMobData moveSubject = nodePair.Item1;

      foreach (var connection in nodePair.Item2.GetConnectedNodes())
      {
        MapNode neighbor = connection as MapNode;
        // if there's an adjacent place we can divide into, divide into
        if (neighbor.Mobs.Count == 0 && nodePair.Item1.XP >= 8)
        {
          moveTarget = neighbor;
          moveSubject = nodePair.Item1.Split();
          break;
        }
        bool shouldMove = true;
        foreach (var mob in neighbor.Mobs)
        {
          // if there's already a mob of the same faction or if there is a faction too strong to challenge, bail
          if (mob.Faction == nodePair.Item1.Faction || mob.XP * 2 >= nodePair.Item1.XP)
          {
            shouldMove = false;
            break;
          }
        }
        if (shouldMove)
        {
          moveTarget = neighbor;
          break;
        }
      }
      if (moveTarget != null)
      {
        ++mobsMoving;
        bool hideOriginal = false;
        // if the mob is moving (rather than being created from a split)
        if (moveSubject == nodePair.Item1)
        {
          nodePair.Item2.Mobs.Remove(moveSubject);
          hideOriginal = true;
        }
        moveTarget.Mobs.Add(moveSubject);
        board.AnimateMobMove(moveSubject.Faction, nodePair.Item2, moveTarget, .5f, voluntaryMoveFinished, hideOriginal);
      }
    }

    private void voluntaryMoveFinished()
    {
      --mobsMoving;
      if (mobsMoving == 0)
      {
        finishWeekEnd();
      }
    }

    private void finishWeekEnd()
    {
      foreach (var m in MapNodes)
      {
        foreach (var mob in m.Mobs)
        {
          if (mob.IsRouted)
          {
            mob.FinishedRouting();
          }
        }
      }

      advanceDoomsdayClock();
      board.UpdateNodes();
      if (weekendFinishedCallback != null)
      {
        weekendFinishedCallback();
      }
    }

    private void advanceDoomsdayClock()
    {
      foreach (var node in MapNodes)
      {
        McCoyMobData strongestMob = null;
        foreach (var mob in node.Mobs)
        {
          if (strongestMob == null || mob.XP > strongestMob.XP)
          {
            strongestMob = mob;
          }
          if (!OnlyStrongestMobsSearch)
          {
            node.Search(mob.XP, (int)mob.Health);
          }
        }
        if (strongestMob == null)
        {
          continue;
        }
        if (OnlyStrongestMobsSearch)
        {
          node.Search(strongestMob.XP, (int)strongestMob.Health);
        }
      }
      MapNodes.Sort((a, b) => { return b.SearchStatus() - a.SearchStatus(); });
    }
    #endregion
  }
}