using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using Oculus.Platform;
using TMPro;


public static class GestureTypingSystemClient
{

    delegate void UpdateCandidatePlane(string[] candidateWords);

    // string Adderess = "http://127.0.0.1:8080";
    private static string Adderess = "http://192.168.10.9:8080";
    // string Adderess = "http://192.168.219.119:8080";
    //string testPage = "/test-api";


    public static IEnumerator CallGestureInferenceAPI(
        Func<string[], int> UpdateCandidatePlaneFunc, 
        string sendData)
    {
        string Page = "/shark2";
        var url = Adderess + Page;
        // var json = GetJsonData();
        var json = sendData;
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

            UpdateCandidatePlaneFunc(bestWords);
        }
    }

    public static IEnumerator CallGestureInferenceAPI2(
        Func<string[], int> UpdateCandidatePlaneFunc, 
        string sendData, 
        pressedCoordinates pressedCoordinatesData, 
        List<pressedCoordinates> inputGesturesList)
    {
        string Page = "/shark2";
        var url = Adderess + Page;
        // var json = GetJsonData();
        var json = sendData;
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

            inputGesturesList.Add(pressedCoordinatesData); // ジェスチャーのリストに座標系列を追加

            UpdateCandidatePlaneFunc(bestWords);
        }
    }

    // public static IEnumerator CallGestureInferenceAPI2(TMP_Text textObj, string sendData)
    // {
    //     string Page = "/shark2";
    //     var url = Adderess + Page;
    //     // var json = GetJsonData();
    //     var json = sendData;
    //     //var json = JsonUtility.ToJson(data);
    //     var postData = Encoding.UTF8.GetBytes(json);

    //     Debug.Log("json: " + json);

    //     using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
    //     {
    //         uploadHandler = new UploadHandlerRaw(postData),
    //         downloadHandler = new DownloadHandlerBuffer()
    //     };

    //     request.SetRequestHeader("Content-Type", "application/json");

    //     yield return request.SendWebRequest();

    //     //if (request.result == UnityWebRequest.Result.ConnectionError ||
    //     //    request.result == UnityWebRequest.Result.ProtocolError)
    //     //{

    //     // 通信結果
    //     if (request.isNetworkError ||
    //         request.isHttpError)  // 失敗
    //     {
    //         Debug.Log("Network error:" + request.error);
    //     }
    //     else                  // 成功
    //     {
    //         string response = request.downloadHandler.text;
    //         Debug.Log("Succeeded:" + response);
    //         // new List<string>{request.downloadHandler.text}

    //         responseJson resJson = JsonUtility.FromJson<responseJson>(response);
    //         Debug.Log("resJson: " + resJson.best_word);

    //         string[] bestWords = resJson.best_word.Split(" ");

    //         textObj.text = String.Join(", ", bestWords);
    //     }
    // }

    public static IEnumerator CallServerToRegisterData(acquiredDataList inputDataList) {
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
