using UnityEngine;

public class CamViewSetup : MonoBehaviour
{
    [SerializeField] private Camera m_playerCam1;
    [SerializeField] private Camera m_playerCam2;
    [SerializeField] private Camera m_funCam1;
    [SerializeField] private Camera m_funCam2;

    [SerializeField] private float m_fullWidthHor, m_halfWidthVer;
    [SerializeField] private float m_halfHeightHor, m_fullHeightVer;

    [SerializeField] private bool m_verticalIfTrue = false;
    [SerializeField] private bool m_forFunBool = false;

    //FullCamWindow: Width 536.4 - Height 302.

    private void OnEnable()
    {
        if (m_funCam1 != null && m_funCam2 != null && m_forFunBool)
        {
            SetPizzaFourSeasons(m_playerCam1, m_playerCam2, m_funCam1, m_funCam2);
            return;
        }
        else if (m_verticalIfTrue)
            SetCamerasVertical(m_playerCam1, m_playerCam2);
        else
            SetCamerasHorizontal(m_playerCam1, m_playerCam2);
    }

    private void SetCamerasHorizontal(Camera _camera1, Camera _camera2, Camera _camera3 = null, Camera _camera4 = null)
    {
        float Cam1X = _camera1.pixelRect.x;
        float Cam1Y = _camera1.pixelRect.y;
        float Cam1W = _camera1.pixelRect.width;
        float Cam1H = _camera1.pixelRect.height;

        _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_fullWidthHor, Cam1H * m_halfHeightHor);

        float Cam2X = _camera2.pixelRect.x;
        float Cam2Y = _camera2.pixelRect.y;
        float Cam2W = _camera2.pixelRect.width;
        float Cam2H = _camera2.pixelRect.height;

        Cam2Y += Cam2H * m_halfHeightHor;
        _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_fullWidthHor, Cam2H * m_halfHeightHor);
    }

    private void SetCamerasVertical(Camera _camera1, Camera _camera2, Camera _camera3 = null, Camera _camera4 = null)
    {
        float Cam1X = _camera1.pixelRect.x;
        float Cam1Y = _camera1.pixelRect.y;
        float Cam1W = _camera1.pixelRect.width;
        float Cam1H = _camera1.pixelRect.height;

        _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_halfWidthVer, Cam1H * m_fullHeightVer);

        float Cam2X = _camera2.pixelRect.x;
        float Cam2Y = _camera2.pixelRect.y;
        float Cam2W = _camera2.pixelRect.width;
        float Cam2H = _camera2.pixelRect.height;

        Cam2X += Cam2W * m_halfWidthVer;
        _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_halfWidthVer, Cam2H * m_fullHeightVer);
    }

    private void SetPizzaFourSeasons(Camera _camera1, Camera _camera2, Camera _camera3, Camera _camera4)
    {
        float Cam1X = _camera1.pixelRect.x;
        float Cam1Y = _camera1.pixelRect.y;
        float Cam1W = _camera1.pixelRect.width;
        float Cam1H = _camera1.pixelRect.height;

        float Cam2X = _camera2.pixelRect.x;
        float Cam2Y = _camera2.pixelRect.y;
        float Cam2W = _camera2.pixelRect.width;
        float Cam2H = _camera2.pixelRect.height;

        float Cam3X = _camera3.pixelRect.x;
        float Cam3Y = _camera3.pixelRect.y;
        float Cam3W = _camera3.pixelRect.width;
        float Cam3H = _camera3.pixelRect.height;

        float Cam4X = _camera4.pixelRect.x;
        float Cam4Y = _camera4.pixelRect.y;
        float Cam4W = _camera4.pixelRect.width;
        float Cam4H = _camera4.pixelRect.height;

        Cam1W *= m_halfWidthVer;
        _camera1.pixelRect = new Rect(Cam1X, Cam1Y, Cam1W * m_fullWidthHor, Cam1H * m_halfHeightHor);

        Cam2X += Cam2W * m_halfWidthVer;
        _camera2.pixelRect = new Rect(Cam2X, Cam2Y, Cam2W * m_halfWidthVer, Cam2H * m_halfHeightHor);

        Cam3Y += Cam3H * m_halfHeightHor;
        _camera3.pixelRect = new Rect(Cam3X, Cam3Y, Cam3W * m_halfWidthVer, Cam3H * m_halfHeightHor);

        Cam4X += Cam4W * m_halfWidthVer;
        Cam4Y += Cam4H * m_halfHeightHor;
        _camera4.pixelRect = new Rect(Cam4X, Cam4Y, Cam4W * m_halfWidthVer, Cam4H * m_halfHeightHor);
    }
}