namespace Assets.McCoy.Brawler.Buffs
{
  public class BrawlerDashingBuff : BrawlerBuffDelegate
  {

    bool log = false;

    public override string EditorHelper()
    {
      return "";
    }

    public override void Apply()
    {
      base.Apply();
      player.isDashing = true;
    }
    public override void Remove(int numAppliedBeforeRemoving)
    {
      base.Remove(numAppliedBeforeRemoving);
      if(numAppliedBeforeRemoving == 1)
      {
        player.isDashing = false;
      }
    }
  }
}
