using Assets.McCoy.BoardGame;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  public class McCoyCryptId
  {
    public Vector3 Position
    {
      get; private set;
    }

    private GameObject worldObject;

    public int Amount
    {
      get; private set;
    }

    public bool PickedUp
    {
      get; private set;
    }

    public McCoyCryptId Initialize(Vector3 position, int amount, GameObject drop)
    {
      this.Amount = amount;
      this.Position = position;
      this.worldObject = drop;
      return this;
    }
    public void PickUp()
    {
      GameObject.Destroy(worldObject);
      PickedUp = true;
      McCoyGameState.Instance().PickUpCredits(Amount);
    }
  }
}