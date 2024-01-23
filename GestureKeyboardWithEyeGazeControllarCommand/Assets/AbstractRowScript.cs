using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractRowScript : MonoBehaviour
{
    public void setLocalPosExceptX(Transform myTransform)
    {
        // ローカル座標での座標を取得
        Vector3 localPos = myTransform.localPosition;
        localPos.y = 0.01f;    // ローカル座標を基準にした、y座標
        localPos.z = 0.0f;    // ローカル座標を基準にした、z座標
        myTransform.localPosition = localPos; // ローカル座標での座標を設定
    }

    public void setRocalSize(Transform myTransform)
    {
        GameObject rootObj = myTransform.root.gameObject;

        float keyBoardHeight = rootObj.transform.lossyScale.x; // キーボードのHeight値
        //Debug.Log("KeyBoardHeight: " + keyBoardHeight);

        // ローカル座標を基準にした、サイズを取得
        Vector3 localScale = myTransform.localScale;
        localScale.x = (float)(0.4 / keyBoardHeight); // ローカル座標を基準にした、x軸方向へ2倍のサイズ変更
        localScale.y = 1.0f; // ローカル座標を基準にした、y軸方向へ2倍のサイズ変更
        localScale.z = 1.0f; // ローカル座標を基準にした、z軸方向へ2倍のサイズ変更
        myTransform.localScale = localScale;
    }

}
