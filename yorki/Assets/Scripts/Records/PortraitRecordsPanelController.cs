using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PortraitRecordsPanelController : MonoBehaviour
{
    const float RecordRowHeight = 44f;

    public RectTransform listContent;
    public Button recordButtonTemplate;
    public Text summaryText;
    public Text emptyText;
    public RawImage previewImage;
    public Text detailText;

    Texture2D previewTexture;

    public void RefreshRecords()
    {
        ClearGeneratedButtons();

        var records = PortraitRecordRepository.Records;
        if (summaryText != null)
            summaryText.text = $"완성한 초상화 {records.Count}점";

        bool hasRecords = records.Count > 0;
        if (emptyText != null)
            emptyText.gameObject.SetActive(!hasRecords);

        if (!hasRecords)
        {
            ClearPreview();
            if (detailText != null)
                detailText.text = "";
            ResizeContent(0);
            return;
        }

        int rowIndex = 0;
        for (int i = records.Count - 1; i >= 0; i--)
        {
            PortraitRecordData record = records[i];
            CreateRecordButton(record, rowIndex);
            rowIndex++;
        }

        ResizeContent(records.Count);
        ShowRecord(records[records.Count - 1]);
    }

    void CreateRecordButton(PortraitRecordData record, int rowIndex)
    {
        if (recordButtonTemplate == null || listContent == null)
            return;

        Button button = Instantiate(recordButtonTemplate, listContent);
        button.name = "Record_" + record.recordId;
        button.gameObject.SetActive(true);

        RectTransform rt = button.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, -rowIndex * RecordRowHeight);

        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
            label.text = $"Day {record.dayIndex}  {record.hour:00}:{record.minute:00}  {record.customerName}";

        button.onClick.AddListener(() => ShowRecord(record));
    }

    void ShowRecord(PortraitRecordData record)
    {
        if (record == null)
            return;

        if (detailText != null)
        {
            detailText.text =
                $"손님  {record.customerName}\n" +
                $"작업 시각  Day {record.dayIndex}, {record.hour:00}:{record.minute:00}\n" +
                $"점수  {record.score}\n" +
                $"반응  {FormatReaction(record.reactionLevel)}\n" +
                $"수입  € {record.payment:0}.00";
        }

        LoadPreview(PortraitRecordRepository.GetPortraitPath(record));
    }

    void LoadPreview(string path)
    {
        ClearPreview();
        if (previewImage == null || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return;

        try
        {
            previewTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!previewTexture.LoadImage(File.ReadAllBytes(path)))
            {
                ClearPreview();
                return;
            }

            previewTexture.filterMode = FilterMode.Point;
            previewImage.texture = previewTexture;
            previewImage.color = Color.white;
        }
        catch (IOException exception)
        {
            Debug.LogWarning($"[PortraitRecords] Failed to load preview: {exception.Message}");
            ClearPreview();
        }
    }

    void ClearGeneratedButtons()
    {
        if (listContent == null)
            return;

        for (int i = listContent.childCount - 1; i >= 0; i--)
        {
            Transform child = listContent.GetChild(i);
            if (recordButtonTemplate == null || child != recordButtonTemplate.transform)
                Destroy(child.gameObject);
        }
    }

    void ResizeContent(int recordCount)
    {
        if (listContent == null)
            return;

        Vector2 size = listContent.sizeDelta;
        size.y = Mathf.Max(0f, recordCount * RecordRowHeight);
        listContent.sizeDelta = size;
    }

    void ClearPreview()
    {
        if (previewImage != null)
        {
            previewImage.texture = null;
            previewImage.color = Color.clear;
        }

        if (previewTexture != null)
        {
            Destroy(previewTexture);
            previewTexture = null;
        }
    }

    void OnDestroy()
    {
        ClearPreview();
    }

    static string FormatReaction(ReactionLevel level)
    {
        switch (level)
        {
            case ReactionLevel.VerySatisfied: return "매우 만족";
            case ReactionLevel.Satisfied: return "만족";
            case ReactionLevel.Neutral: return "보통";
            case ReactionLevel.Unsatisfied: return "불만족";
            default: return "매우 불만족";
        }
    }
}
