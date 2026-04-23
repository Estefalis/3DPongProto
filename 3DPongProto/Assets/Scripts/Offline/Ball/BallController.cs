using System;
using ThreeDeePongProto.Shared.AudioManagement;
using ThreeDeePongProto.Shared.Managers;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class BallController : MonoBehaviour
{
    #region Core Settings
    [Header("Core Settings")]
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private float m_initialSpeed = 10.0f;
    [SerializeField, Min(25.0f)] private float m_offWallAngle = 25.0f;
    [SerializeField, Min(0.1f)] private float m_offPaddleAngle = 0.1f;
    #endregion

    #region Dynamic Physics Settings
    [Header("Dynamic Physics")]
    [SerializeField] private float m_minSpeed = 10f;
    [SerializeField] private float m_maxSpeed = 40f;
    [SerializeField, Tooltip("How much of the paddle's speed transfers to the ball")] 
    private float m_paddleMomentumTransfer = 0.5f;
    
    private float m_currentSpeedTarget; //The current target speed the ball is trying to maintain.
    #endregion

    #region References
    private LocalMatchManager m_matchManager;
    [SerializeField] private AudioSource m_ballAudioSource;
    private Vector3 m_ballPopPosition;
    private Quaternion m_ballPopRotation;
    private int m_trackId = 0, m_firstPlayerID = -1;
    #endregion

    #region Actions
    public static event Action OnHitGoalOne, OnHitGoalTwo;  //LocalMatchManager updates MatchUserInterface with OnScoreChanged.
    public static event Action<int> OnHitPlayer;            //LocalMatchManager keep track of Players hit by Ball for goal-notifications.
    public static event Action OnFirstServe;                //LocalMatchManager saves MatchStartTime and sets 'MatchHasStarted'-Bool to true.
    public static event Action<ESoundEmittingObjects, EAudioType, int, bool> PlaySpecificAudio;
    //TODO: Audioplay-Structure: (Emitter, AudioSourceSettings (Diegetic/NonDiegetic), Track-ID (if not random), RandomBool);
    #endregion

    private void Awake()
    {
        m_matchManager = FindObjectOfType<LocalMatchManager>();

        if (m_rigidbody == null)
            m_rigidbody = GetComponent<Rigidbody>();

        if (m_ballAudioSource == null)
            m_ballAudioSource = GetComponent<AudioSource>();

        m_ballPopPosition = m_rigidbody.position;
        m_ballPopRotation = m_rigidbody.rotation;
    }

    private void OnEnable()
    {
        AudioManager.LetsRegisterAudioSources(m_ballAudioSource);
        CharacterInputHandler.AKickBall += BallStart;
    }

    private void OnDisable()
    {
        AudioManager.LetsRemoveAudioSources(m_ballAudioSource);
        CharacterInputHandler.AKickBall -= BallStart;
    }

    private void FixedUpdate()
    {
        //STRICTLY limit Ball-GameObjects velocity to defined min- and max-values. But only, if the ball is actually moving.
        if (m_rigidbody.velocity.sqrMagnitude > 0.1f)
            EnforceSpeedLimits();
    }

    #region State Management
    private void ResetBall()
    {
        //Rigibody-reset must happen first, or movementSpeed could be added multiple times and it's rotation would stay changed.
        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.position = m_ballPopPosition;
        m_rigidbody.rotation = m_ballPopRotation;
        m_firstPlayerID = -1;
        m_currentSpeedTarget = m_initialSpeed; //Reset target speed to the startValue.
    }


    private void BallStart(int _masterID)
    {
        int firstPlayerID = -1;

        //Garanty to only apply the force on the ball only once.
        if (!m_matchManager.GameIsPaused && m_firstPlayerID == firstPlayerID)
        {
            m_firstPlayerID = _masterID;
            if (!m_matchManager.MatchIsActive)
                OnFirstServe?.Invoke();

            ApplyForceOnBall();
        }
    }

    private void ApplyForceOnBall()
    {
        if (m_rigidbody.velocity != Vector3.zero) return;

        // Determine a random direction, avoiding straight horizontal/vertical
        float randomAngle = GetRandomServeAngle();
        
        // Convert angle to a direction vector (assuming Y is up, playing on X/Z plane)
        Vector3 serveDirection = new Vector3(Mathf.Sin(randomAngle * Mathf.Deg2Rad), 0, Mathf.Cos(randomAngle * Mathf.Deg2Rad));

        m_currentSpeedTarget = m_initialSpeed;
        m_rigidbody.velocity = serveDirection * m_currentSpeedTarget;

        //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
        PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);    //BallstartSound
    }

    private float GetRandomServeAngle()
    {
        int sideChoice = UnityEngine.Random.Range(0, 4);
        return sideChoice switch
        {
            0 => UnityEngine.Random.Range(m_offPaddleAngle, 90f - m_offWallAngle),
            1 => UnityEngine.Random.Range(90f + m_offWallAngle, 180f - m_offPaddleAngle),
            2 => UnityEngine.Random.Range(180f + m_offPaddleAngle, 270f - m_offWallAngle),
            _ => UnityEngine.Random.Range(270f + m_offWallAngle, 360f - m_offPaddleAngle),
        };
    }
    #endregion

    #region Physics & Collision Logic
    private void EnforceSpeedLimits()
    {
        //Clamp the target speed between the defined min- and max-value.
        m_currentSpeedTarget = Mathf.Clamp(m_currentSpeedTarget, m_minSpeed, m_maxSpeed);

        //Force the Rigidbody to maintain this exact speed along its current trajectory.
        m_rigidbody.velocity = m_rigidbody.velocity.normalized * m_currentSpeedTarget;
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("GoalOne"))
        {
            //LocalMatchManager: WinCondition-Check & increase Points of Team2/Player2 - MatchUserInterface: Updates MatchUI - PlayerControls: Resets Paddle on Goal
            OnHitGoalOne?.Invoke();
            HandleGoalScored();
        }
        else if (_other.CompareTag("GoalTwo"))
        {
            //LocalMatchManager: WinCondition-Check & increase Points of Team1/Player1 - MatchUserInterface: Updates MatchUI - PlayerControls: Resets Paddle on Goal
            OnHitGoalTwo?.Invoke();
            HandleGoalScored();
        }
    }

    private void HandleGoalScored()
    {
        //In AudioManager: (AudioType, EAudioType 2D/3D, List/Array-ID, Track-ID (if not random), SpatialBlend, RandomBool);
        PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);
        ResetBall();
    }

    private void OnCollisionEnter(Collision _collision)
    {
        GameObject hitObject = _collision.gameObject;

        //On Paddles.
        if (hitObject.CompareTag("TpOne") || hitObject.CompareTag("TpTwo"))
        {
            PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.NonDiegetic, m_trackId, false);
            
            //Try to get the player script.
            if (hitObject.transform.parent.TryGetComponent<CharacterMovement>(out var player))
            {
                OnHitPlayer?.Invoke(player.m_playerID);
                
                // --- DYNAMIC ACCELERATION LOGIC ---
                //Try to get the paddle's Rigidbody to read its velocity.
                if (player.TryGetComponent<Rigidbody>(out var playerRb))
                {
                    float paddleSpeed = playerRb.velocity.magnitude;
                    
                    //Add a portion of the paddle's speed to the ball's target speed.
                    m_currentSpeedTarget += paddleSpeed * m_paddleMomentumTransfer;
                }
            }
        }
        //On Walls.
        else if (hitObject.CompareTag("EastWall") || hitObject.CompareTag("WestWall"))
        {
            PlaySpecificAudio?.Invoke(ESoundEmittingObjects.Ball, EAudioType.Diegetic, m_trackId, false);
            
            // Optional: You could slightly decrease speed here if you want friction
            // m_currentSpeedTarget *= 0.98f; 
        }
    }
    #endregion
}