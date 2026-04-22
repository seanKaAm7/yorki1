using System.Collections;
using UnityEngine;

public class ReactionUI : MonoBehaviour
{
    // 반응 결과를 대사창에 표시하고 3초 뒤 캔버스 초기화
    public void Show(ReactionResult result, string customerName)
    {
        string paymentStr = result.payment > 0 ? $"  (+€{result.payment})" : "";
        DialogueUI.Instance?.Show(customerName, result.dialogue + paymentStr, 3f);
        StartCoroutine(ResetCanvas(3f));
    }

    IEnumerator ResetCanvas(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance?.drawingCanvas?.ClearCanvas();
        // 반응 끝나면 내레이터 화면으로 복귀
        GameManager.Instance?.SetState(GameManager.GameState.Narrator);
    }
}
