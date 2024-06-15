namespace BT_NetworkSystem
{
    using System.Net;
    public interface IReceiveData
    {
        void OnReceiveData(byte[] data, IPEndPoint ipEndpoint);
    }
}
