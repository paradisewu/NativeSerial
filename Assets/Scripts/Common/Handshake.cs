  //
     // 摘要:
     //     指定在建立串行端口的通信使用的控制协议 System.IO.Ports.SerialPort 对象。
    public enum Handshake
    {
        //
        // 摘要:
        //     无法控制用于在握手。
        None = 0,
        //
        // 摘要:
        //     使用 XON/XOFF 软件控制协议。 XOFF 控件发送，以停止数据传输。 XON 控制发送以继续传输。 这些软件控制而不是请求发送 (RTS) 使用，并清除硬件控件的发送
        //     (CTS)。
        XOnXOff = 1,
        //
        // 摘要:
        //     使用请求发送 (RTS) 硬件流控制。 RTS 通知可用于传输数据。 RTS 行输入的缓冲区变满之后，如果将设置为 false。 RTS 行会将设置为 true
        //     更多的空间变得可用时输入缓冲区中。
        RequestToSend = 2,
        //
        // 摘要:
        //     使用请求-发送 (RTS) 硬件控制和 XON/XOFF 软件控制。
        RequestToSendXOnXOff = 3
    }