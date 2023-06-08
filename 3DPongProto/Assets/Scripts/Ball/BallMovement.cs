using UnityEngine;
using ThreeDeePongProto.Managers;

public class BallMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody m_rigidbody;

    [SerializeField] private float m_impulseForce = 10f;

    private Vector3 m_ballPopPosition;
    /*TODO:
     * References: 
     * GameManager: - GameRunsBool (GameIsPaused)
     *              - Current PlayerScoreTextfields
     *              - RoundTextfield
     * Player:      - Player 1 & 2
     * Ball:        - Rigidbody
     *              - ImpulseForce on Rigidbody
     *              - BallMovementSpeed (m_impulseForce) and AdditionalSpeed (m_onContactAddUp)
     *              - GoalTriggers
     *              - AudioSource
     *              - AudioClipArray/-List
     *              - BallStartPosition
     */
    private void Awake()
    {
        if(m_rigidbody == null)
            m_rigidbody = GetComponentInChildren<Rigidbody>();

        m_ballPopPosition = m_rigidbody.position;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.B))
    //        ApplyForceOnBall();
    //}

    //private void ApplyForceOnBall()
    //{
    //    //Reset before reapplying.
    //    m_rigidbody.velocity = Vector3.zero;
    //    m_rigidbody.AddRelativeForce(m_rigidbody.transform.forward * Random.insideUnitCircle.x * m_impulseForce, ForceMode.Impulse);
    //}
}