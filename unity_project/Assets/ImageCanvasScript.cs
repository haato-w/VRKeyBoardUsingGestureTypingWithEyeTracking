using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.EventSystems;


[System.Serializable]
public class Coordinate
{
    public float x;
    public float y;
}

[System.Serializable]
public class pressedCoordinates
{
    public List<Coordinate> data = new List<Coordinate>();
}

public class responseJson
{
    public string best_word;
    public float elapsed_time;
}

public class ImageCanvasScript : MonoBehaviour
{
    Vector3 leftBottomPos;
    Vector3 leftUpperPos;
    Vector3 rightUpperPos;
    Vector3 rightBottomPos;
    Vector2 pictureSizeForPx;
    Vector2 pictureSizeForScreen;

    // 試してみる
    public Camera oculusCam;

    private pressedCoordinates pressedCoordinatesForKeyBoardGesture;

    // private CandidatePlaneScript candidatePlaneObject;

    public GameObject pointer;

    private bool button_pressed = false;

    public List<GameObject> handwriting_object = new List<GameObject>();

    GameObject eyeGazeController;
    
    // void OnTriggerEnter(Collider other) {
    //     Debug.Log("hit!!!!!!");
    // }

    // void OnTriggerStay(Collider other)
    // {
        // Debug.Log("pass throgh!!");
        // Vector3 hitPos = other.ClosestPointOnBounds(this.transform.position);
        // oculusCam
        // Debug.Log(other.contacts);
        // Debug.Log(hitPos);

        // raycastを使う方式　raycastを使うのであればontriggerを使うべきではないかも
        // eyeGazeController = GameObject.Find("EyeGazeController");
        // OVREyeGaze gaze = eyeGazeController.GetComponent<EyeGazeController>().eyeGaze;
        // if (gaze != null && gaze.EyeTrackingEnabled) {
        //     // 視線の同期
        //     Vector3 direction = new Vector3();
        //     direction.x = gaze.transform.rotation.x;
        //     direction.y = gaze.transform.rotation.y;
        //     direction.z = gaze.transform.rotation.z - 1.0f;
        //     Debug.Log("oculusCam pos: " + oculusCam.transform.position);
        //     Debug.Log("eye direction: " + direction);
        //     Ray oculusCamRay = new Ray(oculusCam.transform.position, direction);
        //     Debug.Log("do raycast");
        //     Debug.DrawRay(new Vector3(0, 0, 0), oculusCamRay.direction * 30, Color.red, 5.0f);
        //     RaycastHit hit;
        //     if (Physics.Raycast(oculusCamRay,out hit, 100.0f)) {
        //         Debug.Log("raycast hit!!");
        //         Debug.Log(hit.collider.gameObject.transform.position);
        //     }
        // } else {
        //     if (gaze == null) { Debug.Log("gaze is null"); }
        //     else { Debug.Log("can not get eye direction"); }
        // }
        

        // ポインタの座標を更新する
        // pointer.transform.position = hitPos;
        // Vector3 pos = pointer.transform.position;
        // pos.z = 0;
        // pointer.transform.position = pos;


        // // ボタンの色を変化させる
        // if (button_pressed) {
        //     pointer.GetComponent<Renderer>().material.color = Color.magenta;
        // } else {
        //     pointer.GetComponent<Renderer>().material.color = Color.green;
        // }

        // // 筆跡を追加する
        // if (button_pressed) {
        //     GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //     sphere.GetComponent<Renderer>().material.color = Color.magenta;
        //     sphere.transform.position = hitPos;
        // }

        // // 座標の記録を取る
        // if (button_pressed) {
        //     Coordinate xyPairCoordinate = getCoordinateForGesture(hitPos);
        //     pressedCoordinatesForKeyBoardGesture.data.Add(xyPairCoordinate);
        // }
    // }

    // private void OnTriggerExit(Collider other) {
    //     // キーボードから視線を外すとポインタの色を変える
    //     pointer.GetComponent<Renderer>().material.color = Color.red;
    // }


