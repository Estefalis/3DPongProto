using UnityEngine;

public class SmoothCamFollow : MonoBehaviour
{
    [SerializeField] Transform m_player;
    [SerializeField] Vector3 m_offset;

    [Range (1, 10)]
    [SerializeField] float m_smoothfactor;

    private void FixedUpdate ()
    {
        Follow ();
    }

    void Follow()
    {
        Vector3 desiredPosition = m_player.position + m_offset;
        Vector3 smoothedFollowing = Vector3.Lerp (transform.position, desiredPosition, m_smoothfactor * Time.fixedDeltaTime);
        transform.position = smoothedFollowing;

        //transform.LookAt (m_player);
    }
}