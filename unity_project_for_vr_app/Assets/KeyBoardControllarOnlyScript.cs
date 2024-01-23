using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Oculus.Interaction.DistanceReticles;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using TMPro;

public class KeyBoardControllarOnlyScript : MonoBehaviour
{
    public GameObject keysParent;
    public Transform eyeTransform;
    public Transform leftContorollerTransform;
    public Transform rightContorollerTransform;
    public LineRenderer leftContorollerLineComp;
    public LineRenderer rightContorollerLineComp;
    public GameObject keyBoardBaseObj;
    public TMP_Text exampleText;
    public TMP_Text enteredText;
    public TMP_Text guessedTextCompLeft, guessedTextCompMiddle, guessedTextCompRight;
    public AudioSource hitAudio;
    public AudioSource selectAudio;

    private KeyScript[] keys;
    // private KeyScript? lastSelectedKey = null;
    private KeyScript? lastSelectedKeyRight = null;
    private KeyScript? lastSelectedKeyLeft = null;

    // private string lastSelectedCandidateKey = null;
    private string lastSelectedCandidateKeyLeft = null;
    private string lastSelectedCandidateKeyRight = null;

    private Vector3[] localCorners;
    private float boardWidth, boardHeight;

    // キーボードのキーの状態変数
    private bool isUsingUppercase = false;
    private bool isUsingSecondary = false;

    // 視線のヒット座標を保持するためのデータ構造
    private pressedCoordinates pressedCoordinatesForKeyBoardGesture;

    // for debug
    GameObject[] cornerSpheres = new GameObject[4];

    private LineRenderer lineRendererOnKeyBoard;

    // ポインタを保持するプロパティ
    private GameObject leftBeforePointer;
    private GameObject rightBeforePointer;

    private List<string> exapleSentencesList = ExampleSentences.ControllerOnlyExampleSentencesForExperiment; // 例文のリスト
    private int exampleInd = 0; // 表示するExampleのインデックス

    private List<string> InputSentence = new List<string>(); // 入力されたテキストを保持するリスト
    private List<string> inputKeys = new List<string>(); // 入力された全てのキーを保持するリスト

    private string[] guessedWordList;

    // 取得したデータを保持するリスト
    private acquiredDataList inputDataList = new acquiredDataList();

    private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch(); // ストップウォッチ
    private bool isFirstInput = true;

    int[] centroids_X = new int[]{99,419,280,245,193,318,388,457,546,527,598,669,567,491,619,692,47,265,173,335,474,350,121,210,405,138};
    int[] centroids_Y = new int[]{96,156,156,96,37,96,96,96,37,96,96,96,156,156,37,37,37,37,96,37,37,156,37,156,37,156};

    private bool isLeftInput = false;
    private bool isRightInput = false;


    // #############################################################################################################################################
    // #############################################################################################################################################

    private void ClearSelection() {
        foreach (KeyScript key in keys) {
            key.SetSelected(false);
        }
    }

    private void SetUseSecondary(bool useSecondary) {
        this.isUsingSecondary = useSecondary;
        foreach (KeyScript key in keys) {
            key.SetUseSecondary(useSecondary);
        }
    }

    private void SetUseUppercase(bool useUppercase) {
        this.isUsingUppercase = useUppercase;
        foreach (KeyScript key in keys) {
            key.SetUseUppercase(useUppercase);
        }
    }

    // キーボードの四隅のワールド座標を取得する
    private Vector3[] GetKeyBoardLocalCorners() {
        // ローカルとワールドにおけるボードのScaleを用いたコーナー座標の取得
        Vector3 boardScale = keyBoardBaseObj.transform.localScale;
        Vector3 localBoardx = new Vector3(1, 0, 0) * boardScale.x;
        Vector3 localBoardz = new Vector3(0, 0, 1) * boardScale.z;
        Vector3 leftUpper = -localBoardx / 2 + localBoardz / 2; // ここにローカル座標がある
        Vector3 rightUpper = localBoardx / 2 + localBoardz / 2; // ここにローカル座標がある
        Vector3 rightUnder = localBoardx / 2 - localBoardz / 2; // ここにローカル座標がある
        Vector3 leftUnder = -localBoardx / 2 - localBoardz / 2; // ここにローカル座標がある
        Vector3[] corners = new Vector3[]{leftUpper, rightUpper, rightUnder, leftUnder};

        return corners;
    }

