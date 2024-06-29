using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPreabsList", menuName = "ScriptableObjects/NetObjectPreabsList")]
public class PrefabsNetObjectSO : ScriptableObject
{
    [Serializable]
    public class PrefabsNetObject
    {
        public int id;
        public GameObject gameObject;
    }

    public PrefabsNetObject[] PrefabsListNetObject;
}
