namespace UFE3D
{
	public class FluxPlayer
	{
		#region public instance properties
		public FluxPlayerInputBuffer inputBuffer
		{
			get
			{
				return this._inputBuffer;
			}
		}

		public UFEController inputController
		{
			get
			{
				return UFE.GetController(this.player);
			}
		}

		public bool isLocalPlayer
		{
			get
			{
				//			int localPlayer = UFE.GetLocalPlayer();
				//			return localPlayer <= 0 && localPlayer == this.player;

				return !this.isRemotePlayer;
			}
		}

		public bool isRemotePlayer
		{
			get
			{
				return this.player == UFE.GetRemotePlayer();
			}
		}

		public ControlsScript controlsScript
		{
			get
			{
				return UFE.GetControlsScript(this.player);
			}
		}

		public int player
		{
			get
			{
				return this._player;
			}
		}
		#endregion

		#region private instance properties
		public FluxPlayerInputBuffer _inputBuffer = new FluxPlayerInputBuffer();
		public int _player;
		#endregion

		#region public instance constructors
		public FluxPlayer(int player) : this(player, 0) { }

		public FluxPlayer(int player, int currentFrame) : this(player, currentFrame, -1) { }

		public FluxPlayer(int player, int currentFrame, int maxBufferSize)
		{
			this.Initialize(currentFrame, maxBufferSize, player);
		}
		#endregion

		#region public instance methods
		public virtual void Initialize(int id)
		{
			this.Initialize(0, id);
		}

		public virtual void Initialize(long currentFrame, int id)
		{
			this.Initialize(currentFrame, -1, id);
		}

		public virtual void Initialize(long currentFrame, int maxBufferSize, int id)
		{
	  _player = id;
			this._inputBuffer.Initialize(currentFrame, maxBufferSize);
		}

		public bool RemoveNextInput()
		{
			return this._inputBuffer.RemoveNextInput();
		}
		#endregion
	}
}