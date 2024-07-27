using System;
using System.Collections.Generic;

namespace ThreeDeePongProto.Shared.InputActions
{
	[Serializable]
	public class ActionMapParent
	{
		public List<ActionMapBindings> ActionMapEntries = new ();
	}
}