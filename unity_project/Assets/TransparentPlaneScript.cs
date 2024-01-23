using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


// [System.Serializable]
// public class Coordinate
// {
//     public float x;
//     public float y;
// }

// [System.Serializable]
// public class pressedCoordinates
// {
//     public List<Coordinate> data = new List<Coordinate>();
// }

public class TransparentPlaneScript : MonoBehaviour
{

    //private List<Coordinate> pressedCoordinates;
    private pressedCoordinates pressedCoordinates;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        //左クリックを受け付ける
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Vector3 mousePosition = Input.mousePosition;
        //     Debug.Log("Pressed primary button." + mousePosition);

        //     this.pressedCoordinates = new pressedCoordinates();

        //     this.RegisterCoordinate(mousePosition);
        // }

        //if (Input.GetMouseButton(0))
        //{
        //    Vector3 mousePosition = Input.mousePosition;
        //    //Debug.Log("Pressed primary button 3." + mousePosition);

        //    this.RegisterCoordinate(mousePosition);
        //}

        //if (Input.GetMouseButtonUp(0))
        //{
        //    Vector3 mousePosition = Input.mousePosition;
        //    Debug.Log("Pressed primary button 2." + mousePosition);

        //    this.RegisterCoordinate(mousePosition);

        //    Debug.Log("All Pressed Coordinates." + string.Join(",", pressedCoordinates));

        //    //StartCoroutine("CallServer");
        //    StartCoroutine("CallServer2");
        //}



        ////右クリックを受け付ける
        //if (Input.GetMouseButtonDown(1))
        //    Debug.Log("Pressed secondary button.");

        ////ミドルクリックを受け付ける
        //if (Input.GetMouseButtonDown(2))
        //    Debug.Log("Pressed middle click.");
    }


    private void RegisterCoordinate(Vector3 coordinate)
    {
        //Dictionary<string, string> xyPairCoordinate = new Dictionary<string, string>();
        //xyPairCoordinate.Add("x", coordinate.x.ToString());
        //xyPairCoordinate.Add("y", coordinate.y.ToString());
        //foreach (KeyValuePair<string, string> items in xyPairCoordinate)
        //    Debug.Log("xyPairCoordinate: " + items.Key + ": " + items.Value);

        //pressedCoordinates.Add(xyPairCoordinate);



        Coordinate xyPairCoordinate = new Coordinate()
        {
            x = coordinate.x,
            y = coordinate.y,
        };

        pressedCoordinates.data.Add(xyPairCoordinate);
    }


    private string getJsonData()
    {
        Debug.Log("pressedCoordinates.Count: " + pressedCoordinates.data.Count);

        pressedCoordinates newPressedCoordinates = new pressedCoordinates();

        // データ数多すぎるので間引く
        for (int i = 0; i < (pressedCoordinates.data.Count / 10) + 1; i++)
        {
            newPressedCoordinates.data.Add(pressedCoordinates.data[i * 10]);
        }

        Debug.Log("newPressedCoordinates.Count: " + newPressedCoordinates.data.Count);

        string json = JsonUtility.ToJson(newPressedCoordinates);
        return json;
    }


    IEnumerator CallServer()
    {

        string Adderess = "127.0.0.1:5000";
        string Page = "/test_api";

        string json = getJsonData();
        Debug.Log("json: " + json);

        WWWForm form = new WWWForm();
        form.AddField("json", json);

        //1.UnityWebRequestを生成
        UnityWebRequest request = UnityWebRequest.Post("http://" + Adderess + Page, form);

        //2.SendWebRequestを実行し、送受信開始
        yield return request.SendWebRequest();

        //3.isNetworkErrorとisHttpErrorでエラー判定
        if (request.isHttpError || request.isNetworkError)
        {
            //4.エラー確認
            Debug.Log(request.error);
        }
        else
        {
            //4.結果確認
            Debug.Log(request.downloadHandler.text);
        }

    }



    IEnumerator CallServer2()
    {
        //string localHost = "http://127.0.0.1:5000";
        string Adderess = "http://192.168.86.250:7200";
        //string testPage = "/test-api";
        string Page = "/shark2";

        var url = Adderess + Page;
        var json = getJsonData();
        //var json = JsonUtility.ToJson(data);
        var postData = Encoding.UTF8.GetBytes(json);

        Debug.Log("json: " + json);

        using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(postData),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        //if (request.result == UnityWebRequest.Result.ConnectionError ||
        //    request.result == UnityWebRequest.Result.ProtocolError)
        //{

        // 通信結果
        if (request.isNetworkError ||
            request.isHttpError)  // 失敗
        {
            Debug.Log("Network error:" + request.error);
        }
        else                  // 成功
        {
            Debug.Log("Succeeded:" + request.downloadHandler.text);
        }
    }

}


