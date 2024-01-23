using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using UnityEngine.UI; // Textの内容を変更するために必要
using System.Linq;
using System.Runtime.CompilerServices;



public class ControllerOnly : MonoBehaviour
{
    public GameObject leftContorollerObj;
    public GameObject rightContorollerObj;

    // public GameObject eyeLineEmpty; // デバッグ用　視線を表示するLineRenderer用のオブジェクト
    public GameObject leftContorollerLineObj;
    private LineRenderer leftContorollerLineComp;
    public GameObject rightContorollerLineObj;
    private LineRenderer rightContorollerLineComp;
    
    // インスペクターからの設定では取得できなかったのでFind関数で取得する
    // カメラ位置を取得するのに使う
    // private GameObject oculusCam;
    
    // private GameObject beforePointer; // ポインタを保持するプロパティ
    private GameObject leftBeforePointer;
    private GameObject rightBeforePointer;

    // private OVREyeGaze eyeGaze; // 視線情報を保持しているコンポーネント
    // private LineRenderer eyeLinerend; // デバッグ用　視線を表示するLineRendererを保持するコンポーネント

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

    // public GameObject deleteKeyObg;
    // private TextMesh deleteKeyComp;

    // public GameObject NextKeyObg;
    // private TextMesh NextKeyComp;

    public GameObject ExampleSentenceObg;
    private TextMesh ExampleSentenceComp;

    private string pleaseInputGesture = "Please Input Gesture";
    private string goToNextSentence = "Go To Next Sentence ?";

    private List<string> InputSentence;

    private List<string> exapleSentencesList = ExampleSentences.ControllerOnlyExampleSentencesForPractice;
    
    private int exampleInd = 0; // 表示するExampleのインデックス

    // 入力されたテキストを保持するリスト
    private acquiredDataList inputDataList = new acquiredDataList();

    private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

    private bool isLeftInput = false;
    private bool isRightInput = false;

    // Centroids of 26 keys
    List<float> centroids_X = new List<float>(){50, 205, 135, 120, 100, 155, 190, 225, 275, 260, 295, 330, 275, 240, 310, 345, 30, 135, 85, 170, 240, 170, 65, 100, 205, 65};
    List<float> centroids_Y = new List<float>(){85, 120, 120, 85, 50, 85, 85, 85, 50, 85, 85, 85, 120, 120, 50, 50, 50, 50, 85, 50, 50, 120, 50, 120, 50, 120};

    // Start is called before the first frame update
    void Start()
    {
        inputDataList.input_method = "ControllerOnly";

        // コントローラーから出る光線のLineRendererコンポーネントを取得
        leftContorollerLineComp = leftContorollerLineObj.GetComponent<LineRenderer>();
        rightContorollerLineComp = rightContorollerLineObj.GetComponent<LineRenderer>();
        
        LineRendererOnCanvas = keyBoardCanvas.GetComponent<LineRenderer>();
        candidateTextCompLeft = candidateTextLeft.GetComponent<TextMesh>();
        candidateTextCompMiddle = candidateTextMiddle.GetComponent<TextMesh>();
        candidateTextCompRight = candidateTextRight.GetComponent<TextMesh>();
        inputSentenceComp = inputSentenceObj.GetComponent<TextMesh>();
        promptTextComp = promptTextObj.GetComponent<TextMesh>();
        wordNotFoundComp = wordNotFoundObj.GetComponent<TextMesh>();
        experimentStartTextComp = experimentStartTextObj.GetComponent<TextMesh>();
        experimentEndTextComp = experimentEndTextObj.GetComponent<TextMesh>();
        // deleteKeyComp = deleteKeyObg.GetComponent<TextMesh>();
        // NextKeyComp = NextKeyObg.GetComponent<TextMesh>();
        ExampleSentenceComp = ExampleSentenceObg.GetComponent<TextMesh>();

        resetWord();

        // 候補ワードの初期値をセットする
        initCandidateTexts();
        experimentStartTextComp.color = Color.yellow;

        // 視線用LineRendererのセッティング
        leftContorollerLineComp.widthMultiplier = 0.01f;
        rightContorollerLineComp.widthMultiplier = 0.01f;

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
        inputSentenceComp.text = string.Join("", InputSentence);
    }

