using System;

public class MySerialPort
{    
    private IntPtr m_comm;
    private string m_portName;
    private int m_portNum;
    private int m_baudRate;
    private char m_Parity='n';
    private int m_dataBits = 8;
    private int m_StopBits = 1;

    public MySerialPort(string portName, int baudRate)
    {
        this.m_portName = portName;
        string _name= this.m_portName.Substring(3);
        if(!int.TryParse(_name,out m_portNum))
        {
            this.m_portNum = -1;
        }
        this.m_baudRate = baudRate;
        m_comm = cnCommWrapper.CreateComm();
    }

    public MySerialPort(string portName, int baudRate, Parity parity, StopBits stopBits) : this(portName, baudRate)
    {
        this.m_portName = portName;
        string _name = this.m_portName.Substring(3);
        if (!int.TryParse(_name, out m_portNum))
        {
            this.m_portNum = -1;
        }
        this.m_baudRate = baudRate;
        switch (parity)
        {
            case Parity.None:
                m_Parity = 'n';
                break;
            case Parity.Even:
                m_Parity = 'e';
                break;
            case Parity.Odd:
                m_Parity = 'o';
                break;
            case Parity.Mark:
                m_Parity = 'm';
                break;
            case Parity.Space:
                m_Parity = 's';
                break;
        }
        switch (stopBits)
        {
            case StopBits.One:
                m_StopBits = 1;
                break;
            case StopBits.Two:
                m_StopBits = 2;
                break;
            case StopBits.OnePointFive:
                m_StopBits = 3;
                break;
        }
        m_comm = cnCommWrapper.CreateComm();
    }

    public void Open()
    {
        string setStr = string.Format("{0},{1},{2},{3}", m_baudRate, m_Parity, m_dataBits, m_StopBits);
        bool ret = cnCommWrapper.Open(m_comm, this.m_portNum, setStr);
        return;
    }
    public void Close()
    {
        cnCommWrapper.Close(m_comm);
    }
    public int Read(byte[] buffer)
    {
        return cnCommWrapper.Read(m_comm, buffer, buffer.Length);
    }
    public void Write(byte[] buffer)
    {
        cnCommWrapper.Write(m_comm, buffer, buffer.Length);
    }
    public bool IsOpen
    {
        get
        {
            return true;// cnCommWrapper.IsOpen(m_comm);
        }
    }
}
