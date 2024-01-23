using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBoardPlaneScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // キーの中心座標を取得する処理
        Transform myTransform = this.transform;
        Transform row0Obj = myTransform.GetChild(0);
        Transform row1Obj = myTransform.GetChild(1);
        Transform row2Obj = myTransform.GetChild(2);
        Transform row3Obj = myTransform.GetChild(3);

        Dictionary<string, List<float>> keyCentroids = new Dictionary<string, List<float>>();

        for (int i = 0; i < 10; i++)
        {
            GameObject keyObj = row0Obj.GetChild(i).GetChild(0).gameObject;
            string keytext = getKeyText(keyObj);
            Vector3 keyPosition = getScreenPosition(keyObj);
            List<float> centroid = new List<float> { keyPosition.x, keyPosition.y };
            keyCentroids[keytext] = centroid;
        }

        for (int i = 0; i < 9; i++)
        {
            GameObject keyObj = row1Obj.GetChild(i).GetChild(0).gameObject;
            string keytext = getKeyText(keyObj);
            Vector3 keyPosition = getScreenPosition(keyObj);
            List<float> centroid = new List<float> { keyPosition.x, keyPosition.y };
            keyCentroids[keytext] = centroid;
        }

        for (int i = 1; i < 8; i++)
        {
            GameObject keyObj = row2Obj.GetChild(i).GetChild(0).gameObject;
            string keytext = getKeyText(keyObj);
            Vector3 keyPosition = getScreenPosition(keyObj);
            List<float> centroid = new List<float> { keyPosition.x, keyPosition.y };
            keyCentroids[keytext] = centroid;
        }

        // Debug.Log("keyCentroids.Count: " + keyCentroids.Count);


        // デバッグ
        foreach (KeyValuePair<string, List<float>> pair in keyCentroids)
        {
            // Debug.Log(pair.Key + " : [" + pair.Value[0] + ", " + pair.Value[1] + "]");
        }

        List<int> xCentroids = new List<int>();
        List<int> yCentroids = new List<int>();

        for (char i = 'A'; i <= 'Z'; i++)
        {
            List<float> cent = keyCentroids[i.ToString()];
            xCentroids.Add((int)cent[0]);
            yCentroids.Add((int)cent[1]);
        }

        // Debug.Log("xCentroids: " + string.Join(',', xCentroids));
        // Debug.Log("yCentroids: " + string.Join(',', yCentroids));

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string getKeyText(GameObject keyObj)
    {
        //GameObject childObj = myTransform.GetChild(0).gameObject;
        return keyObj.GetComponent<UnityEngine.TextMesh>().text;
    }

    public Vector3 getScreenPosition(GameObject keyObj)
    {
        Transform ketTransform = keyObj.GetComponent<Transform>();
        Camera cam = Camera.main;
        Vector3 pos = ketTransform.position;
        return cam.WorldToScreenPoint(pos);
    }
}
