using System.Collections;
using ThreeDeePongProto.Offline.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Offline.Player.Inputs
{
    public class PlayerControlSubPosZ : PlayerControlMain
    {
        private IEnumerator m_paddleTwoPushCoroutine, m_paddleFourPushCoroutine;
        protected bool m_pushPlayer2 = false, m_pushPlayer4 = false;
        private Vector3 m_axisRotPosZ;

        protected override void Awake()
        {
            if (m_rigidbody == null)
            {
                m_rigidbody = GetComponentInChildren<Rigidbody>();
            }

            m_playerIDData.PlayerId = m_playerId;
            m_paddleTwoPushCoroutine = PushPaddleTwo(m_lerpDuration);
            m_paddleFourPushCoroutine = PushPaddleFour(m_lerpDuration);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopAllCoroutines();
            InGameMenuOpens -= DisablePlayerActions;
            MenuOrganisation.CloseInGameMenu -= StartCoroutinesAndActions;
        }

        protected override void Start()
        {
            base.Start();
            m_playerMovement.PlayerActions.PushMovePosZP2.performed += PushInputPlayerTwo;
            m_playerMovement.PlayerActions.PushMovePosZP2.canceled += CanceledInputPlayerTwo;
            m_playerMovement.PlayerActions.PushMovePosZP4.performed += PushInputPlayerFour;
            m_playerMovement.PlayerActions.PushMovePosZP4.canceled += CanceledInputPlayerFour;

            InGameMenuOpens += DisablePlayerActions;
            MenuOrganisation.CloseInGameMenu += StartCoroutinesAndActions;
            StartCoroutinesAndActions();
        }

        protected override void Update()
        {
            base.Update();

            switch (m_playerIDData.PlayerId)
            {
                case 1:
                    m_axisRotPosZ = new Vector3(0, m_playerMovement.PlayerActions.TurnMovePosZP2.ReadValue<Vector2>().x, 0);  //Player2 ID = 1.
                    break;
                case 3:
                    m_axisRotPosZ = new Vector3(0, m_playerMovement.PlayerActions.TurnMovePosZP4.ReadValue<Vector2>().x, 0);  //Player4 ID = 3.
                    break;
                default:
                    m_axisRotPosZ = new Vector3(0, m_playerMovement.PlayerActions.TurnMovePosZP2.ReadValue<Vector2>().x, 0);  //Player2 ID = 1.
                    break;
            }
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
        protected virtual void MovePaddleByPlayer()
        {
            m_rbPosition = -m_rigidbody.transform.localPosition;
            switch (m_playerIDData.PlayerId)
            {
                case 1:
                {
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * -new Vector3(m_playerMovement.PlayerActions.SideMovePosZP2.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovePosZP2.ReadValue<Vector2>().y);  //Player2 ID
                    break;
                }
                case 3:
                {
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * -new Vector3(m_playerMovement.PlayerActions.SideMovePosZP4.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovePosZP4.ReadValue<Vector2>().y);  //Player4 ID
                    break;
                }
                default:
                    m_readValueVector = m_movementSpeed * Time.fixedDeltaTime * -new Vector3(m_playerMovement.PlayerActions.SideMovePosZP2.ReadValue<Vector2>().x, 0, m_playerMovement.PlayerActions.SideMovePosZP2.ReadValue<Vector2>().y);  //Player2 ID
                    break;
            }

            m_rigidbody.MovePosition(m_rbPosition + m_readValueVector);
        }

        /// <summary>
        /// Turns the playerPaddle based on Quaternion and 'rigidbody.rotation'.
        /// </summary>
        private void TurnPaddleByPlayer()
        {
            m_deltaRotation = Quaternion.Euler(m_axisRotPosZ * m_rotationSpeed * m_baseRotationSpeed * Time.fixedDeltaTime);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * m_deltaRotation);
        }

        protected override void StartCoroutinesAndActions()
        {
            //Use BaseClass code and return to do the rest here.
            base.StartCoroutinesAndActions();

            if (m_paddleTwoPushCoroutine != null)
                StartCoroutine(m_paddleTwoPushCoroutine);
            if (m_paddleFourPushCoroutine != null)
                StartCoroutine(m_paddleFourPushCoroutine);
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
        private IEnumerator PushPaddleTwo(float _lerpDuration)
        {
            float currentTime = 0;

            while (currentTime < _lerpDuration)
            {
                if (m_pushPlayer2)
                {
                    currentTime += Time.deltaTime;

                    #region zFloat Mathf.MoveTowards
                    if (m_playerId == 1)
                    {
                        float startZPos = m_rigidbody.transform.localPosition.z;
                        float endZPos = m_rigidbody.transform.localPosition.z - -m_maxPushDistance;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _lerpDuration)) + m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                        yield return new WaitForSeconds(m_delayRetreat);
                        m_pushPlayer2 = false;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _lerpDuration)) + -m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);

                        #region Nested Coroutine
                        //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                        if (m_enablePushDelay)
                        {
                            m_tempBlocked = true;
                            Coroutine pushRestriction = StartCoroutine(RestrictPushPTwo());
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

        /// <summary>
        /// MoveForthAndBack handles forward- and backwardPushes of the playerPaddle.
        /// '_lerpDuration' also works as maximal Time in the whileLoop, but could be replaced with a fix floatAmount.
        /// <param name="_lerpDuration"></param>
        /// <returns></returns>
        private IEnumerator PushPaddleFour(float _lerpDuration)
        {
            float currentTime = 0;

            while (currentTime < _lerpDuration)
            {
                if (m_pushPlayer4)
                {
                    currentTime += Time.deltaTime;

                    #region zFloat Mathf.MoveTowards
                    if (m_playerId == 3)
                    {
                        float startZPos = m_rigidbody.transform.localPosition.z;
                        float endZPos = m_rigidbody.transform.localPosition.z - -m_maxPushDistance;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos = Mathf.MoveTowards(startZPos, endZPos, _lerpDuration)) + m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, endZPos);

                        yield return new WaitForSeconds(m_delayRetreat);
                        m_pushPlayer4 = false;

                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos = Mathf.MoveTowards(endZPos, startZPos, _lerpDuration)) + -m_rigidbody.transform.forward;
                        m_rigidbody.transform.localPosition = new Vector3(m_rigidbody.transform.localPosition.x, m_rigidbody.transform.localPosition.y, startZPos);

                        #region Nested Coroutine
                        //Coroutine to restrict paddleForwardMovement by a certain amount of time.
                        if (m_enablePushDelay)
                        {
                            m_tempBlocked = true;
                            Coroutine pushRestriction = StartCoroutine(RestrictPushPFour());
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

        private IEnumerator RestrictPushPTwo()
        {
            float countdown = m_delayRepetition;

            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator RestrictPushPFour()
        {
            float countdown = m_delayRepetition;

            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                yield return null;
            }
        }
        #endregion

        #region Player CallbackContexts
        private void PushInputPlayerTwo(InputAction.CallbackContext _callbackContext)
        {
            if (!m_blockPushInput)
            {
                if (!m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_pushPlayer2 = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void PushInputPlayerFour(InputAction.CallbackContext _callbackContext)
        {
            if (!m_blockPushInput)
            {
                if (!m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_pushPlayer4 = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void CanceledInputPlayerTwo(InputAction.CallbackContext _callbackContext)
        {
            m_pushPlayer2 = false;
        }

        private void CanceledInputPlayerFour(InputAction.CallbackContext _callbackContext)
        {
            m_pushPlayer4 = false;
        }
        #endregion
    }
}