using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    public GameObject treePrefab;

	// Use this for initialization
	void Start () {
        int rows = 5;
        int columns = 3;
		for (int i = -20; i <= 20; i += 10) {
            for (int j = -10; j <= 10; j += 7) {
                float x = Random.Range(-1, 1);
                float z = Random.Range(-2, 2);
                GameObject go = Instantiate(treePrefab);
                go.transform.position = new Vector3(i + x, 0, j + z);
            }
            
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
