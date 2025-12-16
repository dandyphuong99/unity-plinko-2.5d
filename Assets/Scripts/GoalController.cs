using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalController : MonoBehaviour
{
	//public float vitesse;
    //private Rigidbody2D rb;
    private bool isScored = false;

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody2D>();
        
    }

    //// Update is called once per frame
    //void FixedUpdate()
    //{
    //    float moveHorizontal = Input.GetAxis("Horizontal");
    //    float moveVertical = Input.GetAxis("Vertical");
    //    Vector3 mouvement = new Vector3(moveHorizontal, 0.0f, moveVertical);
    //    rb.AddForce(mouvement*vitesse);
    //}

    /// Khi va chạm
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Khi bóng chạm cột
        if (collision.gameObject.CompareTag("Ball"))
        {
            // Random lực ngang mạnh hơn
            ContactPoint2D contact = collision.contacts[0];
            Vector2 normal = contact.normal;

            // Lực đẩy ngược với normal, thêm chút random ngang
            Vector2 pushForce = -normal * 2f;
            pushForce.x += Random.Range(-0.5f, 0.5f);

            //rb.AddForce(pushForce, ForceMode2D.Impulse);

            if (!isScored)
            {
                isScored = true; // Đánh dấu đã ghi điểm để tránh ghi nhiều lần
                ScopeManager.Instance.IncreaseScope();
            }
        }
    }


    


}
