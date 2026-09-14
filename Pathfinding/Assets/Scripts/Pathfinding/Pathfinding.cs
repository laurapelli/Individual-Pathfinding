using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class Pathfinding : MonoBehaviour
{

	public Transform  mSeeker;
  public Transform  mTarget;   

  NodePathfinding              CurrentStartNode;
  NodePathfinding              CurrentTargetNode;

	Grid              Grid;

  int               Iterations = 0;
  float             LastStepTime = 0.0f;
  float             TimeBetweenSteps = 0.01f;

  bool              EightConnectivity = true;


  /***************************************************************************/

	void Awake()
  {
		Grid = GetComponent<Grid> ();

    Iterations = 0;
    LastStepTime = 0.0f;
	}

  /***************************************************************************/

	void Update()
  {
    // Positions changed?
    if( PathInvalid() ){
      // Remove old path
      if( Grid.path != null ){
        Grid.path.Clear();
      }
      // Start calculating path again
      Iterations = 0;
      if( TimeBetweenSteps == 0.0f ){
        Iterations = -1;
      }
      FindPath(mSeeker.position, mTarget.position, Iterations );
    }
    else{
      // Path found?
      if( Iterations >= 0 ){
        // One or more iterations?
        if( TimeBetweenSteps == 0.0f ){
          // One iteration, look until path is found
          Iterations = -1;
          FindPath(mSeeker.position, mTarget.position, Iterations );
        }
        else if( Time.time > LastStepTime + TimeBetweenSteps ){
          // Iterate increasing depth every time step
          LastStepTime = Time.time;
          Iterations++;
          FindPath(mSeeker.position, mTarget.position, Iterations );
        }
      }
    }
	}

  /***************************************************************************/

	bool PathInvalid()
  {
    return CurrentStartNode != Grid.NodeFromWorldPoint(mSeeker.position) || CurrentTargetNode != Grid.NodeFromWorldPoint(mTarget.position) ;
  }

    /***************************************************************************/

    public List<NodePathfinding> FindPath( Vector3 startPos, Vector3 targetPos, int iterations )
  {
		CurrentStartNode  = Grid.NodeFromWorldPoint(startPos);
		CurrentTargetNode = Grid.NodeFromWorldPoint(targetPos);

		List<NodePathfinding> openSet      = new List<NodePathfinding>();
		HashSet<NodePathfinding> closedSet = new HashSet<NodePathfinding>();
		openSet.Add(CurrentStartNode);
        Grid.openSet    = openSet;

        int currentIteration = 0;
        NodePathfinding node = CurrentStartNode;
		while( openSet.Count > 0 && node != CurrentTargetNode && ( iterations == -1 || currentIteration < iterations ) ){

            /****  SELECCIONAR EL MEJOR NODO DE OPENSET (MENOR fCost)  ****/
            // Select best node from open list
            node = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < node.fCost || (openSet[i].fCost == node.fCost && openSet[i].hCost < node.hCost))
                {
                    node = openSet[i];
                }
            }
            /****/

            // Manage open/closed list
            openSet.Remove(node);
		    closedSet.Add(node);
            Grid.openSet    = openSet;
            Grid.closedSet  = closedSet;



            // Check destination
		    if (node != CurrentTargetNode) {

                // Open neighbours
                foreach (NodePathfinding neighbour in Grid.GetNeighbours(node, EightConnectivity)) {
                    /****  VERIFICAR SI EL NODO VECINO ES CAMINABLE Y NO ESTÁ CERRADO  ****/
                    if (!neighbour.mWalkable || closedSet.Contains(neighbour))
                        continue;

                    float newMovementCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = Heuristic(neighbour, CurrentTargetNode);
                        neighbour.mParent = node;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                    /****/
                }

                currentIteration++;
            }
            else{
                // Path found!
                return RetracePath(CurrentStartNode, CurrentTargetNode);

                // Path found
                Iterations = -1;

                Debug.Log("Statistics:");
                Debug.LogFormat("Total nodes:  {0}", openSet.Count + closedSet.Count );
                Debug.LogFormat("Open nodes:   {0}", openSet.Count );
                Debug.LogFormat("Closed nodes: {0}", closedSet.Count );
            }
		}
        return null;
	}

    /***************************************************************************/

    List<NodePathfinding> RetracePath(NodePathfinding startNode, NodePathfinding endNode)
    {
		List<NodePathfinding> path = new List<NodePathfinding>();
        NodePathfinding currentNode = endNode;

        /****  RETROCEDER DESDE EL NODO DESTINO AL INICIO PARA OBTENER EL CAMINO  ****/
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.mParent;
        }

        


        path.Reverse();

        // Aplicar suavizado al camino 
        SmoothPath(path);
        /****/

        Grid.path = path;

        List<NodePathfinding> finalPath = new List<NodePathfinding>();
        foreach (NodePathfinding node in path)
        {
            finalPath.Add(node);
        }

        return finalPath;
    }

  /***************************************************************************/

	float GetDistance(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        // Distance function
        //nodeA.mGridX   nodeB.mGridX
        //nodeA.mGridY   nodeB.mGridY
        int dstX = Mathf.Abs(nodeA.mGridX - nodeB.mGridX);
        int dstY = Mathf.Abs(nodeA.mGridY - nodeB.mGridY);
        if (EightConnectivity) {
            /****/
            // Diagonal distance
            if (dstX > dstY) {
                return nodeB.mCostMultiplier * (14 * dstY + 10 * (dstX - dstY));
            }
            else {
                return nodeB.mCostMultiplier * (14 * dstX + 10 * (dstY - dstX));
            }
        }
        /****/
    
        else
        {
            /****/
            // Manhattan distance
            return nodeB.mCostMultiplier * 10 * (dstX + dstY);
            /****/
        }
    }

  /***************************************************************************/

	float Heuristic(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        // Heuristic function
        //nodeA.mGridX   nodeB.mGridX
        //nodeA.mGridY   nodeB.mGridY
        int dstX = Mathf.Abs(nodeA.mGridX - nodeB.mGridX);
        int dstY = Mathf.Abs(nodeA.mGridY - nodeB.mGridY);

        /****/
        // Si hay diagonales, usamos la distancia octil
        if (EightConnectivity)
        {
            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            else
                return 14 * dstX + 10 * (dstY - dstX);
        }
        else
        {
            /****/
            // Manhattan distance
            return  10 * (dstX + dstY);
            /****/
        }
        /****/

    }

    /***************************************************************************/

    void SmoothPath(List<NodePathfinding> path)
    {
        // Si el camino tiene menos de 2 nodos, no hay nada que suavizar
        if (path.Count < 2)
        {
            return;
        }

        List<NodePathfinding> smoothedPath = new List<NodePathfinding>();
        smoothedPath.Add(path[0]);  // El primer nodo siempre está en el camino

        // Comienza con el primer nodo
        NodePathfinding lastValidNode = path[0];

        for (int i = 1; i < path.Count; i++)
        {
            // Verifica si el camino directo entre el último nodo válido
            // y el nodo actual es transitable y tiene un coste razonable
            if (BresenhamWalkable(lastValidNode.mGridX, lastValidNode.mGridY, path[i].mGridX, path[i].mGridY, lastValidNode.gCost))
            {
                // Si es transitable, no agregamos este nodo al camino y continuamos
                continue;
            }

            // Si no es transitable, agregamos el último nodo válido al camino
            smoothedPath.Add(path[i - 1]);
            lastValidNode = path[i - 1];
        }

        // Siempre agregamos el último nodo del camino
        smoothedPath.Add(path[path.Count - 1]);

        // Reemplazamos el camino original con el suavizado
        path.Clear();
        path.AddRange(smoothedPath);

    }
    /***************************************************************************/

    public bool BresenhamWalkable(int x, int y, int x2, int y2, float gCostStart)
    {

        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0)
            {
                dy2 = -1;
            }
            else if (h > 0)
            {
                dy2 = 1;
            }
            dx2 = 0;
        }
        int numerator = longest >> 1;

        // Tener en cuenta el coste 
        float totalGCost = 0;
        float gCostThreshold = gCostStart * 0.1f;

        for (int i = 0; i <= longest; i++)
        {
            NodePathfinding node = Grid.GetNode(x, y);
            totalGCost += node.mCostMultiplier;

            if (!node.mWalkable ||(node.mCostMultiplier > 1f && totalGCost > gCostThreshold)  || !IsFourConnected(x, y))
            {
                return false;
            }

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }

        return true;
    }

    private bool IsFourConnected(int x, int y)
    {
        // Comprobar la conectividad en las cuatro direcciones
        return Grid.GetNode(x + 1, y)?.mWalkable == true ||
               Grid.GetNode(x - 1, y)?.mWalkable == true ||
               Grid.GetNode(x, y + 1)?.mWalkable == true ||
               Grid.GetNode(x, y - 1)?.mWalkable == true;
    }

}
