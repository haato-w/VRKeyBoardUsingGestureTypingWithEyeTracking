using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractKeyScript : MonoBehaviour
{
    //public string keyText;
    //public Vector3 screenPosition;

    //public AbstractKeyScript()
    //{
    //    Debug.Log("AbstractKeyScript");
    //    Transform myTransform = this.transform;
    //    keyText = getKeyText(myTransform);
    //    screenPosition = getScreenPosition(myTransform);
    //}

    public void setLocalScale(Transform myTransform)
    {
        GameObject parentObj = myTransform.parent.gameObject;

        float rowWidth = parentObj.transform.lossyScale.z; // キーボードの横の長さ

        Vector3 localScale = myTransform.localScale; // ローカル座標を基準にした、サイズを取得
        localScale.x = 1.0f;
        localScale.y = 1.0f; // ローカル座標を基準にした、y軸方向へサイズ変更
        localScale.z = (float)(0.4 / rowWidth);
        myTransform.localScale = localScale;
    }

    public void setLocalPositionExceptZ(Transform myTransform)
    {
        // ローカル座標での座標を取得
        Vector3 localPos = myTransform.localPosition;
        localPos.x = 0.0f;    // ローカル座標を基準にした、x座標
        localPos.y = 0.01f;    // ローカル座標を基準にした、y座標
        myTransform.localPosition = localPos; // ローカル座標での座標を設定
    }

    public string getKeyText(Transform myTransform)
    {
        GameObject childObj = myTransform.GetChild(0).gameObject;
        return childObj.GetComponent<UnityEngine.TextMesh>().text;
    }

    public Vector3 getScreenPosition(Transform myTransform)
    {
        Camera cam = Camera.main;
        Vector3 pos = myTransform.position;
        return cam.WorldToScreenPoint(pos);
    }
}