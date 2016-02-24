using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class OutLog : MonoBehaviour
{
    public static int mEnableLog = 0; /* 0 off log 1 on log 2 on log callback */
    public static int mLogType = -1; /* -2 custom log -1 all on log 0 Error 1 Assert 2 Warning 3 Log 4 Exception */

    //  Mars 控制List的长度和索引，避免内存的回收
    private const int MAXSIZELST = 50;  //  List 的最大长度
    private const float TIMER = 10000;
    private int m_iCurIndex = 0;            //  当前的List的索引
    private System.Timers.Timer m_timer;    //  计时器
    private StreamWriter m_writer;

    static List<string> mLines = new List<string>();
    static string[] mWriteTxt = new string[MAXSIZELST];
    #region 原流程
    //static List<string> mWriteTxt = new List<string>();
    #endregion
    private string mOutPath;

    private static OutLog mInstance = null;
    public static OutLog Instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject("OutLog", typeof(OutLog));
                mInstance = go.GetComponent<OutLog>();
                DontDestroyOnLoad(go);
            }
            return mInstance;
        }
    }

    void Start()
    {
        mInstance = this;
        DontDestroyOnLoad(this);

        if (mEnableLog == 0)
        {
            return;
        }
        else if (mEnableLog == 2) 
        {
            //在这里做一个Log的监听
            Application.RegisterLogCallback(HandleLog);
        }

        //Application.temporaryCachePath Unity中只有这个路径是既可以读也可以写的。
        mOutPath = Application.temporaryCachePath + "/outLog.txt";
        //Bei Fen之前保存的Log
        if (System.IO.File.Exists(mOutPath))
        {
			System.IO.File.Move(mOutPath, Application.temporaryCachePath + "/outLog" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
            printf(Application.temporaryCachePath + "/outLog" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
        }

        //m_writer = new StreamWriter(mOutPath, true, Encoding.UTF8);
        //m_timer = new System.Timers.Timer(TIMER);
        //m_timer.AutoReset = true;
        //m_timer.Elapsed += new System.Timers.ElapsedEventHandler(WriteLogToTxt);
        //m_timer.Enabled = true;
    }

    void Update()
    {
        #region 原流程
        //if (mEnableLog == 0)
        //{
        //    return;
        //}
        //因为写入文件的操作必须在主线程中完成，所以在Update中哦给你写入文件。
        //if (mWriteTxt.Count > 0)
        //{
        //    string[] temp = mWriteTxt.ToArray();
        //    foreach (string t in temp)
        //    {
        //        using (StreamWriter writer = new StreamWriter(mOutPath, true, Encoding.UTF8))
        //        {
        //            writer.WriteLine(DateTime.Now + "-----" + t + "\n");
        //        }
        //        mWriteTxt.Remove(t);
        //    }
        //}
        #endregion
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (mEnableLog == 0)
        {
            return;
        }

        //  当前的索引技术如果超过最大限制，则开始写入数据，并且重置当前索引
        if (m_iCurIndex >= MAXSIZELST)
            WriteLogToTxt();

        mWriteTxt[m_iCurIndex] = logString;
        m_iCurIndex++;

        if (type == LogType.Error || type == LogType.Exception)
        {
            printf(logString + "\n" + stackTrace);
            mWriteTxt[m_iCurIndex] = logString + "\n" + stackTrace;
            m_iCurIndex++;
        }

        #region 原流程
        //mWriteTxt.Add(logString);
        //if (type == LogType.Error || type == LogType.Exception)
        //{
        //    printf(logString + "\n" + stackTrace);
        //    mWriteTxt.Add(logString + "\n" + stackTrace);
        //}
        #endregion
    }

    private void WriteLogToTxt()
    {
//        for (int i = 0; i < m_iCurIndex; i++)
//            m_writer.WriteLine(DateTime.Now + "-----" + mWriteTxt[i] + "\n");

        m_iCurIndex = 0;
    }

    public void WriteLogToTxt(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (0 == m_iCurIndex)
            return;

        WriteLogToTxt();
    }

    private void AddLog(string logString, object stackTrace, LogType type)
    {
        if (mEnableLog == 0)
        {
            return;
        }

        string tempStackTrace = "";
        if (stackTrace != null)
        {
            tempStackTrace = stackTrace.ToString();
        }

        //	if (mLogType == (int)type || mLogType == -1 || mLogType == -2)
        //	{
        //mWriteTxt.Add(logString);
        //if (tempStackTrace != "")
        //    mWriteTxt.Add(tempStackTrace);
        //	}
    }

    //这里我把错误的信息保存起来，用来输出在手机屏幕上
    public void printf(params object[] objs)
    {
        if (mEnableLog == 0)
        {
            return;
        }

        string text = "";
        for (int i = 0; i < objs.Length; ++i)
        {
            if (i == 0)
            {
                text += objs[i].ToString();
            }
            else
            {
                text += ", " + objs[i].ToString();
            }
        }
        if (Application.isPlaying)
        {
            if (mLines.Count > 20)
            {
                mLines.RemoveAt(0);
            }
            mLines.Add(text);

            mWriteTxt[m_iCurIndex] = text;
            m_iCurIndex++;
            //mWriteTxt.Add(text);
        }
    }

    void OnGUI()
    {
        GUI.color = Color.red;
        for (int i = 0, imax = mLines.Count; i < imax; ++i)
        {
            if (mEnableLog != 0)
            {
                GUILayout.Label(mLines[i]);
            }
        }
    }

    public void Log(string message)
    {
        Log(message, null);
    }
    public void Log(string message, string context)
    {
        AddLog(message, context, LogType.Log);
    }
    public void LogError(string message)
    {
        LogError(message, null);
    }
    public void LogError(string message, object context)
    {
        AddLog(message, context, LogType.Error);
    }
    public void LogWarning(string message)
    {
        LogWarning(message, null);
    }
    public void LogWarning(string message, object context)
    {
        AddLog(message, context, LogType.Warning);
    }
    public void LogException(string message)
    {
        LogException(message, null);
    }
    public void LogException(string message, object context)
    {
        AddLog(message, context, LogType.Exception);
    }
    public void LogCustom(string message)
    {
        LogCustom(message, null);
    }
    public void LogCustom(string message, object context)
    {
        AddLog(message, context, LogType.Exception);
    }

    //	public void SetEnableLog(int enableLog)
    //	{
    //		mEnableLog = enableLog;
    //	}
    //	public void SetLogType(int logType)
    //	{
    //		mLogType = logType;
    //	}
}