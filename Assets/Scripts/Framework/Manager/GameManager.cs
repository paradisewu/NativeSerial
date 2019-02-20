using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
namespace LuaFramework
{
    public class GameManager : Manager
    {
        private MySerialPort myserial;
        //private string OpenMessage = "01 03 01 C2 00 10 E4 06";  //06E4 計算8路
        //private string OpenMessage = "01 03 01 C2 00 48 E5 FC";  //FCE5 計算8路

        public bool IsOpen = false;
        private byte[] buffers = new byte[1024];
        private byte[] tempbuffers;
        private static byte[] SendMsgData;


        private ByteBuffer BufferBox;
        public Dictionary<int, float> HeightValues;
        //private Dictionary<int, float> OriginalValues;

        private int m_HeightCount;
        private int HeightCount
        {
            get
            {
                return m_HeightCount;
            }
            set
            {
                if (m_HeightCount != value)
                {
                    m_HeightCount = value;
                    HeightList = new int[value];
                }
            }
        }
        public int[] HeightList;


        private string ComPort;
        private string OpenMessage;

        private void Awake()
        {
            ConfigFilePath = Application.dataPath + "/../Config.ini";
            ReadConfig();
            ComPort = ConfigDictionary["COM"];
            OpenMessage = ConfigDictionary["SendMessage"];
        }

        void Start()
        {
            //UIManager.SwitchingScene(SceneType.SceneVideo);
            myserial = new MySerialPort("COM4", 9600, Parity.None, StopBits.Two);

            HeightValues = new Dictionary<int, float>();

            if (string.IsNullOrEmpty(OpenMessage))
            {
                return;
            }
            SendMsgData = HexToByte(OpenMessage);
            try
            {
                myserial.Open();
                IsOpen = true;
            }
            catch (System.Exception e)
            {
                IsOpen = false;
                Debug.Log(e.Message);
            }
            StartCoroutine(AnalysisData());
        }


        IEnumerator AnalysisData()
        {
            yield return new WaitForSeconds(1f);
            WaitForSeconds wait = new WaitForSeconds(0.1f);
            while (IsOpen)
            {
                Debug.Log("...........");
                myserial.Write(SendMsgData);
                yield return wait;

                int readnum = myserial.Read(buffers);
                if (readnum > 0)
                {

                    tempbuffers = new byte[readnum];

                    Buffer.BlockCopy(buffers, 0, tempbuffers, 0, readnum);
                    BufferBox = new ByteBuffer(tempbuffers);
                    BufferBox.ReadByte();
                    BufferBox.ReadByte();

                    int length = BufferBox.ReadByte();
                    length = length / 4;
                    HeightCount = length;
                    for (int i = 0; i < length; i++)
                    {
                        ushort high1 = BufferBox.ReadByte();  //每一路重量
                        ushort high2 = BufferBox.ReadByte();  //每一路重量

                        ushort low1 = BufferBox.ReadByte();  //每一路重量
                        ushort low2 = BufferBox.ReadByte();  //每一路重量

                        int high = high1 * 256 + high2;
                        int low = low1 * 256 + low2;
                        int t = high * 256 + low;


                        //Debug.Log(i + "  >>   " + t);
                        if (HeightValues.ContainsKey(i))
                        {
                            HeightValues[i] = t;
                        }
                        else
                        {
                            HeightValues.Add(i, t);
                        }
                        //HeightList[i] = t;
                    }
                }
                yield return wait;
            }
        }


        private int m_PlayMovieIndex = 0;
        public int PlayMovieIndex
        {
            get
            {
                return m_PlayMovieIndex;
            }
            set
            {
                if (value != m_PlayMovieIndex)
                {
                    Debug.Log(" value  :" + value);
                    m_PlayMovieIndex = value;
                    facade.SendMessageCommand(MessageDef.VideoPlay, m_PlayMovieIndex);
                }
            }
        }

