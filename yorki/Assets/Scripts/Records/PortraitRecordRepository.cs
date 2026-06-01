using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PortraitRecordRepository
{
    const string SaveFolderName = "Yorki";
    const string PortraitFolderName = "Portraits";
    const string RecordsFileName = "records.json";

    static PortraitRecordCollection collection;

    static string SaveFolderPath => Path.Combine(Application.persistentDataPath, SaveFolderName);
    static string PortraitFolderPath => Path.Combine(SaveFolderPath, PortraitFolderName);
    static string RecordsFilePath => Path.Combine(SaveFolderPath, RecordsFileName);

    public static IReadOnlyList<PortraitRecordData> Records
    {
        get
        {
            EnsureLoaded();
            return collection.records;
        }
    }

    public static PortraitRecordData SavePortrait(Texture2D portrait, string customerId, string customerName,
        int dayIndex, int hour, int minute, int score, ReactionLevel reactionLevel, int payment)
    {
        if (portrait == null)
            return null;

        try
        {
            EnsureLoaded();
            Directory.CreateDirectory(PortraitFolderPath);

            string recordId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string portraitFileName = recordId + ".png";
            File.WriteAllBytes(Path.Combine(PortraitFolderPath, portraitFileName), portrait.EncodeToPNG());

            var record = new PortraitRecordData
            {
                recordId = recordId,
                customerId = customerId ?? "",
                customerName = string.IsNullOrWhiteSpace(customerName) ? "손님" : customerName,
                dayIndex = dayIndex,
                hour = hour,
                minute = minute,
                score = score,
                reactionLevel = reactionLevel,
                payment = payment,
                portraitFileName = portraitFileName
            };

            collection.records.Add(record);
            SaveCollection();
            return record;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[PortraitRecordRepository] 작업 기록 저장 실패: {exception.Message}");
            return null;
        }
    }

    public static string GetPortraitPath(PortraitRecordData record)
    {
        if (record == null || string.IsNullOrWhiteSpace(record.portraitFileName))
            return "";
        return Path.Combine(PortraitFolderPath, record.portraitFileName);
    }

    public static bool ClearAllRecords()
    {
        try
        {
            if (Directory.Exists(PortraitFolderPath))
                Directory.Delete(PortraitFolderPath, true);
            if (File.Exists(RecordsFilePath))
                File.Delete(RecordsFilePath);

            collection = new PortraitRecordCollection();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[PortraitRecordRepository] 작업 기록 전체 삭제 실패: {exception.Message}");
            return false;
        }
    }

    static void EnsureLoaded()
    {
        if (collection != null)
            return;

        try
        {
            if (File.Exists(RecordsFilePath))
                collection = JsonUtility.FromJson<PortraitRecordCollection>(File.ReadAllText(RecordsFilePath));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[PortraitRecordRepository] 작업 기록 로드 실패: {exception.Message}");
        }

        if (collection == null || collection.records == null)
            collection = new PortraitRecordCollection();
    }

    static void SaveCollection()
    {
        Directory.CreateDirectory(SaveFolderPath);
        File.WriteAllText(RecordsFilePath, JsonUtility.ToJson(collection, true));
    }
}