    private void DelWord() {
        if (InputSentence.Count > 0) InputSentence.RemoveAt(InputSentence.Count - 1);
        inputSentenceComp.text = string.Join("", InputSentence);
    }

    private void resetWord() {
        InputSentence = new List<string>();
    }

    // Update is called once per frame
    void Update() {

        if (leftContorollerObj == null || rightContorollerObj == null) return;

        // 左右のコントローラーの位置と方向を取得
        Vector3 leftContorollerPos = leftContorollerObj.transform.position;
        Vector3 rightContorollerPos = rightContorollerObj.transform.position;
        Vector3 leftContorollerDir = leftContorollerObj.transform.TransformDirection(Vector3.forward);
        Vector3 rightContorollerDir = rightContorollerObj.transform.TransformDirection(Vector3.forward);

        // 左右のコントローラーから出る光線の位置と方向を設定する
        leftContorollerLineComp.SetPosition(0, leftContorollerPos); leftContorollerLineComp.SetPosition(1,  leftContorollerPos + leftContorollerDir * 50);
        rightContorollerLineComp.SetPosition(0, rightContorollerPos); rightContorollerLineComp.SetPosition(1,  rightContorollerPos + rightContorollerDir * 50);

        // 候補ワードの色を黒に戻す
        candidateTextCompLeft.color = candidateTextCompMiddle.color = candidateTextCompRight.color = Color.black;
        // 削除キーの色を黒に戻す
        // deleteKeyComp.color = Color.black;
        // Nextキーの色を黒に戻す
        // NextKeyComp.color = Color.black;

        // Nextキー（Trigger）が押された
        // 両手同時押しは完全に同タイミングに合わせるのが難しいので下記の条件にする
        if ((QuestContorollarAccessor.GetDownLeftPrimaryIndexTrigger() && QuestContorollarAccessor.GetRightPrimaryIndexTrigger()) || 
            (QuestContorollarAccessor.GetLeftPrimaryIndexTrigger() && QuestContorollarAccessor.GetDownRightPrimaryIndexTrigger())) {
            ExampleSentenceComp.text = "";
            initCandidateTexts();
            if (exampleInd <= exapleSentencesList.Count) {
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                float elapsedSeconds = (float)(ts.Hours * 60 * 60 + ts.Minutes * 60 + ts.Seconds) + (float)(ts.Milliseconds) / 1000.0f;

                // 入力されたテキストと経過時間をリストに保存する
                inputDataList.data.Add(
                    new acquiredData() {
                        result_text = inputSentenceComp.text,
                        elapsed_time = elapsedSeconds.ToString()
                    }
                );
                inputSentenceComp.text = ""; // 入力されたテキストをテキストUIから消す
                resetWord(); // ワードを保持しているリストを初期化する

                // 次のテキストへの更新の場合
                if (exampleInd < exapleSentencesList.Count) {
                    ExampleSentenceComp.text = exapleSentencesList[exampleInd];
                    promptTextComp.color = Color.gray;

                    // タイマーをリセットしてスタートする
                    sw.Reset();
                    sw.Start();
                
                // 最後の処理
                } else if (exampleInd == exapleSentencesList.Count) {
                    experimentEndTextComp.color = Color.yellow;
                    // ネットワーク処理の呼び出し
                    // StartCoroutine("CallServerToRegisterData");
                    StartCoroutine(GestureTypingSystemClient.CallServerToRegisterData(inputDataList));
                }
                
                exampleInd += 1;
            }
        }


        // 削除キー（B）が押された
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) {
            DelWord();
        }


        // 以下、RayCastを行う
        Ray leftrRay = new Ray(leftContorollerPos, leftContorollerDir);
        Ray rightRay = new Ray(rightContorollerPos, rightContorollerDir);
        RaycastHit leftHit;
        RaycastHit rightHit;

