using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Cập nhật vị trí của camera
public class CamereController : MonoBehaviour
{
	public GameObject ball; 
    private Vector3 offset; 

    void Start () {
         offset = transform.position - ball.transform.position;
    }

    void LateUpdate () {
         //transform.position = ball.transform.position + offset;
    }
}
