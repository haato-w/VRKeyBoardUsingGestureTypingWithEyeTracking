using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyWScript : AbstractKeyScript
{
    // Start is called before the first frame update
    void Start()
    {
        Transform myTransform = this.transform;

        //this.setLocalScale(myTransform); // スケールを設定

        //this.setLocalPositionExceptZ(myTransform); // ポジションのXY座標を設定

        //// ローカル座標での座標を取得
        //Vector3 localPos = myTransform.localPosition;
        //GameObject parentObj = myTransform.parent.gameObject;
        //float rowWidth = parentObj.transform.lossyScale.z; // キーボードの横の長さ
        //localPos.z = -(float)(1 - (0.4 + 0.05 + 0.4) / rowWidth) * rowWidth; // ローカル座標を基準にした、z座標
        ////localPos.z = -(float)((5 / 2 + 5 * 4 + 40 * 4 + 40 / 2) / 100); // ローカル座標を基準にした、z座標
        //myTransform.localPosition = localPos; // ローカル座標での座標を設定

        string keyText = getKeyText(myTransform);
        Vector3 screenPosition = getScreenPosition(myTransform);
        // Debug.Log(keyText + " screenPos: " + screenPosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
