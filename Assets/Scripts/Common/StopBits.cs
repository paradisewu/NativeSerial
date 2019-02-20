//
// 摘要:
//     指定停止上使用的比特数 System.IO.Ports.SerialPort 对象。
public enum StopBits
{
    //
    // 摘要:
    //     使用没有停止位。 不支持此值 System.IO.Ports.SerialPort.StopBits 属性。
    None = 0,
    //
    // 摘要:
    //     使用一个停止位。
    One = 1,
    //
    // 摘要:
    //     使用两个停止位。
    Two = 2,
    //
    // 摘要:
    //     使用 1.5 停止位。
    OnePointFive = 3
}