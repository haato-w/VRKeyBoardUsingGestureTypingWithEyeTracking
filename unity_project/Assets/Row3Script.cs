using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Row3Script : AbstractRowScript
{
    // Start is called before the first frame update
    void Start()
    {
        Transform myTransform = this.transform;

        // ローカル座標を用いてサイズを設定
        this.setRocalSize(myTransform);

        // ローカル座標を用いてYZ座標を定義
        this.setLocalPosExceptX(myTransform);


        // ローカル座標を用いてX座標を定義
        Vector3 localPos = myTransform.localPosition;
        localPos.x = (float)((0.05 + 0.2) * 2.0 * 2.3); // 半分の倍率になる理由が不明
        // Debug.Log("newRow3LocalX: " + localPos.x);
        myTransform.localPosition = localPos; // ローカル座標での座標を設定
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
