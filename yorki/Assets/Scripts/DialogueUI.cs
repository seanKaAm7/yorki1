using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI 레퍼런스")]
    public Text nameText;
    public Text bodyText;

    private Coroutine autoClearRoutine;

    void Awake() => Instance = this;

    // 대사 표시. autoClearAfter > 0 이면 해당 초 뒤에 자동으로 지워짐
    public void Show(string speakerName, string text, float autoClearAfter = 0f)
    {
        if (nameText != null) nameText.text = speakerName;
        if (bodyText  != null) bodyText.text  = text;

        if (autoClearRoutine != null) StopCoroutine(autoClearRoutine);
        if (autoClearAfter > 0f)
            autoClearRoutine = StartCoroutine(AutoClear(autoClearAfter));
    }

    public void Clear()
    {
        if (nameText != null) nameText.text = "";
        if (bodyText  != null) bodyText.text  = "";
        autoClearRoutine = null;
    }

    IEnumerator AutoClear(float delay)
    {
        yield return new WaitForSeconds(delay);
        Clear();
    }
}
