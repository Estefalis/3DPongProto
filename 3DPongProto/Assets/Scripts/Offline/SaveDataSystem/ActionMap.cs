using System;
using System.Collections.Generic;

namespace ThreeDeePongProto.Shared.InputActions
{
	[Serializable]
	public class ActionMap
	{
		public List<ActionMapBindings> ActionMapEntries = new ();
	}
}