using System;
using ThreeDeePongProto.Managers;
using ThreeDeePongProto.Offline.Player.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallMovement : MonoBehaviour
{
    #region Script-References
    [SerializeField] private MatchManager m_matchManager;
    private PlayerInputActions m_ballMovement;
    #endregion

    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private float m_impulseForce;
    [SerializeField] private float m_offWallAngle = 15.0f;
    [SerializeField] private float m_offPaddleAngle = 0.1f;
    [SerializeField] private MatchValues m_matchValues;
    //[SerializeField] float m_onContactAddUp = 1.10f;

    private Vector3 m_ballPopPosition;
    private Quaternion m_ballPopRotation;

    /*TODO:
     * References:
     * Player:      - Player 1 & 2, if AdditionalSpeed (m_onContactAddUp) shall be applied
     *              - AudioSource
     *              - AudioClipArray/-List
     */

    //These uint-Actions tell the MatchUserInterface class to update the corresponding TMP element.
    public static event Action m_HitGoalOne;
    public static event Action m_HitGoalTwo;
    public static event Action m_RoundCountStarts;

    private void Awake()
    {
        if (m_rigidbody == null)
            m_rigidbody = GetComponentInChildren<Rigidbody>();

        m_ballPopPosition = m_rigidbody.position;
        m_ballPopRotation = m_rigidbody.rotation;
    }

    private void Start()
    {
        m_ballMovement = UserInputManager.m_playerInputActions;
        m_ballMovement.PlayerActions.Enable();
        m_ballMovement.PlayerActions.PokeTheBall.performed += StartBallMovement;
    }

    private void OnDisable()
    {
        m_ballMovement.PlayerActions.Disable();
        m_ballMovement.PlayerActions.PokeTheBall.performed -= StartBallMovement;
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
        //TODO: Research, if 'UnityEngine.Random.insideUnitCircle' could replace 'UnityEngine.Random.Range'.
        int sideChoice = 0;

        if (DateTime.Now.Millisecond < 500)
            sideChoice = Mathf.FloorToInt(UnityEngine.Random.Range(0, 4) + DateTime.Now.Millisecond / 1000);
        else
            sideChoice = Mathf.RoundToInt(UnityEngine.Random.Range(0, 4) + DateTime.Now.Millisecond / 1000);
#if UNITY_EDITOR
        //Debug.Log($"BallMovement Randomed Side: {sideChoice}");
#endif
        switch (sideChoice)
        {
            case 0:
                //0 = directly to the playerTwo-paddle, 90 = directly to the eastWall.
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0 + m_offPaddleAngle, 90 - m_offWallAngle), transform.eulerAngles.z);
                break;
            case 1:
                //90 = directly to the eastWall, 180 = directly to the playerOne-paddle.
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(90 + m_offWallAngle, 180 - m_offPaddleAngle), transform.eulerAngles.z);
                break;
            case 2:
                //180 = directly to the playerOne-paddle, 270 = directly to the westWall.
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(180 + m_offPaddleAngle, 270 - m_offWallAngle), transform.eulerAngles.z);
                break;
            case 3:
                //270 = directly to the westWall, (36)0 = directly to the playerTwo-paddle.
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(270 + m_offWallAngle, 360 - m_offPaddleAngle), transform.eulerAngles.z);
                break;
            default:
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0 + m_offPaddleAngle, 90 - m_offWallAngle), transform.eulerAngles.z);
                break;
        }

        m_rigidbody.AddRelativeForce(transform.forward * m_impulseForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider _other)
    {
        ResetBall();

        if (_other.gameObject.CompareTag("GoalOne"))
        {
            //Match-Points of Player/Team 2 are increasing.
            m_HitGoalOne?.Invoke();
        }

        if (_other.gameObject.CompareTag("GoalTwo"))
        {
            //Match-Points of Player/Team 1 are increasing.
            m_HitGoalTwo?.Invoke();
        }
    }

    private void StartBallMovement(InputAction.CallbackContext _callbackContext)
    {
        if (!GameManager.Instance.GameIsPaused)
        {
            ResetBall();
            ApplyForceOnBall();

            if (!m_matchManager.MatchStarted)
            {
                m_RoundCountStarts?.Invoke();
                //m_matchValues.StartDateTime = DateTime.Now.Ticks;
                m_matchValues.StartTime = Time.time;
            }
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