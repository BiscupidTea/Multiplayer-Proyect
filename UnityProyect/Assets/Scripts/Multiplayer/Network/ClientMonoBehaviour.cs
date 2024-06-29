using System.Net;
using UnityEngine;
using BT_NetworkSystem;

public class ClientMonoBehaviour : MonoBehaviour
{
    private ClientNetManager clientNetManager = new();
    public PrefabsNetObjectSO prefabs;

    public void OnStartClient(string playerName)
    {
        Debug.Log("Start Connection player");
        clientNetManager.Initialize(new Player(playerName, -1));

        clientNetManager.StartClient();
        clientNetManager.Connect();

        clientNetManager.OnInstanceObject.AddListener(InstanceNewNetObject);

        RequestPlayer();
    }

    public void OnCloseClient()
    {
        clientNetManager.OnInstanceObject.RemoveListener(InstanceNewNetObject);
    }

    private void Update()
    {
        clientNetManager.Update();
    }

    private void InstanceNewNetObject(FactoryData newObject)
    {
        GameObject NewNetObject = Instantiate(prefabs.PrefabsListNetObject[newObject.PrefabId].gameObject,
            new Vector3(newObject.PositionX, newObject.PositionY, newObject.PositionZ),
            new Quaternion(newObject.RotationX, newObject.RotationY, newObject.RotationZ, newObject.RotationW));

        
    }

    private void RequestPlayer()
    {
        FactoryMessage factoryMessage = new FactoryMessage();
        FactoryData data = new FactoryData(new NetObject(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1);
        factoryMessage.data = data;

        clientNetManager.SendMessageToServer(factoryMessage.Serialize(), MessageType.FactoryRequest);
    }
}