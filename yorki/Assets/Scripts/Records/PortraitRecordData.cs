using System;
using System.Collections.Generic;

[Serializable]
public class PortraitRecordData
{
    public string recordId;
    public string customerId;
    public string customerName;
    public int dayIndex;
    public int hour;
    public int minute;
    public int score;
    public ReactionLevel reactionLevel;
    public int payment;
    public string portraitFileName;
}

[Serializable]
public class PortraitRecordCollection
{
    public List<PortraitRecordData> records = new List<PortraitRecordData>();
}
