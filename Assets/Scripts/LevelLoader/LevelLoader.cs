using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Animator transition;
    [SerializeField] private float transitionDuration = 1f;

    private PlayerInput input;
    private bool isRestarting;

    private void Awake()
    {
        input = new PlayerInput();
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.RestartLevel.performed += OnRestartLevelPerformed;
    }

    private void OnDisable()
    {
        input.Player.RestartLevel.performed -= OnRestartLevelPerformed;
        input.Player.Disable();
    }

    private void OnDestroy()
    {
        input?.Dispose();
    }

    private void OnRestartLevelPerformed(InputAction.CallbackContext context)
    {
        if (isRestarting)
            return;

        RestartLevel();
    }

    public void RestartLevel()
    {
        if (isRestarting)
            return;

        StartCoroutine(LoadLevelRoutine(SceneManager.GetActiveScene().buildIndex));
    }

    private IEnumerator LoadLevelRoutine(int levelIndex)
    {
        isRestarting = true;

        if (transition)
            transition.SetTrigger("Start");

        if (transitionDuration > 0f)
            yield return new WaitForSeconds(transitionDuration);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelIndex);

        while (!loadOperation.isDone)
            yield return null;
    }
}