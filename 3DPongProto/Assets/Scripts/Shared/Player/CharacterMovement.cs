using System;
using System.Collections;
using ThreeDeePongProto.Shared.AudioManagement;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterMovement : MonoBehaviour, IProvidePlayerID
    {
        private enum ELerpCategory
        {
            None = 0,
            FloatZ = 1,
            FloatXAndZ = 2,
            Vector3 = 3,
        }

        [SerializeField] protected AudioSource m_audioSource;
        [SerializeField] private Transform m_pushTarget;
        [SerializeField] private ELerpCategory m_elerpCategory = ELerpCategory.FloatZ;

        [Header("Rotation")]
        [SerializeField] private float m_maxRotationAngle = 45.0f; //Maximum angle (�45 degrees).

        [Header("Push")]
        [SerializeField, Range(0.1f, 20.0f)] private float m_pushSpeed = 10.0f;
        [SerializeField, Range(0.1f, 20.0f)] private float m_retreatSpeed = 12.5f;
        [SerializeField, Range(0.5f, 2.0f)] private float m_pushDuration = 1.0f;
        private float m_xPosDifference;
        private bool m_isPushing, m_valueTaken = false;

        internal uint m_receivedUserID;
        internal int m_playerID;

        //Movement
        private Vector3 m_moveVector;
        private Vector3 m_rbPushStartPos;
        private float m_maxSideMovement;
        private float m_maxPushDistance;

        //Rotation
        private Rigidbody m_rigidbody;
        private float m_currentYRotationOffset = 0f;
        private readonly float m_baseRotationSpeed = 100.0f;
        private float m_moveSpeedX, m_rotationSpeedY;
        private bool m_invertXAxis, m_invertYAxis;
        internal Vector2 m_rotationVector;
        #region Old version with deltaRotation
        private Quaternion m_deltaRotation;
        #endregion
        private Quaternion m_initialRbRotation;

        //Positioning & Scale
        private EPlayerLine m_linePosition;     //Option for potential resets or movment-limitation on Z-axis.
        // private float m_goalLineDistance;
        private Vector3 m_paddleScale;

        private CharacterMainController m_characterController;
        private MatchSettingsData m_matchSettingsData;

        private void Awake()
        {
            m_matchSettingsData = SettingsManager.Instance.CurrentSettings.Match;

            m_rigidbody = GetComponentInChildren<Rigidbody>();
            m_rigidbody.transform.rotation = Quaternion.Euler(Vector3.zero);

            m_characterController = GetComponentInParent<CharacterMainController>();
            if (m_characterController == null)
            {
                Debug.LogError("CharacterMovement could not find CharacterMainController!", this);
            }
            else
            {
                m_characterController.m_playerCameraController.m_rigidbody = m_rigidbody;
            }
        }

        private void OnEnable()
        {
            m_isPushing = false;

            Ball.OnHitGoalOne += ResetPlayerRotationOnGoal;
            Ball.OnHitGoalTwo += ResetPlayerRotationOnGoal;
        }

        private void OnDisable()
        {
            if (m_audioSource != null)
                AudioManager.LetsRemoveAudioSources(m_audioSource);

            Ball.OnHitGoalOne -= ResetPlayerRotationOnGoal;
            Ball.OnHitGoalTwo -= ResetPlayerRotationOnGoal;
        }

        private void Start()
        {
            m_paddleScale = LocalMatchManager.DEFAULT_PADDLE_SCALE;
            m_maxPushDistance = LocalMatchManager.Instance.MaxPushDistance;

            if (m_audioSource != null)
                AudioManager.LetsRegisterAudioSources(m_audioSource);
        }

        private void LateUpdate()   //Either in Update() or LateUpdate()!
        {
            ClampMoveRange();
        }

        private void FixedUpdate()
        {
            switch (m_isPushing)
            {
                case true:
                    StartCoroutine(HandlePush());
                    break;
                case false:
                {
                    break;
                }
            }

            HandleMovement();
            HandleRotation();
        }

        /// <summary>
        /// Receives all relevant data from the CharacterController-Parent object.
        /// </summary>
        /// <param name="_controlData"></param>
        /// <param name="_playerProfile"></param>
        internal void Initialize(ControlSettingsData _controlData, MatchSettingsData _matchData, PlayerProfileData _playerProfile)
        {
            m_playerID = _playerProfile.PlayerID;
            // m_linePosition = _matchData.PlayerPositions[m_playerID];
            // m_goalLineDistance = (m_linePosition == EPlayerLine.Frontline) ? _matchData.FrontlineDistance : _matchData.BacklineDistance;

            m_moveSpeedX = _controlData.MoveSpeedX;
            m_rotationSpeedY = _controlData.RotSpeedY;
            m_invertXAxis = _playerProfile.InvertMoveAxisX;
            m_invertYAxis = _playerProfile.InvertRotAxisY;

            SetPositionAndRotation(m_playerID);
        }

        #region Movement
        internal void SetInputVector(Vector2 _inputVector, int _receivedID, bool _isRotation)
        {
            if (_receivedID != m_playerID)
                return;

            switch (_isRotation)
            {
                case true:
                    m_rotationVector = _inputVector;
                    break;
                case false:
                    m_moveVector = _inputVector;
                    break;
            }
        }

        private void HandleMovement()
        {
            float xInvert = m_invertXAxis ? -1f : 1f;

            //Move the player relativ to the local Transform-Direction.
            Vector3 rightMovement = m_moveVector.x * xInvert * transform.right;
            Vector3 forwardMovement = m_moveVector.y * m_rigidbody.transform.forward;

            //Combined Movement.
            Vector3 adjustedMoveVector = (rightMovement + forwardMovement).normalized * (m_moveSpeedX * Time.fixedDeltaTime);
            m_rigidbody.MovePosition(m_rigidbody.transform.position + adjustedMoveVector);
        }

        /// <summary>
        /// Clamps the paddlemMovement on it's 'localPosition.x' and the calculated movementRange based on paddleWidth and fieldWidth.
        /// Can also be used to clamp playerMovement on Z-axis, if required.
        /// </summary>
        public void ClampMoveRange()
        {
            m_rigidbody.transform.localScale = new Vector3(
                m_paddleScale.x,
                m_paddleScale.y,
                m_paddleScale.z
            );

            //Calculate MoveRange.
            m_maxSideMovement = m_matchSettingsData.FieldWidth * 0.5f - m_rigidbody.transform.localScale.x * 0.5f;

            //Clamp Rigidbody moveRange on X-Axis.
            m_rigidbody.transform.localPosition = new Vector3(
                Mathf.Clamp(m_rigidbody.transform.localPosition.x, -m_maxSideMovement, m_maxSideMovement),
                m_rigidbody.transform.localPosition.y,
                m_rigidbody.transform.localPosition.z
            );
        }
        #endregion

        #region Rotation
        private void SetPositionAndRotation(int _playerIndex)
        {
            EPlayerLine line = m_matchSettingsData.PlayerPositions[_playerIndex];
            float zSide = (_playerIndex % 2 == 0) ? -1f : 1f;
            float halfFieldLength = m_matchSettingsData.FieldLength / 2f;
            float lineDistance = (line == EPlayerLine.Backline) ? m_matchSettingsData.BacklineDistance : m_matchSettingsData.FrontlineDistance;
            float zPos = zSide * (halfFieldLength - lineDistance); //Sets distance of players X-MoveLine.

            Vector3 spawnPosition = new(0, transform.position.y + 0.5f, zPos);
            //Quaternion spawnRotation = Quaternion.LookRotation(new Vector3(0, 0, -zSide));
            Quaternion spawnRotation = Quaternion.Euler(0, zSide > 0 ? 180f : 0f, 0);

            //PlayerBase position and rotation. (PlayerMainController pops in first, CharacterController second.)
            if (m_characterController != null)
                m_characterController.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            
            m_rigidbody.transform.localRotation = spawnRotation;
            // m_rigidbody.transform.parent.localRotation = spawnRotation;

            var adjustedPushTarget = spawnRotation.y != 0 ? m_rigidbody.transform.position.z - m_maxPushDistance : m_rigidbody.transform.position.z + m_maxPushDistance;
            //Sets PushTarget Position with 'm_maxPushDistance'.
            m_pushTarget.transform.position = new Vector3(0.0f, m_pushTarget.transform.position.y, adjustedPushTarget);
        }

        private void HandleRotation()
        {
            float yInvert = m_invertYAxis ? -1.0f : 1.0f;
            float angleChange = m_rotationVector.x * yInvert * m_rotationSpeedY * m_baseRotationSpeed * Time.fixedDeltaTime;

            //Addiere the change to the current Offset.
            m_currentYRotationOffset += angleChange;

            //Clamp the total Offset to the max angle.
            m_currentYRotationOffset = Mathf.Clamp(m_currentYRotationOffset, -m_maxRotationAngle, m_maxRotationAngle);

            //Calculate the new final worldRotation.
            Quaternion newWorldRotation = m_initialRbRotation * Quaternion.Euler(0, m_currentYRotationOffset, 0);

            //Apply the new rotation through the Physics-Engine.
            m_rigidbody.MoveRotation(newWorldRotation.normalized);

            #region Old version with deltaRotation
            //float yInvert = m_invertYAxis ? -1f : 1f;
            //Vector3 rotationVector = new(0.0f, m_rotationVector.x, 0.0f); //'Quaternion.Euler' requires a Vector3 for multiplication.

            ////Calculate rotation based on input.
            //m_deltaRotation = Quaternion.Euler(m_baseRotationSpeed * m_rotationSpeedY * Time.fixedDeltaTime * (rotationVector * yInvert)).normalized;

            ////Apply rotation.
            //Quaternion newRotation = m_rigidbody.rotation * m_deltaRotation;

            ////Clamp rotationAngle on maxValue(s).
            //newRotation.ToAngleAxis(out float angle, out Vector3 axis);
            //angle = Mathf.Clamp(angle, -m_maxRotationAngle, m_maxRotationAngle);

            //m_rigidbody.MoveRotation(Quaternion.AngleAxis(angle, axis));
            #endregion
        }
        #endregion

        #region Push
        internal void InitializePush(int _receivedID, bool _initializePush)
        {
            if (_receivedID != m_playerID)
                return;

            if (_initializePush && !m_isPushing)
            {
                //Start pushing.
                m_isPushing = true;
            }
        }

        private IEnumerator HandlePush()
        {
            if (m_receivedUserID != m_characterController.m_playerInputHandler.m_playerInput.user.id || m_valueTaken)
            yield break;
        
            Vector3 rbStartPosition = m_rigidbody.transform.position;
            m_rbPushStartPos = rbStartPosition;

            Vector3 pushTargetPosition = m_pushTarget.transform.position;
            m_xPosDifference = pushTargetPosition.x - rbStartPosition.x;

            m_valueTaken = true;
            float progress = 0.0f;

            while (progress < m_maxPushDistance)
            {
                progress += Time.fixedDeltaTime * m_pushSpeed / m_pushDuration;

                switch (m_elerpCategory)
                {
                    case ELerpCategory.FloatZ:
                    {
                        float newZ = Mathf.Lerp(rbStartPosition.z, pushTargetPosition.z, progress);

                        m_rigidbody.transform.position = new Vector3(rbStartPosition.x, rbStartPosition.y, newZ);
                        break;
                    }
                    case ELerpCategory.FloatXAndZ:
                    {
                        //float adjustedX = Mathf.Lerp(rbStartPosition.x, rbStartPosition.x + xDifference, progress);
                        float newX = Mathf.Lerp(rbStartPosition.x, Mathf.Lerp(rbStartPosition.x, rbStartPosition.x + m_xPosDifference, progress), progress);
                        float newZ = Mathf.Lerp(rbStartPosition.z, pushTargetPosition.z, progress);

                        m_rigidbody.transform.position = new Vector3(newX, rbStartPosition.y, newZ);
                        break;
                    }
                    case ELerpCategory.Vector3: //until further changes.
                    {
                        Vector3 pushVector = Vector3.Lerp(rbStartPosition, pushTargetPosition, progress);
                        m_rigidbody.transform.position = pushVector;
                        break;
                    }
                    case ELerpCategory.None:
                    default:
                        break;
                }

                yield return null;
            }

            StartCoroutine(HandleRetreat());
        }

        private IEnumerator HandleRetreat()
        {
            Vector3 rbStartPosition = m_rigidbody.transform.position;

            float progress = 0.0f;

            while (progress < m_maxPushDistance)
            {
                progress += Time.fixedDeltaTime * m_retreatSpeed / m_pushDuration;

                switch (m_elerpCategory)
                {
                    case ELerpCategory.FloatZ:
                    {
                        float newZ = Mathf.Lerp(rbStartPosition.z, m_rbPushStartPos.z, progress);

                        m_rigidbody.transform.position = new Vector3(rbStartPosition.x, rbStartPosition.y, newZ);
                        break;
                    }
                    case ELerpCategory.FloatXAndZ:
                    {
                        float newX = Mathf.Lerp(rbStartPosition.x, Mathf.Lerp(rbStartPosition.x, rbStartPosition.x - m_xPosDifference, progress), progress);
                        float newZ = Mathf.Lerp(rbStartPosition.z, m_rbPushStartPos.z, progress);

                        m_rigidbody.transform.position = new Vector3(newX, rbStartPosition.y, newZ);
                        break;
                    }
                    case ELerpCategory.Vector3: //until further changes.
                    {
                        Vector3 retreatVector = Vector3.Lerp(rbStartPosition, m_rbPushStartPos, progress);
                        m_rigidbody.transform.position = retreatVector;
                        break;
                    }
                    case ELerpCategory.None:
                    default:
                        break;
                }

                yield return null;
            }

            m_valueTaken = false;
            m_isPushing = false;
        }
        #endregion

        public void SetPlayerID(int _playerID)
        {
            m_playerID = _playerID;
            SetPlayerRotation();
        }

        /// <summary>
        /// Rotations of top parent Transform and Rigidbody need to be adjusted to move correct, depending on positive or negative Z-values.
        /// </summary>
        /// <param name="_playerId"></param>
        private void SetPlayerRotation()
        {
            m_initialRbRotation = m_rigidbody.transform.localRotation;  //rbLocalPos set by OnPlayerJoined in LocalMatchManager.
            gameObject.transform.position = m_characterController.transform.position;
            m_rigidbody.transform.SetPositionAndRotation(transform.position, m_initialRbRotation);
        }

        internal void ResetPlayerRotation(int _receivedID)
        {
            if (_receivedID != m_playerID)
                return;

            m_currentYRotationOffset = 0.0f;
            m_rigidbody.MoveRotation(m_initialRbRotation);      //m_rigidbody.transform.localRotation = m_initialRbRotation;
        }

        private void ResetPlayerRotationOnGoal()
        {
            switch (m_matchSettingsData.RotationReset)
            {
                case true:
                m_rigidbody.MoveRotation(m_initialRbRotation);  //Former m_rigidbody.transform.localRotation = m_initialRbRotation;
                break;
                case false:
                break;
            }
        }
    }
}