    // Start is called before the first frame update
    void Start()
    {
        Transform myTransform = this.transform;

        Camera cam = Camera.main;

        Vector3 pos = myTransform.position; // キーボードのワールド座標
        Vector3 keyBoardScreenPos = cam.WorldToScreenPoint(pos); // キーボードのスクリーン座標
        // Debug.Log("keyBoardScreenPos: " + keyBoardScreenPos);

        Vector3 absScale = myTransform.lossyScale;
        // Debug.Log("absScale: " + absScale);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        pictureSizeForPx = rectTransform.sizeDelta;
        // Debug.Log("pictureSizeForPx: " + pictureSizeForPx);

        // キーボードのスクリーン座標を取得
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        for (var i = 0; i < corners.Length; i++)
        {
            // ワールド座標からスクリーン座標へ変換
            corners[i] = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[i]);
            // Debug.Log($"Key Board Corners[{i}] : {corners[i]}");
        }
        leftBottomPos = corners[0];
        leftUpperPos = corners[1];
        rightUpperPos = corners[2];
        rightBottomPos = corners[3];

        pictureSizeForScreen = new Vector2(rightUpperPos.x - leftUpperPos.x, leftUpperPos.y - leftBottomPos.y);
        // Debug.Log("pictureSizeForScreen: " + pictureSizeForScreen);

        // キーボード左上の画面上の座標が欲しい
        float boardWidth = pictureSizeForPx.x * 10;
        float boardHeight = pictureSizeForPx.y * 10;
        
        float boardLeftUpperX = keyBoardScreenPos.x - boardWidth / 2;
        float boardLeftUpperY = keyBoardScreenPos.y + boardHeight / 2;

        // Debug.Log("boardLeftUpperX: " + boardLeftUpperX);
        // Debug.Log("boardLeftUpperY: " + boardLeftUpperY);


        // 候補ワードオブジェクトの取得
        // GameObject rootObject = transform.root.gameObject;
        // candidatePlaneObject = GameObject.Find("CandidatePlane");
        // candidatePlaneObject = GetComponent<CandidatePlaneScript>();

