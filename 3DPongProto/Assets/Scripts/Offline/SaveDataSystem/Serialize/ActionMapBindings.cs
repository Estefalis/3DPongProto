using System;

namespace ThreeDeePongProto.Shared.InputActions
{
    [Serializable]
    public struct ActionMapBindings
    {
        public string InputActionName;
        public string OverridePath;
        public Guid UniqueGuid;

        public ActionMapBindings(string _inputActionName, string _overridePath, Guid _uniqueGuid = default)
        {
            InputActionName = _inputActionName;
            OverridePath = _overridePath;
            UniqueGuid = _uniqueGuid;
        }
    }
}