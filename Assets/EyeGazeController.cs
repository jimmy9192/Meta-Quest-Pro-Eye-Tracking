using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EyeGazeController : MonoBehaviour
{
    private OVRPlugin.EyeGazesState EyeGazeState;

    [Header("ターゲットオブジェクト")]
    public GameObject RightTargetObject;
    public GameObject LeftTargetObject;

    [Header("カメラ設定")]
    public Camera RightTargetCamera;
    public Camera LeftTargetCamera;

    [Header("記録設定")]
    // public float startDelay = 5f;       // ← Aボタン起動に変更したため未使用
    public float recordingDuration = 5f;   // 記録時間（秒）
    public string experimentCondition = "baseline";  // 実験条件名

    [Header("保存先設定")]
    public string saveFolderName = "ExperimentData";  // Assets内のフォルダ名

    // 記録用のデータ構造
    [System.Serializable]
    public class GazeData
    {
        public float timestamp;
        public Vector3 leftHitPoint;
        public Vector3 rightHitPoint;
        public string leftHitObjectName;
        public string rightHitObjectName;
        public string leftHitObjectTag;
        public string rightHitObjectTag;
        public bool leftHit;
        public bool rightHit;
    }

    private List<GazeData> gazeDataList = new List<GazeData>();

    // 状態管理
    private float elapsedTime = 0f;
    private float recordingStartTime = 0f;
    private bool isRecording = false;
    private bool hasStarted = false;
    private bool hasFinished = false;

    // ファイル保存先
    private string csvFilePath;
    private string saveFolderPath;

    void Start()
    {
        Debug.Log("EyeGazeController started!");
        Debug.Log("Aボタンを押すと記録開始、" + recordingDuration + " 秒間記録します");

        // 保存先フォルダを設定
        SetupSaveFolder();

        // CSVファイル名を実験条件＋実行時刻で設定
        string fileName = "EyeGazeData_" + experimentCondition + "_" +
                          System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        csvFilePath = Path.Combine(saveFolderPath, fileName);

        Debug.Log("====================================");
        Debug.Log("CSV保存先: " + csvFilePath);
        Debug.Log("====================================");
    }

    /// <summary>
    /// 保存先フォルダの設定（エディタ：Assets内、実機：persistentDataPath）
    /// </summary>
    private void SetupSaveFolder()
    {
#if UNITY_EDITOR
        // Unityエディタ実行時：Assetsフォルダ内に保存
        saveFolderPath = Path.Combine(Application.dataPath, saveFolderName);
        Debug.Log("実行モード：エディタ → Assets内に保存します");
#else
        // 実機ビルド時：persistentDataPath（書き込み可能な領域）に保存
        saveFolderPath = Path.Combine(Application.persistentDataPath, saveFolderName);
        Debug.Log("実行モード：実機 → persistentDataPathに保存します");
#endif

        // フォルダがなければ作成
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            Debug.Log("フォルダを作成しました: " + saveFolderPath);
        }
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Aボタンを押したら記録開始（まだ開始していない場合のみ）
        if (!hasStarted && OVRInput.GetDown(OVRInput.Button.One))
        {
            isRecording = true;
            hasStarted = true;
            recordingStartTime = elapsedTime;
            Debug.Log("===== 記録開始（Aボタン） =====");
        }

        // 5秒間の記録が完了したら終了
        if (isRecording && (elapsedTime - recordingStartTime) >= recordingDuration)
        {
            StopRecording();
        }

        // 視線データを取得
        bool success = OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref EyeGazeState);

        if (success)
        {
            var LeftEyeGaze = EyeGazeState.EyeGazes[(int)OVRPlugin.Eye.Left];
            var RightEyeGaze = EyeGazeState.EyeGazes[(int)OVRPlugin.Eye.Right];

            if (LeftEyeGaze.IsValid)
            {
                var LeftPose = LeftEyeGaze.Pose.ToOVRPose();
                var RightPose = RightEyeGaze.Pose.ToOVRPose();

                Vector3 GazeLeftDirection = LeftPose.orientation * Vector3.forward;
                Vector3 GazeRightDirection = RightPose.orientation * Vector3.forward;

                Vector3 GazeLeftPosition = LeftTargetCamera.transform.position;
                Vector3 GazeRightPosition = RightTargetCamera.transform.position;

                GazeData currentData = new GazeData();
                currentData.timestamp = elapsedTime - recordingStartTime;

                // 左眼のRaycast
                if (Physics.Raycast(GazeLeftPosition, GazeLeftDirection, out RaycastHit lefthitinfo))
                {
                    LeftTargetObject.transform.position = lefthitinfo.point;
                    currentData.leftHitPoint = lefthitinfo.point;
                    currentData.leftHitObjectName = lefthitinfo.collider.name;
                    currentData.leftHitObjectTag = lefthitinfo.collider.tag;
                    currentData.leftHit = true;
                }
                else
                {
                    currentData.leftHitPoint = Vector3.zero;
                    currentData.leftHitObjectName = "None";
                    currentData.leftHitObjectTag = "Untagged";
                    currentData.leftHit = false;
                }

                // 右眼のRaycast
                if (Physics.Raycast(GazeRightPosition, GazeRightDirection, out RaycastHit righthitinfo))
                {
                    RightTargetObject.transform.position = righthitinfo.point;
                    currentData.rightHitPoint = righthitinfo.point;
                    currentData.rightHitObjectName = righthitinfo.collider.name;
                    currentData.rightHitObjectTag = righthitinfo.collider.tag;
                    currentData.rightHit = true;
                }
                else
                {
                    currentData.rightHitPoint = Vector3.zero;
                    currentData.rightHitObjectName = "None";
                    currentData.rightHitObjectTag = "Untagged";
                    currentData.rightHit = false;
                }

                if (isRecording)
                {
                    gazeDataList.Add(currentData);
                }
            }
        }
    }

    /// <summary>
    /// 記録を停止してCSVに保存
    /// </summary>
    private void StopRecording()
    {
        isRecording = false;
        hasFinished = true;
        Debug.Log("===== 記録終了 =====");
        Debug.Log("総記録フレーム数: " + gazeDataList.Count);
        Debug.Log("実際の記録頻度: " + (gazeDataList.Count / recordingDuration).ToString("F2") + " Hz");

        SaveToCSV();
    }

    /// <summary>
    /// CSVファイルに保存
    /// </summary>
    private void SaveToCSV()
    {
        StringBuilder csvBuilder = new StringBuilder();

        // ヘッダー行
        csvBuilder.AppendLine("Timestamp,LeftHit,LeftHitObject,LeftHitTag,LeftHitX,LeftHitY,LeftHitZ,RightHit,RightHitObject,RightHitTag,RightHitX,RightHitY,RightHitZ");

        // データ行
        foreach (var data in gazeDataList)
        {
            csvBuilder.AppendLine(
                $"{data.timestamp:F4}," +
                $"{data.leftHit},{data.leftHitObjectName},{data.leftHitObjectTag}," +
                $"{data.leftHitPoint.x:F4},{data.leftHitPoint.y:F4},{data.leftHitPoint.z:F4}," +
                $"{data.rightHit},{data.rightHitObjectName},{data.rightHitObjectTag}," +
                $"{data.rightHitPoint.x:F4},{data.rightHitPoint.y:F4},{data.rightHitPoint.z:F4}"
            );
        }

        // ファイル書き出し
        try
        {
            File.WriteAllText(csvFilePath, csvBuilder.ToString());
            Debug.Log("✅ CSVファイルを保存しました: " + csvFilePath);

#if UNITY_EDITOR
            // ★重要：Unityエディタでファイルを認識させる
            AssetDatabase.Refresh();
            Debug.Log("Unity AssetDatabase をリフレッシュしました");
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ CSV保存に失敗しました: " + e.Message);
        }
    }

    /// <summary>
    /// アプリ終了時の保険
    /// </summary>
    void OnApplicationQuit()
    {
        if (!hasFinished && gazeDataList.Count > 0)
        {
            Debug.Log("アプリ終了時の自動保存を実行します");
            SaveToCSV();
        }
    }
}