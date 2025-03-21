using System;
using System.Collections;
using ThreeDeePongProto.Shared.AudioManagement;
using UnityEngine;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private CharacterMainController m_playerController;
        [SerializeField] private Rigidbody m_rigidbody;
        [SerializeField] private Transform m_pushTarget;
        [SerializeField] protected AudioSource m_audioSource;

        [Header("Movement")]
        [SerializeField, Range(1.0f, 20.0f)] private float m_moveSpeed = 10.0f;
        private float m_maxSideMovement;
        private Vector3 m_moveVector;

        [Header("Rotation")]
        [SerializeField, Range(1.0f, 5.0f)] protected float m_rotationSpeed = 2.5f;
        [SerializeField] private float m_maxRotationAngle = 45.0f; //Maximum angle (±45 degrees).
        private readonly float m_baseRotationSpeed = 100.0f;
        internal Vector2 m_rotationVector;
        private Quaternion m_deltaRotation, m_resetRbRotation;

        [Header("Push")]
        [SerializeField, Range(1.0f, 20.0f)] private float m_pushSpeed = 10.0f;
        [SerializeField, Range(1.0f, 30.0f)] private float m_retreatSpeed = 15.0f;
        [SerializeField, Range(0.5f, 2.0f)] private float m_pushDuration = 1.0f;
        //[SerializeField] private float m_pushDistance = 1.5f;

        private bool m_isPushing;
        private float m_paddleWidthAdjustment;
        private float m_maxPushDistance;

        private float m_localRbTransformRotY;    //If the Rigidbody.localRotation is 180 instead of 0 then forwardDirection is inverted.
        private Vector3 m_localRbTransformPos;

        private void Awake()
        {
            if (m_rigidbody == null)
                m_rigidbody = GetComponent<Rigidbody>();

            SetPlayerRotation(m_playerController.m_playerId);

            m_isPushing = false;
        }

        private void OnDisable()
        {
            if (m_audioSource != null)
                AudioManager.LetsRemoveAudioSources(m_audioSource);

            Ball.HitGoalOne -= ResetPlayerRotation;
            Ball.HitGoalTwo -= ResetPlayerRotation;
        }

        private void Start()
        {
            m_maxPushDistance = m_rigidbody.transform.localRotation.y != 0 ? -m_playerController.m_matchManager.m_maxPushDistance : m_playerController.m_matchManager.m_maxPushDistance;
            m_pushTarget.transform.position = new Vector3(0.0f, 0.0f, m_pushTarget.transform.position.z + m_maxPushDistance);

            m_localRbTransformPos = m_rigidbody.transform.localPosition;
            m_localRbTransformRotY = m_rigidbody.transform.localRotation.y;

            m_resetRbRotation = m_rigidbody.transform.rotation;
            m_rigidbody.transform.localRotation = m_resetRbRotation;    //Sets y-rotation of all players to 0.

            if (m_audioSource != null)
                AudioManager.LetsRegisterAudioSources(m_audioSource);

            Ball.HitGoalOne += ResetPlayerRotation;
            Ball.HitGoalTwo += ResetPlayerRotation;
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
        /// Rotations of top parent Transform and Rigidbody need to be adjusted to move correct, depending on positive or negative Z-values.
        /// </summary>
        /// <param name="_playerId"></param>
        private void SetPlayerRotation(int _playerId)
        {
            Quaternion playerRotation = (_playerId % 2 == 0) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
            m_playerController.transform.rotation = playerRotation;
            m_rigidbody.transform.rotation = playerRotation;
        }

        private void ResetPlayerRotation()
        {
            switch (m_playerController.m_matchUIStates.RotationReset)
            {
                case true:
                    m_rigidbody.transform.localRotation = m_resetRbRotation;
                    break;
                case false:
                    break;
            }
        }

        #region Movement
        internal void SetInputVector(Vector2 _inputVector, int _receivedID, bool _isRotation)
        {
            if (_receivedID != m_playerController.m_playerId)
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
            var xInvert = GetInversion(m_playerController.m_controlUIStates.InvertXAxis);

            //Move the player relativ to the local Transform-Direction.
            Vector3 rightMovement = m_moveVector.x * xInvert * transform.right;
            Vector3 forwardMovement = m_moveVector.y * transform.forward;

            //Combined Movement.
            Vector3 adjustedMoveVector = (rightMovement + forwardMovement).normalized * (m_moveSpeed * Time.fixedDeltaTime);
            m_rigidbody.MovePosition(m_rigidbody.transform.position + adjustedMoveVector);
        }

        /// <summary>
        /// Clamps the paddlemMovement on it's 'localPosition.x' and the calculated movementRange based on paddleWidth and fieldWidth.
        /// Also clamps the desired minimal and maximal moveDistance on the zAxis based on m_goalDistance and m_maxPushDistance to the playerGoals.
        /// </summary>
        public void ClampMoveRange()
        {
            //Update Paddle scaling based on current matchValues.
            m_paddleWidthAdjustment = GetPaddleWidthAdjustment();

            m_rigidbody.transform.localScale = new Vector3(
                m_playerController.m_localPaddleScale.x + m_paddleWidthAdjustment,
                m_playerController.m_localPaddleScale.y,
                m_playerController.m_localPaddleScale.z
            );

            //Calculate MoveRange.
            m_maxSideMovement = m_playerController.m_groundWidth * 0.5f - m_rigidbody.transform.localScale.x * 0.5f;

            float minZ = -m_playerController.m_groundLength * 0.5f + m_playerController.m_goalDistance;
            float maxZ = minZ + m_playerController.m_maxPushDistance;

            //Clamp Rigidbody moveRange on X-Axis.
            m_rigidbody.transform.localPosition = new Vector3(
                Mathf.Clamp(m_rigidbody.transform.localPosition.x, -m_maxSideMovement, m_maxSideMovement),
                m_rigidbody.transform.localPosition.y,
                Mathf.Clamp(m_rigidbody.transform.localPosition.z, minZ, maxZ)
            );
        }
        #endregion

        #region Rotation        
        private void HandleRotation()
        {
            var yInvert = GetInversion(m_playerController.m_controlUIStates.InvertYAxis);
            Vector3 rotationVector = new(0.0f, m_rotationVector.x, 0.0f); //'Quaternion.Euler' requires a Vector3 for multiplication.

            //Calculate rotation based on input.
            m_deltaRotation = Quaternion.Euler(m_baseRotationSpeed * m_rotationSpeed * Time.fixedDeltaTime * (rotationVector * yInvert)).normalized;

            //Apply rotation.
            Quaternion newRotation = m_rigidbody.rotation * m_deltaRotation;

            //Clamp rotationAngle on maxValue(s).
            newRotation.ToAngleAxis(out float angle, out Vector3 axis);
            angle = Mathf.Clamp(angle, -m_maxRotationAngle, m_maxRotationAngle);

            m_rigidbody.MoveRotation(Quaternion.AngleAxis(angle, axis));
        }
        #endregion

        #region Push
        internal void InitializePush(int _receivedID, bool _initializePush)
        {
            if (_receivedID != m_playerController.m_playerId)
                return;

            if (_initializePush && !m_isPushing)
            {
                //Start pushing.
                m_isPushing = true;
            }
        }

        private IEnumerator HandlePush()
        {
            #region Previous
            //float startZ = m_localRbTransformPos.z;
            //var targetZ = /*m_rigidbody.transform.localRotation.y != 0 ? startZ - m_pushDistance :*/ startZ + m_playerController.m_maxPushDistance;
            ////float targetZ = startZ + (m_rigidbody.transform.localRotation.y != 0 ? -m_pushDistance : m_pushDistance);   //Push-Direction
            //float progress = 0f;
            //while (progress < m_pushDuration)
            //{
            //    progress += Time.deltaTime * m_pushSpeed;

            //    float currentX = m_rigidbody.transform.localPosition.x;
            //    float newZPos = Mathf.Lerp(startZ, targetZ, progress);

            //    m_rigidbody.transform.localPosition = new Vector3(currentX, m_rigidbody.transform.localPosition.y, newZPos);
            //    Debug.Log($"CurX: {currentX} | RbLocalYPos: {m_rigidbody.transform.localPosition.y} | NewZPos: {newZPos}");
            //    //m_rigidbody.MovePosition(new Vector3(currentX, m_rigidbody.transform.localPosition.y, newZPos));
            //    yield return null;
            //}

            //StartCoroutine(HandleRetreat());
            #endregion

            Vector3 startPosition = m_rigidbody.position;
            Vector3 pushDirection = m_rigidbody.transform.forward;
            Vector3 targetPosition = m_pushTarget.position;
            Debug.Log(targetPosition);
            float progress = 0f;
            while (progress < m_pushDuration)
            {
                progress += Time.fixedDeltaTime * m_pushSpeed;

                // Flexible X-Position beibehalten
                float currentX = m_rigidbody.transform.position.x;
                float newZ = Mathf.Lerp(startPosition.z, m_pushTarget.position.z, progress);
                float newX = Mathf.Lerp(startPosition.x, currentX, progress);

                m_rigidbody.position = new Vector3(m_rigidbody.transform.position.x, m_rigidbody.transform.position.y, newZ);
                yield return null;
            }

            StartCoroutine(HandleRetreat());
        }

        private IEnumerator HandleRetreat()
        {
            #region Previous
            //float startZ = m_rigidbody.transform.localPosition.z;
            ////var targetZ = m_rigidbody.transform.localRotation.y != 0 ? startZ + m_pushDistance : startZ - m_pushDistance;

            //float progress = 0.0f;
            //while (progress < m_pushDuration)
            //{
            //    progress += Time.deltaTime * m_retreatSpeed;

            //    float currentX = m_rigidbody.transform.localPosition.x;
            //    float newZPos = Mathf.Lerp(startZ, m_localRbTransformPos.z, progress);

            //    m_rigidbody.transform.localPosition = new Vector3(currentX, m_rigidbody.transform.localPosition.y, newZPos);
            //    //m_rigidbody.MovePosition(new Vector3(currentX, m_rigidbody.transform.localPosition.y, newZPos));
            //    yield return null;
            //}

            //m_isPushing = false;
            #endregion

            Vector3 startPosition = m_rigidbody.position;   //<-!

            float progress = 0.0f;
            while (progress < m_pushDuration)
            {
                progress += Time.fixedDeltaTime * m_retreatSpeed;

                float currentX = m_rigidbody.transform.position.x;
                float newZ = Mathf.Lerp(startPosition.z, m_localRbTransformPos.z, progress);
                float newX = Mathf.Lerp(startPosition.x, currentX, progress);

                m_rigidbody.position = new Vector3(m_rigidbody.transform.position.x, m_rigidbody.position.y, newZ);
                yield return null;
            }

            m_isPushing = false;
        }
        #endregion

        #region Helper-Methods
        private float GetPaddleWidthAdjustment()
        {
            return m_playerController.m_matchValues != null ? m_playerController.m_matchValues.PaddleWidthAdjustment : 0.0f;
        }

        private float GetInversion(bool _invertAxis)
        {
            return _invertAxis ? -1.0f : 1.0f;
        }
        #endregion
    }
}