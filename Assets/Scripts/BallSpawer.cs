using System.Collections;
using System.Collections.Generic;
using TikTokLiveSharp.Events;
using TikTokLiveSharp.Events.Objects;
using UnityEngine;
using UnityEngine.UI;

public class BallSpawer : MonoBehaviour
{
    public Ball ballPrefab;    // Prefab quả bóng
    public float spawnInterval = 2f; // Thời gian giữa các lần tạo bóng (giây)
    private float timer;

    void Awake()
    {
        //Debug.Log("BallSpawer Update: position =" + spawnPoint.position.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnBall();
            timer = 0f;
        }
    }
    void SpawnBall(Picture avatarPicture = null)
    {
        Ball ball = Instantiate(ballPrefab, transform.position, transform.rotation);
        if (avatarPicture != null)
        {
            MeshRenderer mr = ball.GetComponent<MeshRenderer>();
            Material[] mats = mr.materials;
            Image img = this.GetComponentInChildren<Image>();
            //Material faceMat = new Material(coinFaceMaterial);
            //faceMat.mainTexture = avatarPicture.Texture;       // gán texture từ avatarPicture

            // Thay mặt dưới (index 1)
            if (mats.Length > 1)
            {
                //mats[1] = faceMat;
                //mats[1].mainTexture;
            }

            mr.materials = mats; // gán lại cho MeshRenderer
        }
    }
}
