using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFE3D;

namespace Assets.McCoy.UI
{
  interface IMcCoyInputManager
  {
    public bool CheckInputs(
    IDictionary<InputReferences, InputEvents> player1PreviousInputs,
    IDictionary<InputReferences, InputEvents> player1CurrentInputs,
    IDictionary<InputReferences, InputEvents> player2PreviousInputs,
    IDictionary<InputReferences, InputEvents> player2CurrentInputs
    );
  }
}