        bool isLeftHit = Physics.Raycast(leftrRay, out leftHit, 100);
        bool isRightHit = Physics.Raycast(rightRay, out rightHit, 100);
        if (isLeftHit || isRightHit)
        {
            // ヒットしたGameObjectを取得
            GameObject leftGameObj = null;
            if (isLeftHit) leftGameObj = leftHit.collider.gameObject;
            GameObject rightGameObj = null;
            if (isRightHit) rightGameObj = rightHit.collider.gameObject;

            Debug.Log("leftGameObj: " + leftGameObj); // for debug
            Debug.Log("rightGameObj: " + rightGameObj); // for debug
            Debug.Log("left hit point: " + leftHit.point); // for debug
            Debug.Log("right hit point: " + rightHit.point); // for debug
            
            // 左ポインタの生成
            GameObject leftSphere = null;
            Renderer leftPointerRenderer = null;
            if (isLeftHit) {
                leftSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leftSphere.transform.position = leftHit.point;
                leftSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // ポインタの大きさを設定
                leftPointerRenderer = leftSphere.GetComponent<Renderer>();
                leftPointerRenderer.material.color = Color.magenta; // ポインタの色を設定
                SphereCollider sphereCollider = leftSphere.GetComponent<SphereCollider>();
                if (sphereCollider != null) sphereCollider.enabled = false; // コライダーを無効化する
            }

            // 右ポインタの生成
            GameObject rightSphere = null;
            Renderer rightPointerRenderer = null;
            if (isRightHit) {
                rightSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rightSphere.transform.position = rightHit.point;
                rightSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // ポインタの大きさを設定
                rightPointerRenderer = rightSphere.GetComponent<Renderer>();
                rightPointerRenderer.material.color = Color.magenta; // ポインタの色を設定
                SphereCollider sphereCollider = rightSphere.GetComponent<SphereCollider>();
                if (sphereCollider != null) sphereCollider.enabled = false; // コライダーを無効化する
            }


            // 候補ワードである場合
            if ((isLeftHit && leftGameObj != null && leftGameObj.CompareTag("CandidateWordLeft")) || 
                (isRightHit && rightGameObj != null && rightGameObj.CompareTag("CandidateWordLeft"))) {
                Debug.Log("word text hit!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! left");
                candidateTextCompLeft.color = Color.red;

                if (isRightHit && rightGameObj.CompareTag("CandidateWordLeft") && QuestContorollarAccessor.GetDownAButton())  {
                    addWord(rightGameObj.GetComponentInChildren<TextMesh>().text);
                    initCandidateTexts();
                    promptTextComp.color = Color.gray;
                } else if(isLeftHit && leftGameObj.CompareTag("CandidateWordLeft") && QuestContorollarAccessor.GetDownXButton()) {
                    addWord(leftGameObj.GetComponentInChildren<TextMesh>().text);
                    initCandidateTexts();
                    promptTextComp.color = Color.gray;
                }

            // 候補ワードである場合
            } else if ((isLeftHit && leftGameObj.CompareTag("CandidateWordMiddle")) || 
                        (isRightHit && rightGameObj.CompareTag("CandidateWordMiddle"))) {
                Debug.Log("word text hit!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! middle");
                candidateTextCompMiddle.color = Color.red;

               if (isRightHit && rightGameObj.CompareTag("CandidateWordMiddle") && QuestContorollarAccessor.GetDownAButton())  {
                    addWord(rightGameObj.GetComponentInChildren<TextMesh>().text);
                    initCandidateTexts();
                    promptTextComp.color = Color.gray;
                } else if(isLeftHit && leftGameObj.CompareTag("CandidateWordMiddle") && QuestContorollarAccessor.GetDownXButton()) {
                    addWord(leftGameObj.GetComponentInChildren<TextMesh>().text);
                    initCandidateTexts();
                    promptTextComp.color = Color.gray;
                }
            
            // 候補ワードである場合
            } else if ((isLeftHit && leftGameObj.CompareTag("CandidateWordRight")) || 
                        (isRightHit && rightGameObj.CompareTag("CandidateWordRight"))) {
                Debug.Log("word text hit!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! right");
                candidateTextCompRight.color = Color.red;

                if (isRightHit && rightGameObj.CompareTag("CandidateWordRight") && QuestContorollarAccessor.GetDownAButton())  {
                    addWord(rightGameObj.GetComponentInChildren<TextMesh>().text);
                    initCandidateTexts();
                    promptTextComp.color = Color.gray;
                } else if(isLeftHit && leftGameObj.CompareTag("CandidateWordRight") && QuestContorollarAccessor.GetDownXButton()) {
                    addWord(leftGameObj.GetComponentInChildren<TextMesh>().text);
                    initCandidateTexts();
                    promptTextComp.color = Color.gray;
                }

            // スペースキーである場合
            } else if ((isLeftHit && leftGameObj.CompareTag("SpaceKey")) || 
                        (isRightHit && rightGameObj.CompareTag("SpaceKey"))) {
                if (QuestContorollarAccessor.GetDownAButton() || QuestContorollarAccessor.GetDownXButton()) {
                    addWord(" ");
                }

            // キーボードUIである場合
            } else {
                if ((isLeftHit && leftGameObj.CompareTag("KeyBoard")) || 
                    (isRightHit && rightGameObj.CompareTag("KeyBoard"))) {

                    if (QuestContorollarAccessor.GetDownAButton()) {
                        Debug.Log(rightHit.point.x + ", " + rightHit.point.y); // for debug
                            // ヒット座標を画像のピクセル座標に変換
                        Vector2 hitPosForPixel = HitPos2PixcelPos(rightHit.point.x, rightHit.point.y);
                        Debug.Log("xHitPosForPixel: " + hitPosForPixel.x); // for debug
                        Debug.Log("yHitPosForPixel: " + hitPosForPixel.y); // for debug

                        addWord(PredictAlphabet(hitPosForPixel));
                    }

                    if (QuestContorollarAccessor.GetDownXButton()) {
                        Debug.Log(leftHit.point.x + ", " + leftHit.point.y); // for debug
                            // ヒット座標を画像のピクセル座標に変換
                        Vector2 hitPosForPixel = HitPos2PixcelPos(leftHit.point.x, leftHit.point.y);
                        Debug.Log("xHitPosForPixel: " + hitPosForPixel.x); // for debug
                        Debug.Log("yHitPosForPixel: " + hitPosForPixel.y); // for debug

                        addWord(PredictAlphabet(hitPosForPixel));
                    }       
                }
            }

            // 古いポインタがある場合、そのポインタの廃棄を行う
            // if (beforePointer != null) {
            //     Destroy(beforePointer);
            // }
            // beforePointer = Sphere;

            if (leftBeforePointer != null) {
                Destroy(leftBeforePointer);
            }
            leftBeforePointer = leftSphere;

            if (rightBeforePointer != null) {
                Destroy(rightBeforePointer);
            }
            rightBeforePointer = rightSphere;

        } else {
            // 視線がUIとヒットしない場合、ポインタを消す
            // if (beforePointer != null) {
            //     Destroy(beforePointer);
            // }

            if (leftBeforePointer != null) {
                Destroy(leftBeforePointer);
            }

            if (rightBeforePointer != null) {
                Destroy(rightBeforePointer);
            }
        }
    }


    // 取得した座標をgesture用に変換する
    private Vector2 HitPos2PixcelPos(float x, float y) {
        float xHitPosForPixel = (x - leftUpperPos.x) / pictureSizeForWorld.x * boardPixelWidth;
        float yHitPosForPixel = (leftUpperPos.y - y) / pictureSizeForWorld.y * boardPixelHeight;
        return new Vector2(xHitPosForPixel, yHitPosForPixel);
    }


    private string PredictAlphabet(Vector2 coord) {
        double min_gosa = Math.Pow(10.0, 10.0);
        string best_alpha = "a";
        for (int i = 0; i < centroids_X.Count(); i++) {
            double gosa = Math.Pow(coord.x - centroids_X[i], 2) + Math.Pow(coord.y - centroids_Y[i], 2);
            if (gosa < min_gosa) {
                min_gosa = gosa;
                best_alpha = ((char)(i + 97)).ToString();
            }
        }
        return best_alpha;
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
}
