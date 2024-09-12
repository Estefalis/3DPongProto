using System;
using System.Collections;
using ThreeDeePongProto.Offline.Managers;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] internal PlayerController m_playerController;

        [Header("Player Details")]
        [SerializeField] private float m_maxRotationAngle;
        [SerializeField, Range(1, 20)] protected float m_movementSpeed = 10.0f;
        [SerializeField, Range(1, 5)] protected float m_rotationSpeed = 2.5f;

        [Header("Forward-Movement")]
        //PushDistance for 'Mathf.MoveTowards'.
        [SerializeField] protected float m_lerpDuration = 1.5f;
        [SerializeField] protected float m_delayRetreat;
        [SerializeField] protected float m_delayRepetition;
        [SerializeField] protected bool m_enablePushDelay = false;
        [SerializeField] internal bool m_blockPushInput;

        internal int m_receivedPlayerId;
        private readonly float m_baseRotationSpeed = 100;
        private float m_maxSideMovement;
        private float m_paddleWidthAdjustment;
        private Quaternion m_paddleStartRotation;
        internal Vector2 m_sideMoveVector;
        private Vector3 m_moveVector;
        internal Vector2 m_axisRotation;
        private Vector3 m_rotateVector;
        private Quaternion m_deltaRotation;
        private Vector3 m_rbPosition;
        private IEnumerator m_paddlePushCoroutineP1, m_paddlePushCoroutineP2, m_paddlePushCoroutineP3, m_paddlePushCoroutineP4;

        internal bool m_pushPlayer = false;
        internal bool m_tempBlocked = false;

        private void OnEnable()
        {
            SetPlayerOrientation(m_playerController.m_rigidbody.transform.position.z < 0);  //PlayerController gets Rb in 'Awake()'.
            m_paddlePushCoroutineP1 = PushPaddleP1(m_lerpDuration);
            //m_paddlePushCoroutineP2 = PushPaddleP2(m_lerpDuration);
            //m_paddlePushCoroutineP3 = PushPaddleP3(m_lerpDuration);
            //m_paddlePushCoroutineP4 = PushPaddleP4(m_lerpDuration);
            //TODO: Coroutines for other 3 Paddles have to get added, or pushInputs separated. (After some sleep... .)
        }

        private void OnDisable()
        {
            Ball.HitGoalOne -= LetsResetPaddleRotation;
            Ball.HitGoalTwo -= LetsResetPaddleRotation;

            StopPushCoroutine();
        }

        private void Start()
        {
            Ball.HitGoalOne += LetsResetPaddleRotation;
            Ball.HitGoalTwo += LetsResetPaddleRotation;

            ClampMoveRange();   //PlayerController gets Variables in (Awake()'.
        }

        private void Update()
        {
            ClampMoveRange();
            ClampRotationAngle();

            //TODO: MUST be removed after testing is completed!!!_______________________________
            //TODO: MatchManager shall pass PaddleWidthAdjustment-Changes after hitting objects to each individual player.
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (m_playerController.m_matchValues != null)
                {
                    //m_matchManager = FindObjectOfType<MatchManager>();
                    m_playerController.m_matchValues.PaddleWidthAdjustment += m_playerController.m_matchManager.PaddleWidthAdjustStep;
                }
            }

            if (Keyboard.current.pKey.wasReleasedThisFrame)
            {
                if (m_playerController.m_matchValues != null)
                {
                    //m_matchManager = FindObjectOfType<MatchManager>();
                    m_playerController.m_matchValues.PaddleWidthAdjustment -= m_playerController.m_matchManager.PaddleWidthAdjustStep;
                }
            }
            //__________________________________________________________________________________
        }

        private void FixedUpdate()
        {
            MovePaddle();
            RotatePaddle();
        }

        #region Custom Methods
        private void SetPlayerOrientation(bool _negPaddlePosZ)
        {
            switch (_negPaddlePosZ)
            {
                case true:
                {
                    m_playerController.transform.rotation = Quaternion.Euler(m_playerController.m_rigidbody.rotation.x, 0.0f, m_playerController.m_rigidbody.rotation.z);
                    m_playerController.m_rigidbody.rotation = Quaternion.Euler(m_playerController.m_rigidbody.rotation.x, 0.0f, m_playerController.m_rigidbody.rotation.z);
                    break;           //NegativeZPosition.
                }
                case false:
                {
                    m_playerController.transform.rotation = Quaternion.Euler(m_playerController.m_rigidbody.rotation.x, +180.0f, m_playerController.m_rigidbody.rotation.z);
                    m_playerController.m_rigidbody.rotation = Quaternion.Euler(m_playerController.m_rigidbody.rotation.x, +180.0f, m_playerController.m_rigidbody.rotation.z);
                    break;          //PositiveZPosition.
                }
            }

            m_paddleStartRotation = m_playerController.m_rigidbody.rotation;
            m_playerController.m_rigidbody.transform.localRotation = m_paddleStartRotation;
        }

        private void MovePaddle()
        {
            var xInvert = m_playerController.m_controlUIStates.InvertXAxis ? -1 : 1;   //ReadValue.x * (m_controlUIStates.InvertXAxis ? -1 : 1)
            switch (m_playerController.m_playerIDData.PlayerId)
            {
                case 0:
                {
                    m_rbPosition = m_playerController.m_rigidbody.transform.localPosition;
                    m_moveVector = m_movementSpeed * Time.fixedDeltaTime * new Vector3(m_sideMoveVector.x * xInvert, 0, m_sideMoveVector.y).normalized;   //Player1 ID
                    m_rotateVector = new Vector3(0.0f, m_axisRotation.x, 0.0f);
                    break;
                }
                case 1:
                {
                    m_rbPosition = -m_playerController.m_rigidbody.transform.localPosition;
                    m_moveVector = m_movementSpeed * Time.fixedDeltaTime * -new Vector3(m_sideMoveVector.x * xInvert, 0, m_sideMoveVector.y).normalized;   //Player2 ID
                    m_rotateVector = new Vector3(0.0f, m_axisRotation.x, 0.0f);
                    break;
                }
                case 2:
                {
                    m_rbPosition = m_playerController.m_rigidbody.transform.localPosition;
                    m_moveVector = m_movementSpeed * Time.fixedDeltaTime * new Vector3(m_sideMoveVector.x * xInvert, 0, m_sideMoveVector.y).normalized;   //Player3 ID
                    m_rotateVector = new Vector3(0.0f, m_axisRotation.x, 0.0f);
                    break;
                }
                case 3:
                {
                    m_rbPosition = -m_playerController.m_rigidbody.transform.localPosition;
                    m_moveVector = m_movementSpeed * Time.fixedDeltaTime * -new Vector3(m_sideMoveVector.x * xInvert, 0, m_sideMoveVector.y).normalized;   //Player4 ID
                    m_rotateVector = new Vector3(0.0f, m_axisRotation.x, 0.0f);
                    break;
                }
                default:
                    break;
            }

            m_playerController.m_rigidbody.MovePosition(m_rbPosition + m_moveVector);
        }

        private void RotatePaddle()
        {
            var yInvert = m_playerController.m_controlUIStates.InvertYAxis ? -1 : 1;
            m_deltaRotation = Quaternion.Euler(m_baseRotationSpeed * m_rotationSpeed * Time.fixedDeltaTime * (m_rotateVector * yInvert)).normalized;
            m_playerController.m_rigidbody.MoveRotation(m_playerController.m_rigidbody.rotation * m_deltaRotation);
        }

        /// <summary>
        /// Clamps the paddlemMovement on it's 'localPosition.x' and the calculated movementRange based on paddleWidth and fieldWidth.
        /// Also clamps the desired minimal and maximal moveDistance on the zAxis based on m_goalDistance and m_maxPushDistance to the playerGoals.
        /// </summary>
        public void ClampMoveRange()
        {
            m_playerController.m_rigidbody.transform.localPosition = new Vector3(m_playerController.m_rigidbody.transform.localPosition.x, m_playerController.m_rigidbody.transform.localPosition.y, -m_playerController.m_groundLength * 0.5f - -m_playerController.m_goalDistance);

            m_maxSideMovement = m_playerController.m_groundWidth * 0.5f - m_playerController.m_rigidbody.transform.localScale.x * 0.5f;

            if (m_playerController.m_matchValues == null)
                m_paddleWidthAdjustment = 0;
            else
                m_paddleWidthAdjustment = m_playerController.m_matchValues.PaddleWidthAdjustment;

            m_playerController.m_rigidbody.transform.localScale = new Vector3(m_playerController.m_localPaddleScale.x + m_paddleWidthAdjustment, m_playerController.m_localPaddleScale.y, m_playerController.m_localPaddleScale.z);

            m_playerController.m_rigidbody.transform.localPosition = new Vector3(Mathf.Clamp(m_playerController.m_rigidbody.transform.localPosition.x, -m_maxSideMovement, m_maxSideMovement),
                m_playerController.m_rigidbody.transform.localPosition.y,
                Mathf.Clamp(m_playerController.m_rigidbody.transform.localPosition.z, -m_playerController.m_groundLength * 0.5f - -m_playerController.m_goalDistance, -m_playerController.m_groundLength * 0.5f - -(m_playerController.m_goalDistance + m_playerController.m_maxPushDistance)));
        }

        /// <summary>
        /// Clamps the maximal rotationAngle based on 'Quaternion.LookRotation, rigidbody's forwardVector and Vector3.up'.
        /// </summary>
        private void ClampRotationAngle()
        {
            Quaternion rotation = Quaternion.LookRotation(m_playerController.m_rigidbody.transform.forward, Vector3.up);
            rotation.ToAngleAxis(out float angle, out Vector3 axis);
            angle = Mathf.Clamp(angle, -m_maxRotationAngle, m_maxRotationAngle);
            m_playerController.m_rigidbody/*.transform*/.rotation = Quaternion.AngleAxis(angle, axis);
        }

        private void LetsResetPaddleRotation()
        {
            #region Saved for a potential Coroutine.
            //float currentTime = 0;
            //float endValue = 0;

            //Quaternion currentValue =
            //    Quaternion.Euler(m_rigidbody.transform.localRotation.x, m_rigidbody.transform.localRotation.y, m_rigidbody.transform.localRotation.z);
            //Quaternion targetValue = Quaternion.Euler(m_rigidbody.transform.localRotation.x, endValue, m_rigidbody.transform.localRotation.z);

            //while (currentTime < m_lerpDuration)
            //{
            //    m_rigidbody.transform.rotation =
            //        Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(m_rigidbody.transform.rotation.x, 0, m_rigidbody.transform.rotation.z), currentTime);
            //    currentTime += Time.deltaTime * m_lerpDuration;
            //}

            //m_rigidbody.transform.localRotation = targetValue;
            #endregion
            switch (m_playerController.m_matchUIStates.RotationReset)
            {
                case true:
                    m_playerController.m_rigidbody.transform.localRotation = m_paddleStartRotation;
                    break;
                case false:
                    break;
            }
        }

        internal void ReStartPushCoroutine()
        {
            StartCoroutine(m_paddlePushCoroutineP1);
            //StartCoroutine(m_paddlePushCoroutineP2);
            //StartCoroutine(m_paddlePushCoroutineP3);
            //StartCoroutine(m_paddlePushCoroutineP4);
        }

        internal void StopPushCoroutine()
        {
            StopAllCoroutines();
        }
        #endregion

        #region IEnumerators
        /// <summary>
        /// MoveForthAndBack handles forward- and backwardPushes of the playerPaddle.
        /// '_lerpDuration' also works as maximal Time in the whileLoop, but could be replaced with a fix floatAmount.
        /// <param name="_lerpDuration"></param>
        /// <returns></returns>
        private IEnumerator PushPaddleP1(float _lerpDuration)
        {
            float currentTime = 0;

            while (currentTime < _lerpDuration)
            {
                if (m_pushPlayer)
                {
                    currentTime += Time.deltaTime;

                    #region zFloat Mathf.MoveTowards
                    //if (m_playerId == 0)  //TODO: Updating Id on each Coroutine all 4 Paddles.
                    //{
                    float startZPos = m_playerController.m_rigidbody.transform.localPosition.z;
                    float endZPos = m_playerController.m_rigidbody.transform.localPosition.z - -m_playerController.m_maxPushDistance;

                    m_playerController.m_rigidbody.transform.localPosition = new Vector3(m_playerController.m_rigidbody.transform.localPosition.x, m_playerController.m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _lerpDuration)) + m_playerController.m_rigidbody.transform.forward;
                    m_playerController.m_rigidbody.transform.localPosition = new Vector3(m_playerController.m_rigidbody.transform.localPosition.x, m_playerController.m_rigidbody.transform.localPosition.y, endZPos);

                    yield return new WaitForSeconds(m_delayRetreat);
                    m_pushPlayer = false;

                    m_playerController.m_rigidbody.transform.localPosition = new Vector3(m_playerController.m_rigidbody.transform.localPosition.x, m_playerController.m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _lerpDuration)) + -m_playerController.m_rigidbody.transform.forward;
                    m_playerController.m_rigidbody.transform.localPosition = new Vector3(m_playerController.m_rigidbody.transform.localPosition.x, m_playerController.m_rigidbody.transform.localPosition.y, startZPos);

                    #region Nested Coroutine
                    //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                    if (m_enablePushDelay)
                    {
                        m_tempBlocked = true;
                        Coroutine pushRestriction = StartCoroutine(RestrictPushP1());
                        yield return pushRestriction;
                        m_tempBlocked = false;
                    }
                    #endregion
                    //}
                    #endregion
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator RestrictPushP1()
        {
            float countdown = m_delayRepetition;

            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                yield return null;
            }
        }
        #endregion
    }
}