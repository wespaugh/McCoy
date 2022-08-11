using FPLibrary;
using System;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;

namespace Assets.McCoy.Brawler.Stages
{
  [Serializable]
  public class BrawlerStageInfo : ScriptableObject, ICloneable
  {
    public string Name = "New Stage";

    public List<BrawlerSubstageInfo> substages = new List<BrawlerSubstageInfo>();

    public object Clone()
    {
      return CloneObject.Clone(this);
    }

    public void GetXBounds(int currentRound, float x, float y, out float xMin, out float xMax)
    {
      var substage = substages[currentRound-1];
      BrawlerStageBoundary bounds = substage.boundaries[0];
      foreach (var b in substage.boundaries)
      {
        // if the next boundary being considered is ahead of where we are checking, we already found the boundary we needed
        if (b.unitXStart > x)
        {
          break;
        }
      }
      float? upperBoundIntersect = null;
      if(bounds.upperBound.slope != 0.0f)
      {
        // x = (y-b)/m
        upperBoundIntersect = (y - bounds.upperBound.yIntercept) / bounds.upperBound.slope;
        // if the intersection point is before the segment starts, there's no collision at our current Y
        if(upperBoundIntersect <= bounds.unitXStart)
        {
          upperBoundIntersect = null;
        }
      }
      float? lowerBoundIntersect = null;
      if (bounds.lowerBound.slope != 0.0f)
      {
        // x = (y-b)/m
        lowerBoundIntersect = (y - bounds.lowerBound.yIntercept) / bounds.lowerBound.slope;
        if(lowerBoundIntersect <= bounds.unitXStart)
        {
          lowerBoundIntersect = null;
        }
      }

      // use the left bound of the current segment, minus just a little to let the player move into the next segment
      xMin = bounds.unitXStart - .1f;
      // if neither defined, use the stage boundaries
      if(upperBoundIntersect == null && lowerBoundIntersect == null)
      {
        xMax = (float)(UFE.config.selectedStage.position.x + UFE.config.selectedStage._rightBoundary);
      }
      else if(upperBoundIntersect != null)
      {
        // if both are defined, use the nearer one
        if(lowerBoundIntersect != null)
        {
          xMax = upperBoundIntersect.Value < lowerBoundIntersect.Value ? upperBoundIntersect.Value : lowerBoundIntersect.Value;
        }
        // if only upper bound defined, use it
        else
        {
          xMax = upperBoundIntersect.Value;
        }
      }
      // if only lower bound defined, use it
      else
      {
        xMax = lowerBoundIntersect.Value;
      }
    }

    public void GetYBounds(int currentRound, float x, out float yMin, out float yMax)
    {
      var substage = substages[currentRound-1];
      BrawlerStageBoundary bounds = substage.boundaries[0];
      foreach (var b in substage.boundaries)
      {
        // if the next boundary being considered is ahead of where we are checking, we already found the boundary we needed
        if( b.unitXStart > x )
        {
          break;
        }
        bounds = b;
      }
      float adjustedX = x - bounds.unitXStart;
      // y = mx+b
      yMin = bounds.lowerBound.slope * adjustedX + bounds.lowerBound.yIntercept;
      yMax = bounds.upperBound.slope * adjustedX + bounds.upperBound.yIntercept;
    }
  }


  [Serializable]
  public class BrawlerStageBoundary : ICloneable
  {
    public float unitXStart;

    public BrawlerSlope upperBound;
    public BrawlerSlope lowerBound;
    public object Clone()
    {
      return CloneObject.Clone(this);
    }
  }

  [Serializable]
  public class BrawlerSlope : ICloneable
  {
    public float slope;
    public float yIntercept; 
    public object Clone()
    {
      return CloneObject.Clone(this);
    }
  }


}