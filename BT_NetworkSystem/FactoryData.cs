namespace BT_NetworkSystem
{
    public struct FactoryData : INetObject
    {
        public NetObject netObject;

        public int ParentId;
        public int PrefabId;

        public float PositionX;
        public float PositionY;
        public float PositionZ;

        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float RotationW;

        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;


        public FactoryData(NetObject netObject, int ParentId, int PrefabId,
            float PositionX, float PositionY, float PositionZ,
            float RotationX, float RotationY, float RotationZ, float RotationW,
            float ScaleX, float ScaleY, float ScaleZ)
        {
            this.netObject = netObject;
            this.ParentId = ParentId;
            this.PrefabId = PrefabId;

            this.PositionX = PositionX;
            this.PositionY = PositionY;
            this.PositionZ = PositionZ;
            
            this.RotationX = RotationX;
            this.RotationY = RotationY;
            this.RotationZ = RotationZ;
            this.RotationW = RotationW;

            this.ScaleX = ScaleX;
            this.ScaleY = ScaleY;
            this.ScaleZ = ScaleZ;
        }

        public int getID()
        {
            return netObject.id;
        }

        public int getOwner()
        {
            return netObject.owner;
        }

        public NetObject getNetObject()
        {
            return netObject;
        }

        public void SetID(int id)
        {
            netObject.id = id;
        }

        public void SetOwner(int owner)
        {
            netObject.owner = owner;
        }
    }
}