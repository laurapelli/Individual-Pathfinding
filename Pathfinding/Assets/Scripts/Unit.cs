using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
  public float      Speed = 10.0f;
  public GameObject Astar;


  List<NodePathfinding> mPath;

	int targetIndex;

  /***************************************************************************/

	void Start() {
	}

  /***************************************************************************/

  private void Update()
  {
    // Mouse click
    if( Input.GetMouseButtonDown(0))
    {
      // Raycast
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;

      // Hit?
      if (Physics.Raycast( ray, out hit, 1000.0f ) ){
        // Find path
        mPath = Astar.GetComponent<Pathfinding>().FindPath( transform.position, hit.point, -1 );

        // If a path was found follow it
        if( mPath != null){
          targetIndex = 0;
			    StopCoroutine("FollowPath");
			    StartCoroutine("FollowPath");
        }
      }
    }
  }

  /***************************************************************************/

	IEnumerator FollowPath() {
		Vector3 currentWaypoint = mPath[0].mWorldPosition;
		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex ++;
				if (targetIndex >= mPath.Count ){
					yield break;
				}
				currentWaypoint = mPath[targetIndex].mWorldPosition;
			}

			transform.position = Vector3.MoveTowards( transform.position, currentWaypoint, Speed * Time.deltaTime );
			yield return null;

		}
	}

  /***************************************************************************/

	public void OnDrawGizmos() {
		if (mPath != null) {
			for (int i = targetIndex; i < mPath.Count; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(mPath[i].mWorldPosition, Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine( transform.position, mPath[i].mWorldPosition );
				}
				else {
					Gizmos.DrawLine( mPath[i-1].mWorldPosition, mPath[i].mWorldPosition );
				}
			}
		}
	}

  /***************************************************************************/

}
