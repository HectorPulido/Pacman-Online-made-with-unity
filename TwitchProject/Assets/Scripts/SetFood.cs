using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFood : MonoBehaviour {

    public Vector2Int size;
    public float scale;
    public float circleScale;
    public GameObject foodPrefab;

	void Start ()
    {
        var go = new GameObject("Foods");
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                var o = new Vector3(i - size.x / 2f, j - size.y / 2f, 0) * scale;
                if (Physics2D.CircleCast(o, circleScale, Vector2.zero).collider == null)
                {
                    Instantiate(foodPrefab,
                                o,
                                Quaternion.identity,
                                go.transform);
                }
            }
        }
	    	
	}

}
