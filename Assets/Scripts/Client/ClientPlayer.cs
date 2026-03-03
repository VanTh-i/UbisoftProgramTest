using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPlayer : MonoBehaviour
{
    public int PlayerID;
    public bool isLocalPlayer;

    private Vector3 serverTargetPosition;

    public float lerpSpeed = 7f;

    public void Initialize(int id, Vector3 startPos, bool isLocal)
    {
        PlayerID = id;
        isLocalPlayer = isLocal;
        transform.position = startPos;
        serverTargetPosition = startPos;

        GetComponent<Renderer>().material.color = isLocal ? Color.green : Color.red;
    }

    public void UpdateServerPosition(Vector3 newPos)
    {
        serverTargetPosition = newPos;
    }

    private void Update()
    {
        Vector3 moveDirection = serverTargetPosition - transform.position;
        moveDirection.y = 0f;

        if (Vector3.Distance(transform.position, serverTargetPosition) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, serverTargetPosition, Time.deltaTime * lerpSpeed);
        }

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
    }
}
