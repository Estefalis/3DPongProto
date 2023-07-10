using UnityEngine;

public enum EGameModi
{
    LocalPC = default,
    LAN,
    Internet
}

public enum ECameraModi
{
    SingleCam = default,
    TwoHorizontal,
    TwoVertical,
    FourSplit
}

namespace ThreeDeePongProto.Managers
{
    public class GameManager : GenericSingleton<GameManager>
    {
        public ECameraModi ECameraMode { get => m_eCameraMode; set => m_eCameraMode = value; }
        [SerializeField] private ECameraModi m_eCameraMode;

        public EGameModi EGameConnectionModi { get => eGameConnectionMode; set => eGameConnectionMode = value; }
        [SerializeField] private EGameModi eGameConnectionMode;

        public bool GameIsPaused { get => m_gameIsPaused; set { m_gameIsPaused = value; } }
        [SerializeField] private bool m_gameIsPaused;

        #region GameSettings
        public float MaxFieldWidth { get => m_maxFieldWidth; private set => m_maxFieldWidth = value; }
        private float m_maxFieldWidth = 25.0f;  //Current Default until the FieldWidth gets set by SettingsOptions.

        public float MaxFieldLength { get => m_maxFieldLength; private set => m_maxFieldLength = value; }
        private float m_maxFieldLength = 50.0f;  //Current Default until the FieldWidth gets set by SettingsOptions.

        public float PaddleWidthAdjustment { get => m_paddleWidthAdjustment; private set => m_paddleWidthAdjustment = value; }
        private float m_paddleWidthAdjustment;
        #endregion

        public void SetFieldWidth(float _fieldWidth)
        {
            m_maxFieldWidth = _fieldWidth;
        }

        public void SetPaddleAdjustAmount(float _amount)
        {
            m_paddleWidthAdjustment += _amount;
        }
    }
}