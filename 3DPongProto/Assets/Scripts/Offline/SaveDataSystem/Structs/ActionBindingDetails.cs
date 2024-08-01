using System;

namespace ThreeDeePongProto.Shared.InputActions
{
    [Serializable]
    public struct ActionBindingDetails
    {
        public string InputActionName;
        public int BindindIndex;
        public string OverridePath;
        public Guid UniqueGuid;

        public ActionBindingDetails(string _inputActionName, int _bindindIndex, string _overridePath, Guid _uniqueGuid = default)
        {
            InputActionName = _inputActionName;
            BindindIndex = _bindindIndex;
            OverridePath = _overridePath;
            UniqueGuid = _uniqueGuid;
        }
    } 
}