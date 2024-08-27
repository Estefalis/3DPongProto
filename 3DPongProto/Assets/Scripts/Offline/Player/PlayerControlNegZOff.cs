using System;
using System.Collections;
using ThreeDeePongProto.Offline.AudioManagement;
using ThreeDeePongProto.Offline.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Offline.Player.Inputs
{
    public class PlayerControlNegZOff : PlayerControlMain
    {
        private IEnumerator m_paddleOnePushCoroutine, m_paddleThreePushCoroutine;
        private bool m_pushPlayerOne = false, m_pushPlayerThree = false;
        private Vector3 m_axisRotNegZ;
        private Quaternion m_paddleStartRotation;

        protected override void Awake()
        {
            if (m_rigidbody == null)
            {
                m_rigidbody = GetComponentInChildren<Rigidbody>();
            }

            m_paddleStartRotation = m_rigidbody.transform.localRotation;
            m_playerIDData.PlayerId = m_playerId;
            m_paddleOnePushCoroutine = PushPaddleOne(m_lerpDuration);
            m_paddleThreePushCoroutine = PushPaddleThree(m_lerpDuration);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Ball.HitGoalOne += LetsResetPaddleRotation;
            Ball.HitGoalTwo += LetsResetPaddleRotation;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopAllCoroutines();
            InGameMenuOpens -= DisablePlayerActions;
            MenuNavigation.CloseInGameMenu -= StartCoroutinesAndActions;
            Ball.HitGoalOne -= LetsResetPaddleRotation;
            Ball.HitGoalTwo -= LetsResetPaddleRotation;
        }

        protected override void Start()
        {
            base.Start();
            m_playerMovement.PlayerActions.PushPaddleNegZP1.performed += PushInputPlayerOne;
            m_playerMovement.PlayerActions.PushPaddleNegZP1.canceled += CanceledInputPlayerOne;
            m_playerMovement.PlayerActions.PushPaddleNegZP3.performed += PushInputPlayerThree;
            m_playerMovement.PlayerActions.PushPaddleNegZP3.canceled += CanceledInputPlayerThree;

            InGameMenuOpens += DisablePlayerActions;
            MenuNavigation.CloseInGameMenu += StartCoroutinesAndActions;
            StartCoroutinesAndActions();

            AudioManager.LetsRegisterAudioSources(m_audioSource);
        }

        protected override void Update()
        {
            base.Update();

            m_axisRotNegZ = m_playerIDData.PlayerId switch
            {
                0 => new Vector3(0, m_playerMovement.PlayerActions.RotatePaddleNegZP1.ReadValue<Vector2>().x, 0),//Player1 ID = 0.
                2 => new Vector3(0, m_playerMovement.PlayerActions.RotatePaddleNegZP3.ReadValue<Vector2>().x, 0),//Player3 ID = 2.
                _ => new Vector3(0, m_playerMovement.PlayerActions.RotatePaddleNegZP1.ReadValue<Vector2>().x, 0),//Player1 ID = 0.
            };
        }

        protected override void FixedUpdate()
        {
            MovePaddleByPlayer();
            TurnPaddleByPlayer();
        }

        /// <summary>
        /// MovePaddleByPlayer requires a switch to handle the positive and negative paddlePositions and moveDirections based on a playerID.
        /// </summary>
        /// <param name="playerID"></param>
        private void MovePaddleByPlayer()
        {
            m_rbPosition = m_rigidbody.transform.localPosition;
            var xInvert = m_controlUIStates.InvertXAxis ? -1 : 1;   //Also possible: ReadValue.x * (m_controlUIStates.InvertXAxis ? -1 : 1)

            switch (m_playerIDData.PlayerId)
            {
                case 0:
                {
                    //var choose = object (bool b) => b ? 1 : "two"; // Func<bool, object>
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * new Vector3(m_playerMovement.PlayerActions.SideMovementNegZP1.ReadValue<Vector2>().x * xInvert, 0, m_playerMovement.PlayerActions.SideMovementNegZP1.ReadValue<Vector2>().y).normalized;   //Player1 ID
                    break;
                }
                case 2:
                {
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * new Vector3(m_playerMovement.PlayerActions.SideMovementNegZP3.ReadValue<Vector2>().x * xInvert, 0, m_playerMovement.PlayerActions.SideMovementNegZP3.ReadValue<Vector2>().y).normalized;   //Player3 ID
                    break;
                }
                default:
                {
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * new Vector3(m_playerMovement.PlayerActions.SideMovementNegZP1.ReadValue<Vector2>().x * xInvert, 0, m_playerMovement.PlayerActions.SideMovementNegZP1.ReadValue<Vector2>().y).normalized;   //Player1 ID
                    break;
                }
            }

            m_rigidbody.MovePosition(m_rbPosition + m_readValueVector);
        }

        /// <summary>
        /// Turns the playerPaddle based on Quaternion and 'rigidbody.rotation'.
        /// </summary>
        private void TurnPaddleByPlayer()
        {
            var yInvert = m_controlUIStates.InvertYAxis ? -1 : 1;
            m_deltaRotation = Quaternion.Euler(m_baseRotationSpeed * m_rotationSpeed * Time.fixedDeltaTime * (m_axisRotNegZ * yInvert)).normalized;
            m_rigidbody.MoveRotation(m_rigidbody.rotation * m_deltaRotation);
        }

        protected override void StartCoroutinesAndActions()
        {
            //Use BaseClass code and return to do the rest here.
            base.StartCoroutinesAndActions();

            if (m_paddleOnePushCoroutine != null)
                StartCoroutine(m_paddleOnePushCoroutine);
            if (m_paddleThreePushCoroutine != null)
                StartCoroutine(m_paddleThreePushCoroutine);
        }

        protected override void DisablePlayerActions()
        {
            //Use BaseClass code and return to do the rest here.
            base.DisablePlayerActions();
            StopAllCoroutines();
        }

        #region IEnumerators - Coroutines
        /// <summary>
        /// MoveForthAndBack handles forward- and backwardPushes of the playerPaddle.
        /// '_lerpDuration' also works as maximal Time in the whileLoop, but could be replaced with a fix floatAmount.
        /// <param name="_lerpDuration"></param>
        /// <returns></returns>
        private IEnumerator PushPaddleOne(float _lerpDuration)
        {
            float currentTime = 0;

            while (currentTime < _lerpDuration)
            {
                if (m_pushPlayerOne)
                {
                    currentTime += Time.deltaTime;

                    #region zFloat Mathf.MoveTowards
                    if (m_playerId == 0)
                    {
                        float startZPos = m_rigidbody.transform.localPosition.z;
                        float endZPos = m_rigidbody.transform.localPosition.z - -m_maxPushDistance;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _lerpDuration)) + m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                        yield return new WaitForSeconds(m_delayRetreat);
                        m_pushPlayerOne = false;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _lerpDuration)) + -m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);

                        #region Nested Coroutine
                        //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                        if (m_enablePushDelay)
                        {
                            m_tempBlocked = true;
                            Coroutine pushRestriction = StartCoroutine(RestrictPushPOne());
                            yield return pushRestriction;
                            m_tempBlocked = false;
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator RestrictPushPOne()
        {
            float countdown = m_delayRepetition;

            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// MoveForthAndBack handles forward- and backwardPushes of the playerPaddle.
        /// '_lerpDuration' also works as maximal Time in the whileLoop, but could be replaced with a fix floatAmount.
        /// <param name="_lerpDuration"></param>
        /// <returns></returns>
        private IEnumerator PushPaddleThree(float _lerpDuration)
        {
            float currentTime = 0;

            while (currentTime < _lerpDuration)
            {
                if (m_pushPlayerThree)
                {
                    currentTime += Time.deltaTime;

                    #region zFloat Mathf.MoveTowards
                    if (m_playerId == 2)
                    {
                        float startZPos = m_rigidbody.transform.localPosition.z;
                        float endZPos = m_rigidbody.transform.localPosition.z - -m_maxPushDistance;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _lerpDuration)) + m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                        yield return new WaitForSeconds(m_delayRetreat);
                        m_pushPlayerThree = false;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _lerpDuration)) + -m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);

                        #region Nested Coroutine
                        //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                        if (m_enablePushDelay)
                        {
                            m_tempBlocked = true;
                            Coroutine pushRestriction = StartCoroutine(RestrictPushPThree());
                            yield return pushRestriction;
                            m_tempBlocked = false;
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator RestrictPushPThree()
        {
            float countdown = m_delayRepetition;

            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                yield return null;
            }
        }

        private void LetsResetPaddleRotation()
        {
            //TODO: Ggf. eine public Methode hinzufuegen, damit man die PaddleRotation manuell reseten kann.
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
            switch (m_matchUIStates.TpOneRotReset)
            {
                case true:
                    m_rigidbody.transform.localRotation = m_paddleStartRotation;
                    break;
                case false:
                    break;
            }
        }
        #endregion

        #region Player CallbackContexts
        private void PushInputPlayerOne(InputAction.CallbackContext _callbackContext)
        {
            if (!m_blockPushInput)
            {
                if (!m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_pushPlayerOne = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void PushInputPlayerThree(InputAction.CallbackContext _callbackContext)
        {
            if (!m_blockPushInput)
            {
                if (!m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_pushPlayerThree = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void CanceledInputPlayerOne(InputAction.CallbackContext _callbackContext)
        {
            m_pushPlayerOne = false;
        }

        private void CanceledInputPlayerThree(InputAction.CallbackContext _callbackContext)
        {
            m_pushPlayerThree = false;
        }
        #endregion
    }
}