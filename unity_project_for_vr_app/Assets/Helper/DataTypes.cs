using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class acquiredDataList
{
    public string input_method = null;
    public List<acquiredData> data = new List<acquiredData>();
}

[System.Serializable]
public class acquiredData
{
    public string result_text;
    public string input_keys;
    public List<pressedCoordinates> series_coordinates;
    public string elapsed_time;
}
