using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using UnityEngine.UI; // Textの内容を変更するために必要
using System.Linq;
using System.Runtime.CompilerServices;


[System.Serializable]
public class acquiredDataList
{
    public List<acquiredData> data = new List<acquiredData>();
}

[System.Serializable]
public class acquiredData
{
    public string text;
    public string elapsed_time;
}


public class EyeGazeController : MonoBehaviour
{
    public GameObject arrow; // デバッグ用の矢印
    public GameObject eyeLineEmpty; // デバッグ用　視線を表示するLineRenderer用のオブジェクト
    
    // インスペクターからの設定では取得できなかったのでFind関数で取得する
    // カメラ位置を取得するのに使う
    private GameObject oculusCam;
    
    private GameObject beforePointer; // ポインタを保持するプロパティ

    private OVREyeGaze eyeGaze; // 視線情報を保持しているコンポーネント
    private LineRenderer eyeLinerend; // デバッグ用　視線を表示するLineRendererを保持するコンポーネント

    // キーボード情報
    public GameObject keyBoardCanvas; // キーボードのキャンバスオブジェクト　キーボード座標の取得、筆跡の表示等に使う
    private Vector2 pictureSizeForPx; // キーボード画像のピクセル値を保持
    private float boardPixelWidth, boardPixelHeight; // キーボード画像のピクセル値を保持
    private Vector3 leftBottomPos, leftUpperPos, rightUpperPos, rightBottomPos; // キーボードのワールド座標を保持
    private Vector2 pictureSizeForWorld; // ワールド空間でのキーボード座標の大きさを保持

    private LineRenderer LineRendererOnCanvas; // キーボード上に筆跡を表示するためのLineRendererコンポーネント

    private pressedCoordinates pressedCoordinatesForKeyBoardGesture; // 視線のヒット座標を保持するためのデータ構造

    public GameObject candidatePlane; // 推論結果を表示するためのコンポーネント
    public GameObject candidateTextLeft, candidateTextMiddle, candidateTextRight;
    private TextMesh candidateTextCompLeft, candidateTextCompMiddle, candidateTextCompRight;

    public GameObject inputSentenceObj; // 文章を保持するTextオブジェクト
    private TextMesh inputSentenceComp; // 文章を保持するMeshTextコンポーネント

    public GameObject promptTextObj;
    private TextMesh promptTextComp;

    public GameObject wordNotFoundObj;
    private TextMesh wordNotFoundComp;

    public GameObject experimentStartTextObj;
    private TextMesh experimentStartTextComp;

    public GameObject experimentEndTextObj;
    private TextMesh experimentEndTextComp;

    public GameObject deleteKeyObg;
    private TextMesh deleteKeyComp;

    public GameObject NextKeyObg;
    private TextMesh NextKeyComp;

    public GameObject ExampleSentenceObg;
    private TextMesh ExampleSentenceComp;

    private string pleaseInputGesture = "Please Input Gesture";
    private string goToNextSentence = "Go To Next Sentence ?";

    private List<string> InputSentence;

    private List<string> exapleSentencesList = new List<string> {
        "the cat has a pleasant temperament", 
        "important news always seems to be late", 
        "that sticker needs to be validated", 
        "parking tickets can be challenged", 
        "I will put on my glasses", 
        "a touchdown in the last minute", 
        "he is just like everyone else", 
        "exercise is good for the mind", 
        "burglars never leave their business card", 
        "freud wrote of the ego" };
    
    private int exampleInd = 0; // 表示するExampleのインデックス

    // 入力されたテキストを保持するリスト
    private acquiredDataList inputDataList = new acquiredDataList();

    private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();


