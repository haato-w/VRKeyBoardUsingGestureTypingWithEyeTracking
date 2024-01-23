using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Oculus.Interaction.DistanceReticles;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using TMPro;

public class KeyBoardScript : MonoBehaviour
{
    public GameObject keysParent;
    public Transform eyeTransform;
    public LineRenderer eyeLineRenderer;
    public GameObject keyBoardBaseObj;
    public TMP_Text exampleText;
    public TMP_Text enteredText;
    public TMP_Text guessedTextCompLeft, guessedTextCompMiddle, guessedTextCompRight;
    public AudioSource hitAudio;
    public AudioSource selectAudio;

    private KeyScript[] keys;
    private KeyScript? lastSelectedKey = null;

    private string lastSelectedCandidateKey = null;

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

    private GameObject beforePointer; // ポインタを保持するプロパティ

    private List<string> exapleSentencesList = ExampleSentences.EyeGazeGestureExampleSentencesForExperiment; // 例文のリスト
    private int exampleInd = 0; // 表示するExampleのインデックス

    private List<string> InputSentence = new List<string>(); // 入力されたテキストを保持するリスト
    private List<string> inputKeys = new List<string>(); // 入力された全てのキーを保持するリスト
    private List<pressedCoordinates> inputGestures = new List<pressedCoordinates>(); // 入力された全てのキーに対応するジェスチャーを保持するリスト

    private string[] guessedWordList;

    // 取得したデータを保持するリスト
    private acquiredDataList inputDataList = new acquiredDataList();

    private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch(); // ストップウォッチ
    private bool isFirstInput = true;

    int[] centroids_X = new int[]{99,419,280,245,193,318,388,457,546,527,598,669,567,491,619,692,47,265,173,335,474,350,121,210,405,138};
    int[] centroids_Y = new int[]{96,156,156,96,37,96,96,96,37,96,96,96,156,156,37,37,37,37,96,37,37,156,37,156,37,156};


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
        // 一文字ずつ追加する　一文字ずつ消すため
        for (int i = 0; i < word.Length; i++) {
            InputSentence.Add(word[i].ToString());
            inputKeys.Add(word[i].ToString());
        }
        InputSentence.Add("⎵");
        inputKeys.Add("⎵");
        enteredText.text = String.Join("", InputSentence);
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


    // #############################################################################################################################################
    // #############################################################################################################################################

