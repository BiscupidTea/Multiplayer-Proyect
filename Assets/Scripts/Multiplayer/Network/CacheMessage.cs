using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CacheMessage
{
    public byte[] message;
    public uint id;
    public MessageType type;
    public DateTime lastEmission;
    public bool Received;

    public CacheMessage(byte[] message, uint id, MessageType type)
    {
        this.message = message;
        this.id = id;
        this.type = type;
        lastEmission = DateTime.UtcNow;
        Received = false;
    }
}