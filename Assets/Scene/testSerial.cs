using LuaFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testSerial : MonoBehaviour
{

    MySerialPort myserial;
    byte[] buffers = new byte[1024];
    // Use this for initialization
    void Start()
    {
        myserial = new MySerialPort("COM4", 9600, Parity.None, StopBits.Two);
        myserial.Open();
    }

    byte[] tempbuffers;

     //private string OpenMessage = "01 03 01 C2 00 08 E4 0C";
    string OpenMessage = "01 03 01 C2 00 10 E4 06";
    string output = "";


    private void OnGUI()
    {
        if (GUILayout.Button("读"))
        {
            int readnum = myserial.Read(buffers);
            Debug.Log(readnum);

            if (tempbuffers == null)
            {
                 tempbuffers = new byte[readnum];
            }
           
            Buffer.BlockCopy(buffers, 0, tempbuffers, 0, readnum);
            Debug.Log(tempbuffers.Length);

            ByteBuffer buffer = new ByteBuffer(tempbuffers);

            buffer.ReadByte();
            buffer.ReadByte();
            int length = buffer.ReadByte();
            length = length / 2;
            for (int i = 0; i < length; i++)
            {
                ushort t = buffer.ReadShort();
                Debug.Log(t);
            }
            //ushort a = BitConverter.to(tempbuffers, 0);
            //Debug.Log(a);

            for (int i = 0; i < readnum; i++)
            {
                //strAscii += ((char)dataBuff[i]).ToString() + ' '; //转换为字符
                string s = buffers[i].ToString("X");
                //Debug.Log(s);

                if (s.Length == 1)   //当16进制数只有一位是，在前面补上0；
                    s = "0" + s;
                output += s + ' ';
            }
            Debug.Log(output);
        }
        if (GUILayout.Button("写"))
        {
            byte[] data = HexToByte(OpenMessage);
            myserial.Write(data);
        }
    }

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
        myserial.Close();
    }
}
