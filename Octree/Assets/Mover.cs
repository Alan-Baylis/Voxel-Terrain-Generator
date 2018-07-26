using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public float moveSpdX, moveSpdZ, moveSpdY;
    public float rotSpd;
    float boost = 2;

    GameObject lastCube;
    public bool dragging = false;


    // Use this for initialization
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            boost++;
        }
        else
        {
            boost = 1;
        }

        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * moveSpdZ * boost, Input.GetAxis("Jump") * Time.deltaTime * moveSpdY * boost, Input.GetAxis("Vertical") * Time.deltaTime * moveSpdX * boost, Space.Self);
        transform.Rotate(0f, Input.GetAxis("Mouse X") * Time.deltaTime * rotSpd, 0f, Space.World);
        transform.Rotate(-Input.GetAxis("Mouse Y") * Time.deltaTime * rotSpd, 0f, 0f, Space.Self);

        if (Input.GetMouseButtonDown(0))
        {
            GameObject newC = (GameObject)Instantiate(Resources.Load("Cube"), transform.position + transform.forward * 6, Quaternion.identity);
        }
        if (Input.GetMouseButtonUp(1))
        {
            dragging = false;
            if (lastCube != null)
                lastCube.transform.parent = null;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            if (hit.transform.tag == "OctCube")
            {
                if (ReferenceEquals(hit.transform.gameObject, lastCube))
                {
                    if (Input.GetMouseButton(1))
                    {
                        lastCube.transform.parent = transform;
                        lastCube.GetComponent<Rigidbody>().isKinematic = true;
                        dragging = true;
                    }
                }
                else
                {
                    if (!dragging)
                    {
                        resetLastCube();
                        hit.transform.GetComponent<Renderer>().material.color = Color.red;
                    }
                }

                if (!dragging)
                    lastCube = hit.transform.gameObject;
            }

        }
        else
        {
            resetLastCube();
            lastCube = null;
        }
    }

    void resetLastCube()
    {
        if (lastCube != null)
        {
            lastCube.transform.GetComponent<Renderer>().material.color = Color.white;
            lastCube.transform.parent = null;
            lastCube.GetComponent<Rigidbody>().isKinematic = false;

        }
    }
}
