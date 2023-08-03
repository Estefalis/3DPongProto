using System;
using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private float m_impulseForce = 10f;
    //[SerializeField] float m_onContactAddUp = 1.10f;

    private Vector3 m_ballPopPosition;
    private Quaternion m_ballPopRotation;

    /*TODO:
     * References: 
     * MatchUI      - RoundTextfield
     * Player:      - Player 1 & 2, if AdditionalSpeed (m_onContactAddUp) shall be applied
     *              - AudioSource
     *              - AudioClipArray/-List
     */

    //These uint-Actions tell the MatchUserInterface class to update the corresponding TMP element.
    public static event Action<uint> m_HitGoalOne;
    public static event Action<uint> m_HitGoalTwo;

    private void Awake()
    {
        if (m_rigidbody == null)
            m_rigidbody = GetComponentInChildren<Rigidbody>();

        m_ballPopPosition = m_rigidbody.position;
        m_ballPopRotation = m_rigidbody.rotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ResetBall();
            ApplyForceOnBall();
        }
    }

    private void ResetBall()
    {
        //Rigibody-reset must happen first, or movementSpeed could be added multiple times and it's rotation would stay changed.
        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.position = m_ballPopPosition;
        m_rigidbody.rotation = m_ballPopRotation;
    }

    private void ApplyForceOnBall()
    {
        int randomZDirection = UnityEngine.Random.Range(0, 2);

        //TODO: Research, if 'UnityEngine.Random.insideUnitCircle' could replace 'UnityEngine.Random.Range'.
        int sidechoice = UnityEngine.Random.Range(0, 3);
        if (sidechoice == 0)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(15, 55), transform.eulerAngles.z);
        else if (sidechoice == 1)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(125, 175), transform.eulerAngles.z);
        if (sidechoice == 2)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(195, 235), transform.eulerAngles.z);
        else if (sidechoice == 3)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(305, 345), transform.eulerAngles.z);

        m_rigidbody.AddRelativeForce(transform.forward * m_impulseForce, ForceMode.Impulse);

#if UNITY_EDITOR
        Debug.Log(sidechoice);
#endif
    }

    private void OnTriggerEnter(Collider _other)
    {
        ResetBall();

        if (_other.gameObject.CompareTag("GoalOne"))
        {
            //Match-Points of Player/Team 2 are increasing.
            m_HitGoalOne?.Invoke(2);
        }

        if (_other.gameObject.CompareTag("GoalTwo"))
        {
            //Match-Points of Player/Team 1 are increasing.
            m_HitGoalTwo?.Invoke(1);
        }
    }

    //private void OnCollisionEnter(Collision _collision)
    //{
    //    if (_collision.gameObject.CompareTag("Player"))
    //    {
    //        m_rigidbody.AddForce(_collision.GetContact(0).normal * m_onContactAddUp, ForceMode.Impulse);
    //    }
    //}
}