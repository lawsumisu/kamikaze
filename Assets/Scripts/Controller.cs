using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    public GameObject treePrefab;


	// Use this for initialization
	void Start () {
        int rows = 5;
        int columns = 3;
        Vector3 scale = transform.localScale;
        int count = 0;
		for (float i = -scale.x / 2; i <= scale.x / 2; i += scale.x / (rows - 1)) {
            for (float j = -scale.z / 2; j <= scale.z / 2; j += scale.z / (columns - 1)) {
                float x = Mathf.Clamp(i + Random.Range(-5, 5), -scale.x / 2, scale.x / 2);
                float z = Mathf.Clamp(j + Random.Range(-5, 5), -scale.z / 2, scale.y / 2);
                GameObject go = Instantiate(treePrefab);
                go.name = string.Format("Tree #{0}", ++count);
                go.transform.position = new Vector3(x, 0, z);
            }
            
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
