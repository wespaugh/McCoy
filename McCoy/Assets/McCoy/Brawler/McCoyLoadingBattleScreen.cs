using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UFE3D;
using TMPro;
using Assets.McCoy;

public class McCoyLoadingBattleScreen : LoadingBattleScreen
{
	#region public instance properties
	public AudioClip onLoadSound;
	public AudioClip music;
	public float delayBeforeMusic = .1f;
	public float delayBeforePreload = .5f;
	public float delayAfterPreload = .5f;
	public TMP_Text nameStage;
	public Image screenshotStage;
	public bool stopPreviousSoundEffectsOnLoad = false;
	#endregion

	#region public override methods
	public override void OnShow()
	{
		base.OnShow();

		if (this.music != null)
		{
			UFE.DelayLocalAction(delegate () { UFE.PlayMusic(this.music); }, this.delayBeforeMusic);
		}

		if (this.stopPreviousSoundEffectsOnLoad)
		{
			UFE.StopSounds();
		}

		if (this.onLoadSound != null)
		{
			UFE.DelayLocalAction(delegate () { UFE.PlaySound(this.onLoadSound); }, this.delayBeforeMusic);
		}

		if (UFE.config.selectedStage != null)
		{
			nameStage.text = McCoy.GetInstance().currentStage.Name;
			if (this.screenshotStage != null)
			{
				this.screenshotStage.sprite = Sprite.Create(
					UFE.config.selectedStage.screenshot,
					new Rect(0f, 0f, UFE.config.selectedStage.screenshot.width, UFE.config.selectedStage.screenshot.height),
					new Vector2(0.5f * UFE.config.selectedStage.screenshot.width, 0.5f * UFE.config.selectedStage.screenshot.height)
				);

				Animator anim = this.screenshotStage.GetComponent<Animator>();
				if (anim != null)
				{
					anim.enabled = false;// UFE.gameMode != GameMode.StoryMode;
				}
			}
		}

		UFE.DelayLocalAction(UFE.PreloadBattle, this.delayBeforePreload);
		UFE.DelayLocalAction(this.StartBattle, UFE.config._preloadingTime);
	}
	#endregion
}