        private int PickUpNumber = 0;
        private List<int> PickUpList = new List<int>();  //记录当前 ID
        private List<int> LastPickUpList = new List<int>(); //记录上一次拿起来的 ID
        public void Update()
        {
            PickUpList.Clear();
            for (int i = 0; i < HeightValues.Count; i++)
            {
                if (Mathf.Abs(HeightValues[i]) < 10 || Mathf.Abs(HeightValues[i]) > 100000)
                {
                    Debug.Log("拾起的數值 ： " + i);
                    PickUpList.Add(i);
                }
            }
            PickUpNumber = PickUpList.Count;
            if (PickUpNumber >= 2)
            {
                //拿起来超过两个
                Debug.Log("超过两个");

                int count = LastPickUpList.Count;
                if (count > 0)
                {
                    if (count >= PickUpList.Count)
                    {
                        // 放下产品
                        LastPickUpList.Clear();
                        for (int i = 0; i < PickUpList.Count; i++)
                        {
                            LastPickUpList.Add(PickUpList[i]);
                        }
                    }
                    else
                    {
                        // 拿起产品

                        int pickupcount = PickUpList.Count - count;
                        PlayMovieIndex = PickUpList[count];

                        LastPickUpList.Clear();
                        for (int i = 0; i < PickUpList.Count; i++)
                        {
                            LastPickUpList.Add(PickUpList[i]);
                        }
                    }
                }
                else
                {
                    PlayMovieIndex = PickUpList[0];
                }
                pickup = true;
            }
            else if (PickUpNumber > 0)
            {
                //拿起来1个
                Debug.Log("只有一个");
                PlayMovieIndex = PickUpList[0];
                LastPickUpList.Add(PickUpList[0]);
                pickup = true;
            }
            else
            {
                Debug.Log("没有重量");
                LastPickUpList.Clear();
                if (pickup == true)
                {
                    m_PlayMovieIndex = -1;
                    facade.SendMessageCommand(MessageDef.VideoPlay, -1);
                }
                pickup = false;
            }
        }


        private bool pickup = false;
        public static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");//移除空格
                                       //create a byte array the length of the
                                       //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                //convert each set of 2 characters to a byte and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            }
            return comBuffer;
        }

        private void OnDestroy()
        {
            SendMsgData = null;
            if (IsOpen)
            {
                myserial.Close();
            }
        }



        #region INI
        private string ConfigFilePath;
        private Dictionary<string, string> ConfigDictionary = new Dictionary<string, string>();
        private List<string> ConfigKeyList = new List<string>() { "COM", "SendMessage" };
        private void ReadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                //读取Config数据
                foreach (string item in ConfigKeyList)
                {
                    string value = INIhelp.GetValue(item);
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (!ConfigDictionary.ContainsKey(item))
                        {
                            ConfigDictionary.Add(item, value);
                        }
                    }
                }
            }
        }

        public void WriteConfig(string key, string value)
        {
            if (File.Exists(ConfigFilePath))
            {
                if (ConfigDictionary.ContainsKey(key))
                {
                    INIhelp.SetValue(key, value);
                    ConfigDictionary[key] = value;
                }
            }
        }
        #endregion
        public class INIhelp
        {
            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

            //ini文件名称
            private static string inifilename = "Config.ini";
            //获取ini文件路径
            private static string inifilepath = Directory.GetCurrentDirectory() + "\\" + inifilename;

            public static string GetValue(string key)
            {
                StringBuilder s = new StringBuilder(1024);
                GetPrivateProfileString("CONFIG", key, "", s, 1024, inifilepath);
                return s.ToString();
            }


            public static void SetValue(string key, string value)
            {
                try
                {
                    WritePrivateProfileString("CONFIG", key, value, inifilepath);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public class IniFiles
        {
            public string inipath;

            //声明API函数

            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
            /// <summary> 
            /// 构造方法 
            /// </summary> 
            /// <param name="INIPath">文件路径</param> 
            public IniFiles(string INIPath)
            {
                inipath = INIPath;
            }

            public IniFiles() { }

            /// <summary> 
            /// 写入INI文件 
            /// </summary> 
            /// <param name="Section">项目名称(如 [TypeName] )</param> 
            /// <param name="Key">键</param> 
            /// <param name="Value">值</param> 
            public void IniWriteValue(string Section, string Key, string Value)
            {
                WritePrivateProfileString(Section, Key, Value, this.inipath);
            }
            /// <summary> 
            /// 读出INI文件 
            /// </summary> 
            /// <param name="Section">项目名称(如 [TypeName] )</param> 
            /// <param name="Key">键</param> 
            public string IniReadValue(string Section, string Key)
            {
                StringBuilder temp = new StringBuilder(500);
                int i = GetPrivateProfileString(Section, Key, "", temp, 500, this.inipath);
                return temp.ToString();
            }
            /// <summary> 
            /// 验证文件是否存在 
            /// </summary> 
            /// <returns>布尔值</returns> 
            public bool ExistINIFile()
            {
                return File.Exists(inipath);
            }
        }

    }
}