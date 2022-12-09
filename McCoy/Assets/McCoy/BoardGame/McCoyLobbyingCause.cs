using System;
using System.Collections.Generic;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class McCoyLobbyingCause : ICloneable
  {
    public enum LobbyingCause
    {
      AfterSchoolPrograms,
      AutomaticWeaponsBan,
      ConsumptionSites,
      Healthcare,
      LoanForgiveness,
      PrisonReform,
      RaiseMinimumWage,
      RemoveAntiHomelessArchitecture,
      RentControl,
      SkyBridge,
      SocialWorkers,
      Subway,
      StateMedia,
    }
    public LobbyingCause lobbyingCause;

    // quest name
    public string title;

    // brief overview
    public string summary;

    // credits required to unlock
    public int cost;

    public object Clone()
    {
      return this.MemberwiseClone();
    }
  }
}
