using Assets.McCoy.Localization;
using Assets.McCoy.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.BoardGame
{
  public class McCoyLobbyingListItem : MonoBehaviour
  {
    [SerializeField]
    McCoyLocalizedText titleLabel = null;

    [SerializeField]
    McCoyLocalizedText summaryLabel = null;

    [SerializeField]
    McCoyLocalizedText costLabel = null;

    [SerializeField]
    GameObject highlight = null;

    McCoyLobbyingCause.LobbyingCause cause;
    int cost;
    public int Cost { get => cost; }

    [SerializeField]
    Button lobbyButton;

    McCoyCityScreen root;

    public void Initialize(McCoyLobbyingCause cause, McCoyCityScreen root)
    {
      this.cause = cause.lobbyingCause;
      this.cost = cause.cost;
      this.root = root;
      updateButtonEnabled();
      titleLabel.SetText(cause.title);
      summaryLabel.SetText(cause.summary);
      ProjectConstants.Localize("com.mccoy.boardgame.lobbyingcost", (costString) =>
      {
        int credits = McCoyGameState.Instance().Credits;
        bool canBuy = Cost <= credits;
        string text = $"{Cost}";
        if (!canBuy)
        {
          text = $"<color=red>{text}</color>";
        }
        text = $"{costString}: {text}";
        costLabel.SetTextDirectly(text);
      });
    }

    public void ToggleHighlight(bool on)
    {
      highlight.SetActive(on);
    }

    private void updateButtonEnabled()
    {
      lobbyButton.enabled = !McCoyGameState.Instance().causesLobbiedFor.Contains(this.cause) && McCoyGameState.Instance().Credits >= cost;
    }

    public void LobbyForCause()
    {
      McCoyLobbyingCauseManager.GetInstance().ApplyCause(cause, root);
      lobbyButton.enabled = false;
      Debug.Log("Before: " + McCoyGameState.Instance().Credits);
      McCoyGameState.Instance().Spend(cost);
      Debug.Log("After: " + McCoyGameState.Instance().Credits);
    }
  }
}
