namespace BT_NetworkSystem
{
    public class NetObject
    {
        public int id;
        public int owner;
    }

    public interface INetObject
    {
        int getID();
        int getOwner();
        NetObject getNetObject();
        void SetID(int id);
        void SetOwner(int owner);
    }
}