using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;

public interface ICHMessenger
{
    void Close();
    void OnDisconnect();
    bool IsConnected();
    void SendMsg(string message);
    void SendHeartBeat();
}
public class CHErrorMsg
{
    public int err_type = 0;
    public string reason = "";
    public string ok_load = "";
    public string cancel_load = "";

    public static CHErrorMsg GetError(string result)
    {
        CHErrorMsg errorData = new CHErrorMsg();
        try
        {
            /*
            JsonReader jsonReader = new JsonReader(result);
            while (jsonReader.Read())
            {
                if (JsonUtil.IsPropertyName(jsonReader, "err"))
                {
                    errorData.err_type = JsonUtil.GetValueInt(jsonReader);
                }
                else if (JsonUtil.IsPropertyName(jsonReader, "reason"))
                {
                    errorData.reason = JsonUtil.GetValue(jsonReader);
                    UnityEngine.Debug.Log("GetError is " + errorData.reason);
                }
                else if (JsonUtil.IsPropertyName(jsonReader, "ok_load"))
                {
                    errorData.ok_load = JsonUtil.GetValue(jsonReader);
                }
                else if (JsonUtil.IsPropertyName(jsonReader, "cancel_load"))
                {
                    errorData.cancel_load = JsonUtil.GetValue(jsonReader);
                }
            }
            */
            JsonData data = JsonMapper.ToObject(result);
            for (int i = 0; i < data.Count; i++)
            {
                var dataArray = data[i];
                //if (CommonFunc.CheckJsonDataHasKey(dataArray, "err"))
                //{
                //    errorData.err_type = CommonFunc.ParseIntFromString(dataArray, "err");
                //}
                //if (CommonFunc.CheckJsonDataHasKey(dataArray, "reason"))
                //{
                //    errorData.reason = CommonFunc.ParseStringFromJsonData(dataArray, "reason");
                //}
                //if (CommonFunc.CheckJsonDataHasKey(dataArray, "ok_load"))
                //{
                //    errorData.ok_load = CommonFunc.ParseStringFromJsonData(dataArray, "ok_load");
                //}
                //if (CommonFunc.CheckJsonDataHasKey(dataArray, "cancel_load"))
                //{
                //    errorData.cancel_load = CommonFunc.ParseStringFromJsonData(dataArray, "cancel_load");
                //}
            }
        }
        catch (Exception e)
        {
            errorData.err_type = 1;
            errorData.reason = result;
        }
        return errorData;
    }
}