    // Start is called before the first frame update
    void Start()
    {
        // 入力手法を記録
        inputDataList.input_method = "EyeGazeGesture";

        // 全キーのスクリプトコンポーネントを取得
        keys = keysParent.GetComponentsInChildren<KeyScript>();

        // デバッグ用視線の太さを設定
        eyeLineRenderer.widthMultiplier = 0.001f;
        eyeLineRenderer.SetPosition(0, new Vector3(5, 5, 5));

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
        if (lastSelectedKey != null) {
            lastSelectedKey.SetSelected(false);
        }

        // ワールド座標系コーナー座標の取得
        Vector3[] worldCorners = ConvToKeyBoardWorldCorners(localCorners);
        // for debug
        // 四隅を出す
        // for (int i = 0; i < 4; i++) {
        //     cornerSpheres[i].transform.position = worldCorners[i];
        // }


        // デバッグ用の視線を打つ
        // eyeLineRenderer.SetPosition(0, eyeTransform.position); eyeLineRenderer.SetPosition(1,  eyeTransform.position + eyeTransform.forward * 50);

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
                        series_coordinates = inputGestures, 
                        elapsed_time = elapsedSeconds.ToString()
                    }
                );
                enteredText.text = ""; // 入力されたテキストをテキストUIから消す
                ResetInputText(); // ワードを保持しているリストを初期化する
                inputGestures = new List<pressedCoordinates>(); // 入力された全てのキーに対応するジェスチャーを保持するリスト

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


        // レイを打つ
        RaycastHit hit;
        Ray ray = new Ray(eyeTransform.position, eyeTransform.forward);

        if (Physics.Raycast(ray, out hit, 100)) {

            // キーの色を更新
            KeyScript? key = hit.transform.parent.GetComponent<KeyScript>();

            if (key != null) {
                key.SetSelected(true);

                if (lastSelectedKey != null && lastSelectedKey.keyCharacter == key.keyCharacter) {

                } else {
                    hitAudio.Play(); // 音を出す
                    lastSelectedKey = key;
                }
            }



            // ポインタの生成
            // GameObject PSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // PSphere.transform.position = hit.point; // ポインタ位置を設定
            // PSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f); // ポインタの大きさを設定
            // Renderer pointerRenderer = PSphere.GetComponent<Renderer>();
            // pointerRenderer.material.color = Color.magenta; // ポインタの色を設定
            // SphereCollider sphereCollider = PSphere.GetComponent<SphereCollider>();
            // if (sphereCollider != null) sphereCollider.enabled = false; // コライダーを無効化する

            GameObject hitObject = hit.collider.gameObject;

            // 候補ワードである場合
            if (hitObject.CompareTag("CandidateWordLeft")) {
                Debug.Log("word text hit!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! left");
                guessedTextCompLeft.color = Color.red;
                if (lastSelectedCandidateKey != "right_candidate") {
                    hitAudio.Play(); // 音を出す
                    lastSelectedCandidateKey = "right_candidate";
                }

                if (QuestContorollarAccessor.GetUpAButton() && guessedWordList.Length >= 1)  {
                    checkTimerStart(); // 最初の入力文字であればタイマーをスタートする
                    selectAudio.Play(); // 選択肢を選んだ時の音を出す
                    AddWord(guessedWordList[0]);
                    ResetGuessedTexts();
                    ResetGuessedWordList();
                    // promptTextComp.color = Color.black;
                }

            // 候補ワードである場合
            } else if (hitObject.CompareTag("CandidateWordMiddle")) {
                Debug.Log("word text hit!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! middle");
                guessedTextCompMiddle.color = Color.red;
                if (lastSelectedCandidateKey != "middle_candidate") {
                    hitAudio.Play(); // 音を出す
                    lastSelectedCandidateKey = "middle_candidate";
                }

                if (QuestContorollarAccessor.GetUpAButton() && guessedWordList.Length >= 2)  {
                    checkTimerStart(); // 最初の入力文字であればタイマーをスタートする
                    selectAudio.Play(); // 選択肢を選んだ時の音を出す
                    AddWord(guessedWordList[1]);
                    ResetGuessedTexts();
                    ResetGuessedWordList();
                    // promptTextComp.color = Color.black;
                }
            
            // 候補ワードである場合
            } else if (hitObject.CompareTag("CandidateWordRight")) {
                Debug.Log("word text hit!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! right");
                guessedTextCompRight.color = Color.red;
                if (lastSelectedCandidateKey != "left_candidate") {
                    hitAudio.Play(); // 音を出す
                    lastSelectedCandidateKey = "left_candidate";
                }

                if (QuestContorollarAccessor.GetUpAButton() && guessedWordList.Length >= 3)  {
                    checkTimerStart(); // 最初の入力文字であればタイマーをスタートする
                    selectAudio.Play(); // 選択肢を選んだ時の音を出す
                    AddWord(guessedWordList[2]);
                    ResetGuessedTexts();
                    ResetGuessedWordList();
                    // promptTextComp.color = Color.black;
                }
            
            // スペースキーである場合
            // } else if (gameObject.CompareTag("SpaceKey")) {
            //     SpaceKeyTextComp.color = Color.red;
            //     if (QuestContorollarAccessor.GetDownAButton() || QuestContorollarAccessor.GetDownXButton()) {
            //         addWord(""); // ジェスチャーでは最後にスペースが入力されるのでから文字を送る
            //     }

            } else if (hit.collider.CompareTag("KeyBoard")) {
                lastSelectedCandidateKey = null; // 候補の選択肢以外に視線が当たっている場合

                // キーボードのローカル座標系のヒット座標
                Vector3 hitLocalPos = gameObject.transform.InverseTransformPoint(hit.point);

                // 入力モードに入ったタイミングで既存の筆跡を消す
                if (QuestContorollarAccessor.GetDownAButton())  {
                    checkTimerStart(); // 最初の入力文字であればタイマーをスタートする

                    lineRendererOnKeyBoard.positionCount = 0;

                    // 特殊キーの場合の処理
                    if (key != null && key.keyCharacter == "Backspace") {
                        DelWord();
                    
                    } else if(key != null && key.uppercaseKeyCharacter == "Space") {
                        AddWord("");

                    // ジェスチャーの始まりの場合
                    } else {
                        // 記録の初期化
                        pressedCoordinatesForKeyBoardGesture = new pressedCoordinates();

                        // 補正キー座標を追加
                        if (key != null) {
                            int a_ind = (int)'a'; int z_ind = (int)'z';
                            int key_int = key.keyCharacter.ToCharArray()[0];
                            if (a_ind <= key_int && key_int <= z_ind) {

                                Coordinate xyPairCoordinate = new Coordinate()
                                {
                                    x = centroids_X[key_int - a_ind],
                                    y = centroids_Y[key_int - a_ind]
                                };

                                pressedCoordinatesForKeyBoardGesture.data.Add(xyPairCoordinate);
                            }
                        }
                    }
                }

                // 入力モード時の処理
                if (QuestContorollarAccessor.GetAButton()) {
                    checkTimerStart(); // 最初の入力文字であればタイマーをスタートする

                    // ポインタの色を変更
                    // pointerRenderer.material.color = Color.green;
                    Debug.Log(hit.point.x + ", " + hit.point.y); // for debug
                    
                    // ヒット座標を画像のピクセル座標に変換
                    float xHitPosForPixel = StandardizationXFromLocal(hitLocalPos.x) * 1000;
                    float yHitPosForPixel = StandardizationYFromLocal(hitLocalPos.z) * 1000;
                    Debug.Log("xHitPosForPixel: " + xHitPosForPixel); // for debug
                    Debug.Log("yHitPosForPixel: " + yHitPosForPixel); // for debug
                    Coordinate xyPairCoordinate = new Coordinate()
                    {
                        x = xHitPosForPixel,
                        y = yHitPosForPixel
                    };
                    pressedCoordinatesForKeyBoardGesture.data.Add(xyPairCoordinate);
                    

                    Vector3 linePoint = gameObject.transform.TransformPoint(new Vector3(hitLocalPos.x, 0.01f, hitLocalPos.z));

                    // 筆跡の太さを一定にするために点を複数打つ
                    for (var i = 0; i < 3; i++) {
                        lineRendererOnKeyBoard.positionCount += 1;
                        lineRendererOnKeyBoard.SetPosition(lineRendererOnKeyBoard.positionCount - 1, linePoint);
                    }

                }

                // 入力モードが終了したときの処理
                if (QuestContorollarAccessor.GetUpAButton()) {
                    checkTimerStart(); // 最初の入力文字であればタイマーをスタートする

                    // 最後の補正座標は一旦やめる
                    // 補正キー座標を追加
                    // if (key != null) {
                    //     int a_ind = (int)'a'; int z_ind = (int)'z';
                    //     int key_int = key.keyCharacter.ToCharArray()[0];
                    //     if (a_ind <= key_int && key_int <= z_ind) {

                    //         Coordinate xyPairCoordinate = new Coordinate()
                    //         {
                    //             x = centroids_X[key_int - a_ind],
                    //             y = centroids_Y[key_int - a_ind]
                    //         };

                    //         pressedCoordinatesForKeyBoardGesture.data.Add(xyPairCoordinate);
                    //     }
                    // }

                    // for debug
                    string tmp = "";
                    foreach (var coord in pressedCoordinatesForKeyBoardGesture.data) {
                        tmp += "(" + Convert.ToString(coord.x) + "," + Convert.ToString(coord.y) + "), ";
                    }
                    Debug.Log("up!!!!!!!!!!!!!!!!!!!!!!!!!!!!"); // for debug
                    Debug.Log("All Pressed Coordinates."); // for debug
                    Debug.Log(tmp); // for debug

                    // ネットワーク処理の呼び出し
                    // StartCoroutine("CallServer2");
                    StartCoroutine(GestureTypingSystemClient.CallGestureInferenceAPI2(UpdateCandidatePlane, GetJsonData(), pressedCoordinatesForKeyBoardGesture, inputGestures));

                    // 候補ワードの部分の”Please Input Gesture”を消す
                    // promptTextComp.color = Color.clear;

                    // 記録の初期化
                    pressedCoordinatesForKeyBoardGesture = new pressedCoordinates();
                }

                // 古いポインタがある場合、そのポインタの廃棄を行う
                // if (beforePointer != null) {
                //     Destroy(beforePointer);
                // } else {
                //     beforePointer = PSphere;
                // }
            } else {
                lastSelectedCandidateKey = null; // 候補の選択肢以外に視線が当たっている場合
            }
        } else {
            // 視線がUIとヒットしない場合、ポインタを消す
            // if (beforePointer != null) {
            //     Destroy(beforePointer);
            // }
        }
    }
}
