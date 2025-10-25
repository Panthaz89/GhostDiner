using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]private Rigidbody rb;
    [SerializeField]private float rollSpeed;
    [SerializeField]private Transform cameraTransform;

    public float size = 2;

    public LayerMask ballLayer;
    public int playerLayer;


    // Update is called once per frame
    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 movement = (input.z * cameraTransform.forward) + (input.x * cameraTransform.right);
        rb.AddForce(movement * rollSpeed * Time.fixedDeltaTime * size);
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Prop"))
        {
            Destroy(collision.rigidbody); //Delete rigid body weirdness
            collision.transform.SetParent(transform);
            collision.gameObject.layer = playerLayer;
            

            RaycastHit hit;
            if(Physics.Raycast(collision.transform.position, (transform.position - collision.transform.position).normalized, out hit, Mathf.Infinity, ballLayer)){
                collision.transform.forward = hit.normal;
                collision.transform.position = hit.point;
                collision.transform.position = collision.transform.position + collision.transform.forward * collision.transform.localScale.z *0.5f;
            }
            //collision.rigidbody.transform.parent = transform;

            


            


            //Transform keeps rotation/scale tied to parent, which may be a problem later
            //Maybe make the objects have 0 mass, no collision, and then
            //get meshed into the ball?
            

            //Option 1: Mega jank parts have collision
            //Option 2: Ball movement (boring??)
        }
    }

}