    // Start is called before the first frame update
    void Start()
    {
        // オブジェクトの取得
        eyeGaze = GetComponent<OVREyeGaze>();
        eyeLinerend = eyeLineEmpty.GetComponent<LineRenderer>();
        LineRendererOnCanvas = keyBoardCanvas.GetComponent<LineRenderer>();
        oculusCam = GameObject.Find("OVRCameraRig");
        // candidateTextComp = candidatePlane.GetComponentInChildren<TextMesh>();
        candidateTextCompLeft = candidateTextLeft.GetComponent<TextMesh>();
        candidateTextCompMiddle = candidateTextMiddle.GetComponent<TextMesh>();
        candidateTextCompRight = candidateTextRight.GetComponent<TextMesh>();
        inputSentenceComp = inputSentenceObj.GetComponent<TextMesh>();
        promptTextComp = promptTextObj.GetComponent<TextMesh>();
        wordNotFoundComp = wordNotFoundObj.GetComponent<TextMesh>();
        experimentStartTextComp = experimentStartTextObj.GetComponent<TextMesh>();
        experimentEndTextComp = experimentEndTextObj.GetComponent<TextMesh>();
        deleteKeyComp = deleteKeyObg.GetComponent<TextMesh>();
        NextKeyComp = NextKeyObg.GetComponent<TextMesh>();
        ExampleSentenceComp = ExampleSentenceObg.GetComponent<TextMesh>();

        // EyeGazeControllerの位置をOVRCameraRigと合わせる
        // これいらないかも
        // this.transform.position = oculusCam.transform.position;

        resetWord();

        // 候補ワードの初期値をセットする
        initCandidateTexts();
        experimentStartTextComp.color = Color.yellow;

        // 視線用LineRendererのセッティング
        eyeLinerend.widthMultiplier = 0.01f;

        // 筆跡用LineRendererのセッティング
        LineRendererOnCanvas.startWidth = 0.3f; // 開始点の太さを0.1にする
        LineRendererOnCanvas.endWidth = 0.3f; // 終了点の太さを0.1にする
        LineRendererOnCanvas.startColor = Color.red;
        LineRendererOnCanvas.endColor = Color.green;

        // 取得したヒット座標を保持するデータ構造の初期化
        pressedCoordinatesForKeyBoardGesture = new pressedCoordinates();

        // キーボード情報の取得
        GetKeyBoardGeometrySettings();

        sw.Reset();
        sw.Start();

        // デバッグ用　サーバーにアクセスする　この状態だと500番台のエラーが出る
        // StartCoroutine("CallServer2");
    }

    private void GetKeyBoardGeometrySettings() {
        RectTransform rectTransform = keyBoardCanvas.GetComponent<RectTransform>();

        pictureSizeForPx = rectTransform.sizeDelta;
        Debug.Log("pictureSizeForPx: " + pictureSizeForPx);

        boardPixelWidth = pictureSizeForPx.x * 10;
        boardPixelHeight = pictureSizeForPx.y * 10;

        // キーボードのワールド座標を取得
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        for (var i = 0; i < corners.Length; i++)
        {
            Debug.Log($"Key Board Corners[{i}] : {corners[i]}");
        }
        leftBottomPos = corners[0];
        leftUpperPos = corners[1];
        rightUpperPos = corners[2];
        rightBottomPos = corners[3];

        pictureSizeForWorld = new Vector2(rightUpperPos.x - leftUpperPos.x, leftUpperPos.y - leftBottomPos.y);
    }

    // 候補ワードの初期値をセットする
    private void initCandidateTexts() {
        candidateTextCompLeft.text = candidateTextCompMiddle.text = candidateTextCompRight.text = "";
        promptTextComp.color = Color.clear;
        wordNotFoundComp.color = Color.clear;
        experimentStartTextComp.color = Color.clear;
        experimentEndTextComp.color = Color.clear;
    }

    private void addWord(string word) {
        InputSentence.Add(word);
        inputSentenceComp.text = string.Join(" ", InputSentence);
    }

    private void DelWord() {
        if (InputSentence.Count > 0) InputSentence.RemoveAt(InputSentence.Count - 1);
        inputSentenceComp.text = string.Join(" ", InputSentence);
    }

    private void resetWord() {
        InputSentence = new List<string>();
    }