    private Vector3[] ConvToKeyBoardWorldCorners(Vector3[] localCorners) {
        return new Vector3[]
            {
                gameObject.transform.TransformPoint(localCorners[0]), 
                gameObject.transform.TransformPoint(localCorners[1]), 
                gameObject.transform.TransformPoint(localCorners[2]), 
                gameObject.transform.TransformPoint(localCorners[3]), 
            };
    }

    private float StandardizationXFromLocal(float x) {
        return x + boardWidth / 2;
    }

    private float StandardizationYFromLocal(float y) {
        return boardHeight - (y + boardHeight / 2);
    }

    private void AdjustKeyBoardAltitude() {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, eyeTransform.position.y - 0.1f, gameObject.transform.position.z);
    }

    private void ResetGuessedTexts() {
        guessedTextCompLeft.text = guessedTextCompMiddle.text = guessedTextCompRight.text = "";
        guessedTextCompLeft.color = guessedTextCompMiddle.color = guessedTextCompRight.color = Color.black;
    }

    private void ResetInputText() {
        InputSentence = new List<string>();
        inputKeys = new List<string>();
    }

    private void DelWord() {
        if (InputSentence.Count > 0) InputSentence.RemoveAt(InputSentence.Count - 1);
        enteredText.text = string.Join("", InputSentence);
        inputKeys.Add("<");
    }

    private void AddWord(string word) {
        InputSentence.Add(word);
        inputKeys.Add(word);
        enteredText.text = string.Join("", InputSentence);
    }

    // 単語の候補をリセットする
    private void ResetGuessedWordList() {
        guessedWordList = new string[0];
    }

    private int UpdateCandidatePlane(string[] candidateWords) {
        ResetGuessedTexts(); // 表示している単語の候補をリセットする

        // ワードが見つからなかった場合
        if (candidateWords.Length == 3 && candidateWords[0] == "Word" && candidateWords[1] == "not" && candidateWords[2] == "found") {
            guessedTextCompLeft.text = "Word not fount";
            ResetGuessedWordList(); // 内部で保持している候補単語の配列を初期化する
            return 1;
        }
        
        guessedWordList = candidateWords; // 内部で保持している候補単語の配列に推論結果を入れる
    
        guessedTextCompLeft.text = candidateWords[0];
        if (candidateWords.Length > 1) guessedTextCompMiddle.text = candidateWords[1];
        if (candidateWords.Length > 2) guessedTextCompRight.text = candidateWords[2];

        return 0;
    }

    private void checkTimerStart() {
        if (isFirstInput) {
            isFirstInput = false;
            sw.Start();
        }
    }

    private string PredictAlphabet(Vector2 coord) {
        double min_gosa = Math.Pow(10.0, 10.0);
        string best_alpha = "a";
        for (int i = 0; i < centroids_X.Length; i++) {
            double gosa = Math.Pow(coord.x - centroids_X[i], 2) + Math.Pow(coord.y - centroids_Y[i], 2);
            if (gosa < min_gosa) {
                min_gosa = gosa;
                best_alpha = ((char)(i + 97)).ToString();
            }
        }
        return best_alpha;
    }


    // #############################################################################################################################################
    // #############################################################################################################################################

    // Start is called before the first frame update
    void Start()
    {
        // 入力手法を記録
        inputDataList.input_method = "ControllarOnly";

        // 全キーのスクリプトコンポーネントを取得
        keys = keysParent.GetComponentsInChildren<KeyScript>();

        // コントローラからの光線の太さを設定
        leftContorollerLineComp.widthMultiplier = 0.001f;
        rightContorollerLineComp.widthMultiplier = 0.001f;

        // キーの初期状態を設定
        ClearSelection();
        SetUseUppercase(false);
        SetUseSecondary(false);

        // キーボードの高さを調整する
        // キーボードの高さを参照する処理があるのでStartでも実行する
        AdjustKeyBoardAltitude();

        // ローカル座標系のキーボードのコーナー座標を取得
        localCorners = GetKeyBoardLocalCorners();
        // キーボードのローカル幅を取得
        boardWidth = localCorners[1].x - localCorners[0].x;
        boardHeight = localCorners[1].z - localCorners[2].z;
        Debug.Log("boardWidth: " + boardWidth);
        Debug.Log("boardHeight: " + boardHeight);
        
        // for debug
        // 四隅にSphereを出す
        // Vector3[] worldCorners = ConvToKeyBoardWorldCorners(localCorners);
        // for (int i = 0; i < 4; i++) {
        //     cornerSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //     cornerSpheres[i].transform.position = worldCorners[i];
        //     cornerSpheres[i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        // }

        // キーの座標を取得する
        Dictionary<string, float[]> keysPos = new Dictionary<string, float[]>();
        foreach(KeyScript key in keys) {
            string keyText = key.keyCharacter;
            Vector3 keyWorldPos = key.GetKeyWorldPos();
            Vector3 keyLocalPos = gameObject.transform.InverseTransformPoint(keyWorldPos);
            keysPos[keyText] = new float[]{keyLocalPos.x, keyLocalPos.z};
        }

        float[] xList = new float[26];
        float[] yList = new float[26];
        int j = 0;
        for (char c = 'a'; c <= 'z'; c++) {
            xList[j] = keysPos[c.ToShortString()][0];
            yList[j] = keysPos[c.ToShortString()][1];
            j++;
        }
        Debug.Log("xList: " + String.Join(",", xList));
        Debug.Log("yList: " + String.Join(",", yList));

        int[] xList2 = new int[26];
        int[] yList2 = new int[26];
        for (int i = 0; i < 26; i++) {
            xList2[i] = (int)(StandardizationXFromLocal(xList[i]) * 1000);
            yList2[i] = (int)(StandardizationYFromLocal(yList[i]) * 1000);
        }
        Debug.Log("xList2: " + String.Join(",", xList2));
        Debug.Log("yList2: " + String.Join(",", yList2));



        // ローカルで座標を計算したい
        // hit座標はワールドからローカルに変換する　その時のy座標を無視すれば良い
        // ローカルでのコーナー座標が必要
        // コーナー座標はローカル系のまま出せない？　localScaleを使って。　ボードローカル座標のコーナー座標はlocalScale使えば簡単にだせそう。
        // デバッグ用コーナーSphereの表示のみワールドに変換して行う


        lineRendererOnKeyBoard = gameObject.GetComponent<LineRenderer>();

        // 筆跡用LineRendererのセッティング
        lineRendererOnKeyBoard.startWidth = 0.005f; // 開始点の太さを0.1にする
        lineRendererOnKeyBoard.endWidth = 0.005f; // 終了点の太さを0.1にする
        lineRendererOnKeyBoard.startColor = Color.red;
        lineRendererOnKeyBoard.endColor = Color.red;

        // ストップウォッチの初期化とスタート
        sw.Reset();
        // sw.Start();

        // 入力されたテキストをリセットする
        ResetInputText();
        // 表示されている候補ワードをリセットする
        ResetGuessedTexts();
        // 内部で保持している候補ワードをリセットする
        ResetGuessedWordList();
    }

    // Update is called once per frame
    void Update()
    {
        // キーボードの高さを調整する
        AdjustKeyBoardAltitude();

        // 候補ワードの色を黒に戻す
        guessedTextCompLeft.color = guessedTextCompMiddle.color = guessedTextCompRight.color = Color.black;
        
        // キーが選択されていた場合、状態を初期化する
        // if (lastSelectedKey != null) {
        //     lastSelectedKey.SetSelected(false);
        // }
        if (lastSelectedKeyRight != null) {
            lastSelectedKeyRight.SetSelected(false);
        }
        if (lastSelectedKeyLeft != null) {
            lastSelectedKeyLeft.SetSelected(false);
        }


        // ワールド座標系コーナー座標の取得
        Vector3[] worldCorners = ConvToKeyBoardWorldCorners(localCorners);
        // for debug
        // 四隅を出す
        // for (int i = 0; i < 4; i++) {
        //     cornerSpheres[i].transform.position = worldCorners[i];
        // }

        // コントローラからの光線を打つ
        leftContorollerLineComp.SetPosition(0, leftContorollerTransform.position); 
        leftContorollerLineComp.SetPosition(1,  leftContorollerTransform.position + leftContorollerTransform.forward * 50);
        rightContorollerLineComp.SetPosition(0, rightContorollerTransform.position); 
        rightContorollerLineComp.SetPosition(1,  rightContorollerTransform.position + rightContorollerTransform.forward * 50);


        // Nextキー（Trigger）が押された
        if ((QuestContorollarAccessor.GetDownLeftPrimaryIndexTrigger() && QuestContorollarAccessor.GetRightPrimaryIndexTrigger()) || 
            (QuestContorollarAccessor.GetLeftPrimaryIndexTrigger() && QuestContorollarAccessor.GetDownRightPrimaryIndexTrigger())) {
            exampleText.text = ""; // 例文をリセット
            ResetGuessedTexts(); // 候補ワードをリセット
            if (exampleInd <= exapleSentencesList.Count) {
                sw.Stop(); // ストップウォッチを停止
                TimeSpan ts = sw.Elapsed;
                double elapsedSeconds = (double)(ts.Hours * 60 * 60 + ts.Minutes * 60 + ts.Seconds) + (double)(ts.Milliseconds) / 1000.0f;

                // 入力されたテキストと経過時間をリストに保存する
                inputDataList.data.Add(
                    new acquiredData() {
                        result_text = enteredText.text,
                        input_keys = String.Join("", inputKeys),
                        elapsed_time = elapsedSeconds.ToString()
                    }
                );
                enteredText.text = ""; // 入力されたテキストをテキストUIから消す
                ResetInputText(); // ワードを保持しているリストを初期化する

                // 次のテキストへの更新の場合
                if (exampleInd < exapleSentencesList.Count) {
                    exampleText.text = exapleSentencesList[exampleInd];
                    // promptTextComp.color = Color.gray;

                    // タイマーをリセットしてスタートする
                    sw.Reset();
                    // sw.Start();
                    // タイマー用のフラグをリセット
                    isFirstInput = true;
                
                // 最後の処理
                } else if (exampleInd == exapleSentencesList.Count) {
                    // experimentEndTextComp.color = Color.yellow;
                    // ネットワーク処理の呼び出し
                    // StartCoroutine("CallServerToRegisterData");
                    StartCoroutine(GestureTypingSystemClient.CallServerToRegisterData(inputDataList));
                }
                
                exampleInd += 1;
            }
        }

        // 削除キー（B）が押された
        // if (QuestContorollarAccessor.GetDownBButton()) {
        //     DelWord();
        // }


        // 以下、RayCastを行う
        Ray leftrRay = new Ray(leftContorollerTransform.position, leftContorollerTransform.forward);
        Ray rightRay = new Ray(rightContorollerTransform.position, rightContorollerTransform.forward);
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
                leftSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f); // ポインタの大きさを設定
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
                rightSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f); // ポインタの大きさを設定
                rightPointerRenderer = rightSphere.GetComponent<Renderer>();
                rightPointerRenderer.material.color = Color.magenta; // ポインタの色を設定
                SphereCollider sphereCollider = rightSphere.GetComponent<SphereCollider>();
                if (sphereCollider != null) sphereCollider.enabled = false; // コライダーを無効化する
            }

            KeyScript? leftKey = null;
            KeyScript? rightKey = null;

            // キーの色を更新
            if (isLeftHit) {
                leftKey = leftHit.transform.parent.GetComponent<KeyScript>();

                if (leftKey != null) {
                    leftKey.SetSelected(true);

                    if (lastSelectedKeyLeft != null && lastSelectedKeyLeft.keyCharacter == leftKey.keyCharacter) {

                    } else {
                        hitAudio.Play(); // 音を出す
                        lastSelectedKeyLeft = leftKey;
                    }
                }
            }
            if (isRightHit) {
                rightKey = rightHit.transform.parent.GetComponent<KeyScript>();

                if (rightKey != null) {
                    rightKey.SetSelected(true);

                    if (lastSelectedKeyRight != null && lastSelectedKeyRight.keyCharacter == rightKey.keyCharacter) {

                    } else {
                        hitAudio.Play(); // 音を出す
                        lastSelectedKeyRight = rightKey;
                    }
                }
            }

            // 候補ワードに当てた場合
            // 視覚的と音のフィードバックを返す
            if (isLeftHit && leftGameObj.CompareTag("CandidateWordLeft")) {
                guessedTextCompLeft.color = Color.red;
                if (lastSelectedCandidateKeyLeft != "right_candidate") {
                    hitAudio.Play(); // 音を出す
                    lastSelectedCandidateKeyLeft = "right_candidate";
                }
            }
            if (isRightHit && rightGameObj.CompareTag("CandidateWordLeft")) {
                guessedTextCompLeft.color = Color.red;
                if (lastSelectedCandidateKeyRight != "right_candidate") {
                    hitAudio.Play(); // 音を出す
                    lastSelectedCandidateKeyRight = "right_candidate";
                }
            }
            if (isLeftHit && leftGameObj.CompareTag("CandidateWordMiddle")) {
                guessedTextCompMiddle.color = Color.red;
                if (lastSelectedCandidateKeyLeft != "middle_candidate") {
                    hitAudio.Play(); // 音を出す
                    lastSelectedCandidateKeyLeft = "middle_candidate";
                }
            }
            if (isRightHit && rightGameObj.CompareTag("CandidateWordMiddle")) {
                guessedTextCompMiddle.color = Color.red;
                if (lastSelectedCandidateKeyRight != "middle_candidate") {
                    hitAudio.Play(); // 音を出す
                    lastSelectedCandidateKeyRight = "middle_candidate";
                }
            }
            if (isLeftHit && leftGameObj.CompareTag("CandidateWordRight")) {
                guessedTextCompRight.color = Color.red;
                if (lastSelectedCandidateKeyLeft != "left_candidate") {
                    hitAudio.Play(); // 音を出す
                    lastSelectedCandidateKeyLeft = "left_candidate";
                }
            }
            if (isRightHit && rightGameObj.CompareTag("CandidateWordRight")) {
                guessedTextCompRight.color = Color.red;
                if (lastSelectedCandidateKeyRight != "left_candidate") {
                    hitAudio.Play(); // 音を出す
                    lastSelectedCandidateKeyRight = "left_candidate";
                }
            }

            // 候補ワードを選択した場合の場合わけはなし

            // キーボードを選択した場合
            if ((isRightHit && rightGameObj.CompareTag("KeyBoard")) || 
                        (isLeftHit && leftGameObj.CompareTag("KeyBoard"))) {
                // lastSelectedCandidateKey = null; // 候補の選択肢以外に視線が当たっている場合
                
                lastSelectedCandidateKeyRight = null;
                lastSelectedCandidateKeyLeft = null;

                if (isRightHit && rightGameObj.CompareTag("KeyBoard") && QuestContorollarAccessor.GetDownAButton()) {
                    checkTimerStart(); // 最初の入力文字であればタイマーをスタートする

                    // 特殊キーの場合の処理
                    if (rightKey != null && rightKey.keyCharacter == "Backspace") {
                        DelWord();
                    
                    } else if(rightKey != null && rightKey.uppercaseKeyCharacter == "Space") {
                        AddWord("⎵");

                    // ジェスチャーの始まりの場合
                    } else {
                        // キーボードのローカル座標系のヒット座標
                        Vector3 hitLocalPos = gameObject.transform.InverseTransformPoint(rightHit.point);

                        Debug.Log(rightHit.point.x + ", " + rightHit.point.y); // for debug
                            
                        // ヒット座標を画像のピクセル座標に変換
                        float xHitPosForPixel = StandardizationXFromLocal(hitLocalPos.x) * 1000;
                        float yHitPosForPixel = StandardizationYFromLocal(hitLocalPos.z) * 1000;
                        Debug.Log("xHitPosForPixel: " + xHitPosForPixel); // for debug
                        Debug.Log("yHitPosForPixel: " + yHitPosForPixel); // for debug

                        AddWord(PredictAlphabet(new Vector2(xHitPosForPixel, yHitPosForPixel)));
                    }
                }

                if (isLeftHit && leftGameObj.CompareTag("KeyBoard") && QuestContorollarAccessor.GetDownXButton()) {
                    checkTimerStart(); // 最初の入力文字であればタイマーをスタートする

                    // 特殊キーの場合の処理
                    if (leftKey != null && leftKey.keyCharacter == "Backspace") {
                        DelWord();
                    
                    } else if(leftKey != null && leftKey.uppercaseKeyCharacter == "Space") {
                        AddWord("⎵");

                    // ジェスチャーの始まりの場合
                    } else {
                        // キーボードのローカル座標系のヒット座標
                        Vector3 hitLocalPos = gameObject.transform.InverseTransformPoint(leftHit.point);

                        Debug.Log(leftHit.point.x + ", " + leftHit.point.y); // for debug
                            
                        // ヒット座標を画像のピクセル座標に変換
                        float xHitPosForPixel = StandardizationXFromLocal(hitLocalPos.x) * 1000;
                        float yHitPosForPixel = StandardizationYFromLocal(hitLocalPos.z) * 1000;
                        Debug.Log("xHitPosForPixel: " + xHitPosForPixel); // for debug
                        Debug.Log("yHitPosForPixel: " + yHitPosForPixel); // for debug

                        AddWord(PredictAlphabet(new Vector2(xHitPosForPixel, yHitPosForPixel)));
                    }
                }

            } else {
                // lastSelectedCandidateKey = null; // 候補の選択肢以外に視線が当たっている場合
                // lastSelectedCandidateKeyLeft = null;
                // lastSelectedCandidateKeyRight = null;
            }

            // 古いポインタがある場合、そのポインタの廃棄を行う
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
            if (leftBeforePointer != null) {
                Destroy(leftBeforePointer);
            }

            if (rightBeforePointer != null) {
                Destroy(rightBeforePointer);
            }
        }
    }
}
