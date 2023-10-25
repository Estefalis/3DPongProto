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

    #region #region SerializeField-Member-Variables
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private float m_impulseForce;
    [SerializeField] private float m_offWallAngle = 15.0f;
    [SerializeField] private float m_offPaddleAngle = 0.1f;
    //[SerializeField] float m_onContactAddUp = 1.10f;

    [SerializeField] private readonly string m_goalOne = "GoalOne";
    [SerializeField] private readonly string m_goalTwo = "GoalTwo";
    [SerializeField] private readonly string m_teamPlayerOne = "TpOne";
    [SerializeField] private readonly string m_teamPlayerTwo = "TpTwo";
    //[SerializeField] private readonly string m_teamPlayerThree = "TpThree";
    //[SerializeField] private readonly string m_teamPlayerFour = "TpFour";
    [SerializeField] private readonly string m_eastWall = "EastWall";
    [SerializeField] private readonly string m_westWall = "WestWall";
    #endregion

    /*TODO:
     * References:
     * Player:      - Player 1 & 2, if AdditionalSpeed (m_onContactAddUp) shall be applied
     *              - AudioSource
     *              - AudioClipArray/-List
     */

    #region #region Non-SerializeField-Member-Variables
    private Vector3 m_ballPopPosition;
    private Quaternion m_ballPopRotation;

    #region Actions
    //TODO: Add parameter on Actions (<Mixergroup/Soundtype>, <AudioSource> (Ball), 2D/3D), so the Audiomanager basicly only has to "chose" a sound.
    public static event Action HitGoalOne;
    public static event Action HitGoalTwo;
    public static event Action HitPaddleTpOne;
    public static event Action HitPaddleTpTwo;
    public static event Action HitEastWall;
    public static event Action HitWestWall;
    public static event Action PlayBallStartSound;
    public static event Action RoundCountStarts;
    #endregion
    #endregion

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
        int sideChoice;
        if (DateTime.Now.Millisecond < 500)
            sideChoice = Mathf.FloorToInt(UnityEngine.Random.Range(0, 4) + DateTime.Now.Millisecond / 1000);
        else
            sideChoice = Mathf.RoundToInt(UnityEngine.Random.Range(0, 4) + DateTime.Now.Millisecond / 1000);

        transform.eulerAngles = sideChoice switch
        {
            //0 = directly to the playerTwo-paddle, 90 = directly to the eastWall.
            0 => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0 + m_offPaddleAngle, 90 - m_offWallAngle), transform.eulerAngles.z),
            //90 = directly to the eastWall, 180 = directly to the playerOne-paddle.
            1 => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(90 + m_offWallAngle, 180 - m_offPaddleAngle), transform.eulerAngles.z),
            //180 = directly to the playerOne-paddle, 270 = directly to the westWall.
            2 => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(180 + m_offPaddleAngle, 270 - m_offWallAngle), transform.eulerAngles.z),
            //270 = directly to the westWall, (36)0 = directly to the playerTwo-paddle.
            3 => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(270 + m_offWallAngle, 360 - m_offPaddleAngle), transform.eulerAngles.z),
            //Default case.
            _ => new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0 + m_offPaddleAngle, 90 - m_offWallAngle), transform.eulerAngles.z),
        };

        m_rigidbody.AddRelativeForce(transform.forward * m_impulseForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.CompareTag(m_goalOne))
        {
            //Match-Points of Player/Team 2 are increasing.
            HitGoalOne?.Invoke();
            ResetBall();
        }

        if (_other.gameObject.CompareTag(m_goalTwo))
        {
            //Match-Points of Player/Team 1 are increasing.
            HitGoalTwo?.Invoke();
            ResetBall();
        }
    }

    private void StartBallMovement(InputAction.CallbackContext _callbackContext)
    {
        if (!m_matchManager.GameIsPaused)
        {
            ResetBall();
            ApplyForceOnBall();

            if (!m_matchManager.MatchStarted)
            {
                //AudioManager shall play a certain sound on BallStart.
                PlayBallStartSound?.Invoke();
                //MatchManager saves values at GameStart.
                RoundCountStarts?.Invoke();                
            }
        }
    }

    private void OnCollisionEnter(Collision _collision)
    {
        if(_collision.gameObject.CompareTag(m_teamPlayerOne))
        {
            //"Wireless" connection to tell the Audiomanager which (kind of) sound to play.
            HitPaddleTpOne?.Invoke();
        }

        if (_collision.gameObject.CompareTag(m_teamPlayerTwo))
        {
            //"Wireless" connection to tell the Audiomanager which (kind of) sound to play.
            HitPaddleTpTwo?.Invoke();
        }

        if (_collision.gameObject.CompareTag(m_eastWall))
        {
            HitEastWall?.Invoke();
        }

        if (_collision.gameObject.CompareTag(m_westWall))
        {
            HitWestWall?.Invoke();
        }

        //if (_collision.gameObject.CompareTag("Player"))
        //{
        //    m_rigidbody.AddForce(_collision.GetContact(0).normal * m_onContactAddUp, ForceMode.Impulse);
        //}
    }
}