    // Update is called once per frame
    void Update() {
    
        if (eyeGaze == null) return;
        // アイトラッキングの無効時
        if (!eyeGaze.EyeTrackingEnabled) return;

        // 矢印と視線の同期
        arrow.transform.rotation = eyeGaze.transform.rotation;

        // 視線の視点と方向を取得する
        Vector3 oculusCamPos = oculusCam.transform.position;
        Vector3 rayStartPos = new Vector3(oculusCamPos.x, oculusCamPos.y, oculusCamPos.z);
        // rayStartPos.x += 0.5f;
        // rayStartPos.y -= 1.0f;
        Vector3 eyeDirection = eyeGaze.transform.TransformDirection(Vector3.forward); // 方向をクオーテーションからVector3に変換（多分）
        // eyeDirection.z -= 0.1f;
        // Debug.Log(oculusCamPos); Debug.Log(eyeDirection); // デバッグ用

        // LineRendererで視線を引く
        eyeLinerend.SetPosition(0, oculusCamPos); eyeLinerend.SetPosition(1,  oculusCamPos + eyeDirection * 50);

        // 候補ワードの色を黒に戻す
        candidateTextCompLeft.color = candidateTextCompMiddle.color = candidateTextCompRight.color = Color.black;
        // 削除キーの色を黒に戻す
        deleteKeyComp.color = Color.black;
        // Nextキーの色を黒に戻す
        NextKeyComp.color = Color.black;

        // 以下、RayCastを行う
        Ray ray = new Ray(rayStartPos, eyeDirection);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            // ヒットしたGameObjectを取得
            GameObject gameObject = hit.collider.gameObject;
            Debug.Log(gameObject); // for debug
            Debug.Log("hit point: " + hit.point); // for debug

            // ポインタの生成
            GameObject Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Sphere.transform.position = hit.point; // ポインタ位置を設定
            Sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // ポインタの大きさを設定
            Renderer pointerRenderer = Sphere.GetComponent<Renderer>();
            pointerRenderer.material.color = Color.magenta; // ポインタの色を設定
            SphereCollider sphereCollider = Sphere.GetComponent<SphereCollider>();
            if (sphereCollider != null) sphereCollider.enabled = false; // コライダーを無効化する

            // 次の文章に移るキーである場合
            if (gameObject.CompareTag("NextKey")) {
                NextKeyComp.color = Color.red;

                if (OVRInput.GetUp(OVRInput.Button.One)) {
                    ExampleSentenceComp.text = "";
                    initCandidateTexts();
                    if (exampleInd <= 10) {
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        float elapsedSeconds = (float)(ts.Hours * 60 * 60 + ts.Minutes * 60 + ts.Seconds) + (float)(ts.Milliseconds) / 1000.0f;

                        // 入力されたテキストと経過時間をリストに保存する
                        inputDataList.data.Add(
                            new acquiredData() {
                                text = inputSentenceComp.text,
                                elapsed_time = elapsedSeconds.ToString()
                            }
                        );
                        inputSentenceComp.text = ""; // 入力されたテキストをテキストUIから消す
                        resetWord(); // ワードを保持しているリストを初期化する

                        // 次のテキストへの更新の場合
                        if (exampleInd < 10) {
                            ExampleSentenceComp.text = exapleSentencesList[exampleInd];
                            promptTextComp.color = Color.gray;

                            // タイマーをリセットしてスタートする
                            sw.Reset();
                            sw.Start();
                        
                        // 最後の処理
                        } else if (exampleInd == 10) {
                            experimentEndTextComp.color = Color.yellow;
                            // ネットワーク処理の呼び出し
                            StartCoroutine("CallServerToRegisterData");
                        }
                        
                        exampleInd += 1;
                    }
                }

            // 削除のキーである場合
            } else if (gameObject.CompareTag("DeleteKey")) {
                deleteKeyComp.color = Color.red;
                if (OVRInput.GetUp(OVRInput.Button.One)) {
                    DelWord();
                }

            // 候補ワードである場合
            } else if (gameObject.CompareTag("CandidateWordLeft")) {
                Debug.Log("word text hit!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! left");
                candidateTextCompLeft.color = Color.red;

                if (OVRInput.GetUp(OVRInput.Button.One))  {
                    addWord(gameObject.GetComponentInChildren<TextMesh>().text);
                    initCandidateTexts();
                    promptTextComp.color = Color.gray;
                }

            // 候補ワードである場合
            } else if (gameObject.CompareTag("CandidateWordMiddle")) {
                Debug.Log("word text hit!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! middle");
                candidateTextCompMiddle.color = Color.red;

                if (OVRInput.GetUp(OVRInput.Button.One))  {
                    addWord(gameObject.GetComponentInChildren<TextMesh>().text);
                    initCandidateTexts();
                    promptTextComp.color = Color.gray;
                }
            
            // 候補ワードである場合
            } else if (gameObject.CompareTag("CandidateWordRight")) {
                Debug.Log("word text hit!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! right");
                candidateTextCompRight.color = Color.red;

                if (OVRInput.GetUp(OVRInput.Button.One))  {
                    addWord(gameObject.GetComponentInChildren<TextMesh>().text);
                    initCandidateTexts();
                    promptTextComp.color = Color.gray;
                }

            // キーボードUIである場合
            } else {
                // 入力モードに入ったタイミングで既存の筆跡を消す
                if (OVRInput.GetDown(OVRInput.Button.One))  {
                    LineRendererOnCanvas.positionCount = 0;
                    // 記録の初期化
                    pressedCoordinatesForKeyBoardGesture = new pressedCoordinates();
                }

                // 入力モード時の処理
                if (OVRInput.Get(OVRInput.Button.One)) {
                    // ポインタの色を変更
                    pointerRenderer.material.color = Color.green;
                    Debug.Log(hit.point.x + ", " + hit.point.y); // for debug
                    

                    // ヒット座標を画像のピクセル座標に変換
                    float xHitPosForPixel = (hit.point.x - leftUpperPos.x) / pictureSizeForWorld.x * boardPixelWidth;
                    float yHitPosForPixel = (leftUpperPos.y - hit.point.y) / pictureSizeForWorld.y * boardPixelHeight;
                    Debug.Log("xHitPosForPixel: " + xHitPosForPixel); // for debug
                    Debug.Log("yHitPosForPixel: " + yHitPosForPixel); // for debug
                    Coordinate xyPairCoordinate = new Coordinate()
                    {
                        x = xHitPosForPixel,
                        y = yHitPosForPixel
                    };
                    pressedCoordinatesForKeyBoardGesture.data.Add(xyPairCoordinate);


                    // 古いポインタがある場合、筆跡の追加を行う
                    if (beforePointer != null) {
                        // 筆跡の太さを一定にするために点を複数打つ
                        for (var i = 0; i < 3; i++) {
                            LineRendererOnCanvas.positionCount += 1;
                            LineRendererOnCanvas.SetPosition(LineRendererOnCanvas.positionCount - 1, hit.point);
                        }
                    }
                }

                // 入力モードが終了したときの処理
                if (OVRInput.GetUp(OVRInput.Button.One)) {

                    // for debug
                    string tmp = "";
                    foreach (var coord in pressedCoordinatesForKeyBoardGesture.data) {
                        tmp += "(" + Convert.ToString(coord.x) + "," + Convert.ToString(coord.y) + "), ";
                    }
                    Debug.Log("up!!!!!!!!!!!!!!!!!!!!!!!!!!!!"); // for debug
                    Debug.Log("All Pressed Coordinates."); // for debug
                    Debug.Log(tmp); // for debug

                    // ネットワーク処理の呼び出し
                    StartCoroutine("CallServer2");

                    // 候補ワードの部分の”Please Input Gesture”を消す
                    promptTextComp.color = Color.clear;

                    // 記録の初期化
                    pressedCoordinatesForKeyBoardGesture = new pressedCoordinates();
                }
            }

            // 古いポインタがある場合、そのポインタの廃棄を行う
            if (beforePointer != null) {
                Destroy(beforePointer);
            }
            beforePointer = Sphere;

        } else {
            // 視線がUIとヒットしない場合、ポインタを消す
            if (beforePointer != null) {
                Destroy(beforePointer);
            }
        }
    }


    private void UpdateCandidatePlane(string[] candidateWords) {
        initCandidateTexts();

        // ワードが見つからなかった場合
        if (candidateWords[0] == "Word" && candidateWords[1] == "not" && candidateWords[2] == "found") {
            wordNotFoundComp.color = Color.cyan;
            return;
        }
    
        candidateTextCompLeft.text = candidateWords[0];
        if (candidateWords.Length > 1) candidateTextCompMiddle.text = candidateWords[1];
        if (candidateWords.Length > 2) candidateTextCompRight.text = candidateWords[2];
    }


    private string GetJsonData()
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


    // string Adderess = "http://127.0.0.1:8080";
    string Adderess = "http://192.168.10.17:8080";
    // string Adderess = "http://192.168.219.119:8080";
    //string testPage = "/test-api";

    IEnumerator CallServer2()
    {
        string Page = "/shark2";
        var url = Adderess + Page;
        var json = GetJsonData();
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

            string[] bestWords = resJson.best_word.Split(" ");

            UpdateCandidatePlane(bestWords);
        }
    }

    IEnumerator CallServerToRegisterData() {
        string Page = "/registerData";
        var url = Adderess + Page;

        // var json = GetJsonData();
        var json = JsonUtility.ToJson(inputDataList);

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
        }
    }
}
