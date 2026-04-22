using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(2, 4)]
    public string text;
}

public class NarratorController : MonoBehaviour
{
    [Header("대사 시퀀스")]
    public DialogueLine[] sequence;

    private int index = 0;

    void Start()
    {
        index = 0;
        ShowCurrent();
    }

    void Update()
    {
        if (GameManager.Instance?.currentState != GameManager.GameState.Narrator) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            Advance();
    }

    void ShowCurrent()
    {
        if (index < sequence.Length)
        {
            DialogueUI.Instance?.Show(sequence[index].speaker, sequence[index].text);
        }
        else
        {
            // 시퀀스 끝 → 드로잉 화면으로 전환
            DialogueUI.Instance?.Clear();
            GameManager.Instance?.StartDrawing();
        }
    }

    void Advance()
    {
        index++;
        ShowCurrent();
    }

    // 반응 끝나고 내레이터 화면으로 돌아올 때 호출
    public void RestartSequence()
    {
        index = 0;
        ShowCurrent();
    }
}
