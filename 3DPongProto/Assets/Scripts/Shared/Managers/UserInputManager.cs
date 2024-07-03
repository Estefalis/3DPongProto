using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

namespace ThreeDeePongProto.Shared.InputActions
{
    public class UserInputManager : MonoBehaviour
    {
        //Reference to the PlayerAction-InputAsset.
        public static PlayerInputActions m_playerInputActions;

        #region Change Action Maps Member
        //ActionEvent to switch between ActionMaps within the new InputActionAsset.
        public static event Action<InputActionMap> m_changeActiveActionMap;

        private int m_currentSceneIndex;
        private string m_currentSceneName;
        #endregion

        #region KeyRebinding Member
        public static event Action m_RebindComplete;
        public static event Action m_RebindCanceled;
        public static event Action<InputAction, int> m_rebindStarted;

        private const string m_cancelWithKeyboardButton = "<Keyboard>/escape";
        private const string m_cancelWithGamepadButton = "<Gamepad>/buttonEast";
        #endregion

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the UserInputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Awake()
        {
            if (m_playerInputActions == null)
            {
                m_playerInputActions = new PlayerInputActions();
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneFinishedLoading;            
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneFinishedLoading;
        }

        #region Change Action Maps
        private void OnSceneFinishedLoading(Scene _scene, LoadSceneMode _mode)
        {
            m_currentSceneIndex = _scene.buildIndex; //TODO: Change to SceneIndex after development.
            m_currentSceneName = _scene.name;

            switch (m_currentSceneName)
            {
                case "StartMenuScene":
                    ToggleActionMaps(m_playerInputActions.UI);
                    break;
                case "LocalGameScene":
                    ToggleActionMaps(m_playerInputActions.PlayerActions);
                    break;
                case "LanGameScene":
                    ToggleActionMaps(m_playerInputActions.PlayerActions);
                    break;
                case "NetGameScene":
                    ToggleActionMaps(m_playerInputActions.PlayerActions);
                    break;
                case "WinScene":
                    ToggleActionMaps(m_playerInputActions.UI);
                    break;
                default:
                    ToggleActionMaps(m_playerInputActions.UI);
                    break;
            }
        }
          
        /// <summary>
        /// Switches ActionMaps, if the active actionMap isn't equal to the submitted one. But does not disable the old actionMaps!
        /// </summary>
        /// <param name="_actionMap"></param>
        public static void ToggleActionMaps(InputActionMap _actionMap)
        {
            //If you try to change to the same ActionMap skip the rest.
            if (_actionMap.enabled)
                return;

            //else disable the current ActionMap to switch to the next.
            m_playerInputActions.Disable();
            m_changeActiveActionMap?.Invoke(_actionMap);
            _actionMap.Enable();
#if UNITY_EDITOR
            //Debug.Log($"UserInputManager while debugging: {_actionMap.name} loaded.");
#endif
        }
        #endregion

        #region KeyRebinding
        public static void StartRebindProcess(string _actionName, int _bindingIndex, TextMeshProUGUI _statusText, bool _excludeMouse)
        {
            //Hier wird Bezug auf das durch 'Apply' generierte C#-Script in Unity genommen, nicht auf die Action-Tabelle selbst.
            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);

            //Absicherung gegen falsche Input-Assets.
            if (inputAction == null || inputAction.bindings.Count <= _bindingIndex)
            {
#if UNITY_EDITOR
                Debug.LogError("InputManager: Could not find action or binding.");
#endif
                return;
            }

            //isComposite: Ansammlung einer Sammlung von Bindings, entsprechend des WASD-Composites.
            //isPartOfComposite: Childs/Bindings des Composite. Muessen daher so benannt sein.
            if (inputAction.bindings[_bindingIndex].isComposite)
            {
                var firstParentSubIndex = _bindingIndex + 1;

                if (firstParentSubIndex < inputAction.bindings.Count && inputAction.bindings[firstParentSubIndex].isPartOfComposite)
                {
                    ExecuteKeyRebind(inputAction, firstParentSubIndex, _statusText, true, _excludeMouse);
                }
            }
            else
                ExecuteKeyRebind(inputAction, _bindingIndex, _statusText, false, _excludeMouse);
        }