        pressedCoordinatesForKeyBoardGesture = new pressedCoordinates();
        // StartCoroutine("CallServer2");
        
    }

    // Update is called once per frame
    void Update()
    {
        // // レイキャストによる座標取得を試してみる
        // // var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // var ray = oculusCam.ScreenPointToRay(Input.mousePosition);
        // RaycastHit hit;

        // if (Physics.Raycast(ray, out hit))
        // {
        //     Vector2 pixelUV = hit.textureCoord;
        //     Debug.Log("pixelUV:::" + pixelUV.x + " , " + pixelUV.y);
        // }


        // // questのボタンを押し始める
        // if(OVRInput.GetDown(OVRInput.Button.One)) {
        //     Debug.Log("Aボタンを押しています");
        //     foreach (var obj in handwriting_object) { Destroy(obj); } // 筆跡オブジェクトを削除する
        //     pressedCoordinatesForKeyBoardGesture = new pressedCoordinates();
        //     button_pressed = true;
        // }

        // // questのボタンを押す
        // if(OVRInput.Get(OVRInput.Button.One))
        // {
        //     Debug.Log("Aボタンを押しています");
        //     button_pressed = true;
        // }

        // // questのボタンを話した
        // if(OVRInput.GetUp(OVRInput.Button.One)) {
        //     Debug.Log("Aボタンを押しています");
        //     Debug.Log("All Pressed Coordinates." + string.Join(",", pressedCoordinatesForKeyBoardGesture));
        //     button_pressed = false;
        //     // StartCoroutine("CallServer2");
        // }


        //左クリックを受け付ける
        // if (Input.GetMouseButtonDown(0))
        // {
        //     pressedCoordinatesForKeyBoardGesture = new pressedCoordinates();

        //     Vector3 mousePosition = Input.mousePosition;
        //     Debug.Log("Pressed primary button." + mousePosition);

        //     Coordinate xyPairCoordinate = getCoordinateForGesture(mousePosition);

        //     pressedCoordinatesForKeyBoardGesture.data.Add(xyPairCoordinate);
        // }

        // if (Input.GetMouseButton(0))
        // {
        //    Vector3 mousePosition = Input.mousePosition;
        //    //Debug.Log("Pressed primary button 3." + mousePosition);

        //    Coordinate xyPairCoordinate = getCoordinateForGesture(mousePosition);

        //    pressedCoordinatesForKeyBoardGesture.data.Add(xyPairCoordinate);
        // }

        // if (Input.GetMouseButtonUp(0))
        // {
        //    Vector3 mousePosition = Input.mousePosition;
        //    Debug.Log("Pressed primary button 2." + mousePosition);

        //    Coordinate xyPairCoordinate = getCoordinateForGesture(mousePosition);

        //    pressedCoordinatesForKeyBoardGesture.data.Add(xyPairCoordinate);

        //    Debug.Log("All Pressed Coordinates." + string.Join(",", pressedCoordinatesForKeyBoardGesture));

        // //    StartCoroutine("CallServer");
        //    StartCoroutine("CallServer2");
        // }
    }

    private Coordinate getCoordinateForGesture(Vector3 mousePosition)
    {
        float mousePosFromLeftUpperX = mousePosition.x;
        float mousePosFromLeftUpperY = Screen.height - mousePosition.y;

        // float keyBoardMousePosX = mousePosFromLeftUpperX - leftUpperPos.x;
        // float keyBoardMousePosY = mousePosFromLeftUpperY - leftUpperPos.y;

        float mousePosForPictureSizeX = (mousePosFromLeftUpperX - leftUpperPos.x) / (pictureSizeForScreen.x) * (pictureSizeForPx.x * 10);
        float mousePosForPictureSizeY = (mousePosFromLeftUpperY - (Screen.height - leftUpperPos.y)) / (pictureSizeForScreen.y) * (pictureSizeForPx.y * 10);

        Debug.Log("mousePosForPictureSizeX: " + mousePosForPictureSizeX);
        Debug.Log("mousePosForPictureSizeY: " + mousePosForPictureSizeY);

        Coordinate xyPairCoordinate = new Coordinate()
        {
            x = mousePosForPictureSizeX,
            y = mousePosForPictureSizeY
        };

        return xyPairCoordinate;
    }

    private string getJsonData()
    {
        Debug.Log("pressedCoordinatesForKeyBoardGesture.Count: " + pressedCoordinatesForKeyBoardGesture.data.Count);

        // pressedCoordinates newPressedCoordinatesForKeyBoardGesture = new pressedCoordinates();

        // データ数多すぎるので間引く
        // for (int i = 0; i < (pressedCoordinatesForKeyBoardGesture.data.Count / 10) + 1; i++)
        // {
        //     newPressedCoordinatesForKeyBoardGesture.data.Add(pressedCoordinatesForKeyBoardGesture.data[i * 10]);
        // }

        // Debug.Log("newPressedCoordinates.Count: " + newPressedCoordinates.data.Count);

        string json = JsonUtility.ToJson(pressedCoordinatesForKeyBoardGesture);
        return json;
    }

    public void updateCandidatePlane(List<string> candidateTexts)
    {
        // Debug.Log("transform.root: " + transform.root);
        GameObject CandidatePlane = GameObject.Find("CandidatePlane");
        Debug.Log("CandidatePlane: " + CandidatePlane);
        Debug.Log("CandidatePlane.GetComponentsInChildren<TextMesh>()" + CandidatePlane.GetComponentsInChildren<TextMesh>().Count());
        TextMesh text = CandidatePlane.GetComponentsInChildren<TextMesh>()[0];
        text.text = string.Join(",", candidateTexts);

        // candidatePlaneObject = GetComponent<CandidatePlaneScript>();
        // Text candidateTextComp = GetComponentInChildren<Text>();
        // candidateTextComp.text = string.Join(",", candidateTexts);
        // // string candidateText = candidateTextComp.text.ToString();
    }

    IEnumerator CallServer2()
    {
        //string localHost = "http://127.0.0.1:5000";
        string Adderess = "http://192.168.10.50:7200";
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
            string response = request.downloadHandler.text;
            Debug.Log("Succeeded:" + response);
            // new List<string>{request.downloadHandler.text}

            responseJson resJson = JsonUtility.FromJson<responseJson>(response);
            Debug.Log("resJson: " + resJson.best_word);

            List<string> resWords = new List<string>(){resJson.best_word};
            // // a.Add("test");
            updateCandidatePlane(resWords);

        }
    }
}
