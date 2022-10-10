using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class SearchableNode
  {
    private const int MAX_SEARCH_DEPTH = 6;

    [SerializeField]
    public string NodeID;

    public List<string> connectionIDs = new List<string>();

    [NonSerialized]
    List<SearchableNode> connectedNodes = new List<SearchableNode>();

    public List<SearchableNode> GetConnectedNodes()
    {
      return new List<SearchableNode>(connectedNodes);
    }

    public bool ConnectedToNode(SearchableNode other)
    {
      return connectedNodes.Contains(other);
    }

    public void AddConnectedNode(SearchableNode other)
    {
      if (!connectedNodes.Contains(other))
      {
        connectedNodes.Add(other);
      }
        if (!connectionIDs.Contains(other.NodeID))
      {
        connectionIDs.Add(other.NodeID);
      }
    }

    private class SearchState
    {
      public enum SearchType
      {
        BreadthFirst,
        DepthFirst
      }

      public SearchState Parent
      { 
        get; 
        private set; 
      }

      int myDepth;
      public SearchableNode node 
      { 
        get; 
        private set; 
      }

      SearchableNode target;

      public bool Exhausted
      {
        get;
        private set;
      }

      bool branchesInitialized = false;

      SearchType searchType;
      Dictionary<string, SearchState> branches = new Dictionary<string, SearchState>();

      public SearchState(SearchState parent, int depth, SearchableNode node, SearchableNode target, SearchType searchType)
      {
        this.Parent = parent;
        myDepth = depth;
        this.node = node;
        this.target = target;
        this.searchType = searchType;
      }

      private void initializeBranches()
      {
        if(branchesInitialized)
        {
          return;
        }
        branchesInitialized = true;
        foreach (var connection in node.connectedNodes)
        {
          SearchState ancestor = this;
          bool alreadySearched = false;
          while(ancestor != null)
          {
            if(ancestor.node.NodeID == connection.NodeID)
            {
              alreadySearched = true;
              break;
            }
            ancestor = ancestor.Parent;
          }
          if(alreadySearched)
          {
            continue;
          }
          // todo: maybe randomize the order in which the branches are added?
          branches.Add(connection.NodeID, new SearchState(this, myDepth + 1, connection, target, searchType));
        }
      }

      public bool SearchToDepth(List<SearchState> results, int maxDepth = -1)
      {
        initializeBranches();

        bool continueSearching = true;
        // if we're doing a capped search and depth is past the cap
        if(maxDepth != -1 && myDepth > maxDepth)
        {
          continueSearching = false;
        }
        // if we're already exhausted, because all our children are exhausted and we've already considered ourselves, stop searching
        continueSearching &= !Exhausted;

        if(continueSearching && node.NodeID == target.NodeID)
        {
          Exhausted = true;
          results.Add(this);
          continueSearching = false;
        }

        int activeBranches = 0;
        foreach(var branch in branches)
        {
          if (! branch.Value.Exhausted)
          {
            activeBranches++;
          }
        }
        /*
        Debug.Log(sb == "" ? "NO VALID NEIGHBORS" : sb);
        Debug.Log("BEGIN RECURSING CHILDREN");
        */
        // if there is nowhere left to search, because there are no connections (shouldn't be possible?) or because all connections have already been searched, bail out
        if(activeBranches == 0)
        {
          Exhausted = true;
          continueSearching = false;
        }

        if (continueSearching)
        {
          if (searchType == SearchType.BreadthFirst)
          {
            int currentDepth = myDepth;
            while(currentDepth != maxDepth)
            {
              ++currentDepth;
              if(currentDepth >= MAX_SEARCH_DEPTH)
              {
                Exhausted = true;
                continueSearching = false;
                break;
              }
              int validBranches = 0;
              foreach(var branch in branches)
              {
                if(branch.Value.Exhausted)
                {
                  continue;
                }
                if (branch.Value.SearchToDepth(results, currentDepth) && !branch.Value.Exhausted)
                {
                  ++validBranches;
                }
              }
              if(validBranches == 0)
              {
                Exhausted = true;
                continueSearching = false;
                break;
              }
            }
          }
        }

        return continueSearching;
      }
    }

    public int DistanceTo(SearchableNode other)
    {
      int distance = 0;

      SearchState root = new SearchState(null, 0, this, other, SearchState.SearchType.BreadthFirst);

      List<SearchState> results = new List<SearchState>();
      root.SearchToDepth(results);

      if(results.Count != 0)
      {
        string sb = "";
        int i = 0;
        foreach(var solution in results)
        {
          sb = "";
          SearchState s = solution;
          while (s.Parent != null)
          {
            sb = $"{(s.node as MapNode).ZoneName}->{sb}";
            s = s.Parent;
          }
          sb = $"{(s.node as MapNode).ZoneName}->{sb}";
          if (i++ == 0)
          {
            //Debug.Log("Solution: " + sb);
          }
        }
      }
      if (results.Count == 0)
      {
        Debug.Log("FOUCLTN find route between " + (this as MapNode).ZoneName + " and " + (other as MapNode).ZoneName);
        return -1;
      }
      var result = results[0];
      while(result.Parent != null)
      {
        result = result.Parent;
        ++distance;
      }

      return distance;
    }
  }
}