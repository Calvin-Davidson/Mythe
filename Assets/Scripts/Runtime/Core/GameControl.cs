using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameControl : MonoBehaviour
{
    private static GameControl instance;
    public UnityEvent onBlockUpdate;
    public UnityEvent onBlockPushed;
    public UnityEvent onNextLevel;
    public UnityEvent onResetLevel;
    private void Awake()
    {

        if (instance != null) return;
        instance = this;
        onBlockUpdate = new UnityEvent();
        onBlockPushed = new UnityEvent();
        onNextLevel = new UnityEvent();
        onResetLevel = new UnityEvent();
    }

    public void BlockUpdateNextFrame()
    {
        StartCoroutine(RunNextFixedUpdate(() => onBlockUpdate?.Invoke()));
    }

    private IEnumerator RunNextFixedUpdate(Action a)
    {
        yield return new WaitForFixedUpdate();
        a?.Invoke();
    }

    public void LoadLevel(int id)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(id);
    }

    public static GameControl Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.AddComponent<GameControl>();
            }
            return instance;
        }
    }
}
