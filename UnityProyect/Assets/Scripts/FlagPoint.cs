using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagPoint : MonoBehaviour
{
    [SerializeField] private bool hide;
    [SerializeField] private float radius;
    [SerializeField] private Color gizmoColor = Color.blue;
    private void OnDrawGizmos()
    {
        if (!hide)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