        /// <summary>
        /// Ausfuehren des Key-Rebindings mittels uebergebener Parameter, entsprechend der ActionMap und des Binding-Indices.
        /// </summary>
        /// <param name="_actionToRebind"></param>
        /// <param name="_bindingIndex"></param>
        /// <param name="_statusText"></param>
        /// <param name="_allCompositeParts"></param>
        /// <param name="_excludeMouse"></param>
        private static void ExecuteKeyRebind(InputAction _actionToRebind, int _bindingIndex, TextMeshProUGUI _statusText, bool _allCompositeParts, bool _excludeMouse)
        {
            if (_actionToRebind == null || _bindingIndex < 0)
                return;

            //TODO: Hier ggf. '$"Please press a Button."; oder '$"Press a {_actionToRebind.expectedControlType}"'.
            _statusText.text = $"Please press a Button";
            //Eine eher menschenuntypische Schreibweise der erwarteten Aktion. Daher ersetzt.
            //_statusText.text = $"Press a {_actionToRebind.expectedControlType}";

            //Wichtig, das hier fuer die Dauer der Aenderung zu blockieren.
            _actionToRebind.Disable();

            //Erzeugung einer Instanz des Rebind-Action-Prozesses.
            InputActionRebindingExtensions.RebindingOperation rebind = _actionToRebind.PerformInteractiveRebinding(_bindingIndex);

            rebind.OnComplete(operation =>
            {
                //Disabled!!! (Moeglicher) Konflikt mit nachtraeglich implementierter 'ToggleActionMaps'-Methode.
                //_actionToRebind.Enable();

                //Verhindern eines Memory-Leaks. Garbage Collection wird hier nicht aufraeumen.
                operation.Dispose();

                //Abarbeiten der Baum-Struktur, wie bei WASD.
                if (_allCompositeParts)
                {
                    var nextBindingIndex = _bindingIndex + 1;

                    if (nextBindingIndex < _actionToRebind.bindings.Count && _actionToRebind.bindings[nextBindingIndex].isPartOfComposite)
                        ExecuteKeyRebind(_actionToRebind, nextBindingIndex, _statusText, _allCompositeParts, _excludeMouse);
                }

                SaveKeyBindingOverride(_actionToRebind);
                //Invoken bei erfolgreich abgeschlossenem Rebinding.
                m_RebindComplete?.Invoke();
            });

            rebind.OnCancel(operation =>
            {
                //Disabled!!! (Moeglicher) Konflikt mit nachtraeglich implementierter 'ToggleActionMaps'-Methode.
                //_actionToRebind.Enable();

                //Verhindern eines Memory-Leaks. Garbage Collection wird hier nicht aufraeumen.
                operation.Dispose();

                //Invoken bei abgebrochenem Rebinding.
                m_RebindCanceled?.Invoke();
            });

            //Falls zusaetzliche Devices Key-Rebinding "canceln" sollen, hier die entsprechenden Commands implementieren.
            rebind.WithCancelingThrough(m_cancelWithKeyboardButton);
            rebind.WithCancelingThrough(m_cancelWithGamepadButton);

            //Devices, wie Maus (oder Andere) hier herausnehmen, sonst wird ein Mausklick bei Aenderungen der Steuerung auch beruecksichtigt!
            if (_excludeMouse)
                rebind.WithControlsExcluding("Mouse");

            m_rebindStarted?.Invoke(_actionToRebind, _bindingIndex);
            //Hier wird der eigentliche "Ueberschreibungs-Prozess gestartet.
            rebind.Start();
        }

        /// <summary>
        /// Methode gibt Informationen fuer den jeweiligen Buttontext zurueck, entsprechend getaetigter Einstellungen und ubermittelter Parameter.
        /// </summary>
        /// <param name="_actionName"></param>
        /// <param name="_bindingIndex"></param>
        /// <returns></returns>
        public static string GetBindingName(string _actionName, int _bindingIndex)
        {
            if (m_playerInputActions == null)
                m_playerInputActions = new PlayerInputActions();

            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);
            return inputAction.GetBindingDisplayString(_bindingIndex);
        }

        /// <summary>
        /// Methode zur Sicherung der KeyRebinding-Aenderungen im Entwicklungsprozess. Muss ggf. noch ersetzt/debugged werden.
        /// </summary>
        /// <param name="_inputAction"></param>
        private static void SaveKeyBindingOverride(InputAction _inputAction)
        {
            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                //Unity hat sowohl einen 'Default-Path' fuer die gesetzten InputActions-Values im 'InputAction-Asset', als auch einen Pfad fuer 'Overrides' waehrend der Runtime.
                PlayerPrefs.SetString(_inputAction.actionMap + _inputAction.name + i, _inputAction.bindings[i].overridePath);
            }
        }

        /// <summary>
        /// Methode zum Laden der KeyRebinding-Aenderungen im Entwicklungsprozess. Muss ggf. noch ersetzt/debugged werden.
        /// Public, da es vom 'Rebind-UI' aufgerufen wird.
        /// </summary>
        /// <param name="_actionName"></param>
        public static void LoadKeyBindingOverride(string _actionName)
        {
            if (m_playerInputActions == null)
                m_playerInputActions = new PlayerInputActions();

            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);

            for (int i = 0; i < inputAction.bindings.Count; i++)
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i)))
                    inputAction.ApplyBindingOverride(i, PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i));
            }
        }

        /// <summary>
        /// Fuer jedes Binding, muss eine 'Remove-Override-Binding-Extension-Function' aufgerufen werden.
        /// </summary>
        /// <param name="_actionName"></param>
        /// <param name="_bindingIndex"></param>
        public static void ResetRebinding(string _actionName, int _bindingIndex)
        {
            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);

            if (inputAction == null && inputAction.bindings.Count <= _bindingIndex)
            {
#if UNITY_EDITOR
                Debug.Log("InputManager: The Action or Binding could not be found!");
#endif
                return;
            }

            if (inputAction.bindings[_bindingIndex].isComposite)
            {
                for (int i = _bindingIndex; i < inputAction.bindings.Count && inputAction.bindings[i].isComposite; i++)
                    inputAction.RemoveBindingOverride(i);
            }
            else
                inputAction.RemoveBindingOverride(_bindingIndex);

            SaveKeyBindingOverride(inputAction);
        }
        #endregion
    }
}