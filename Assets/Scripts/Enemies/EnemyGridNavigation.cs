using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class EnemyGridNavigation
    {
        // grid directions
        private static readonly Vector2Int[] Neighbours =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };

        private readonly NavMeshAgent agent;

        //  max distance research
        private readonly int resolveSearchRadius;

        private readonly int maxExpandedNodes;


        private readonly float sampleMaxDistance;

        private readonly float maxHorizontalSampleOffset;


      
        // valid cells buffer
        private readonly Dictionary<Vector2Int, Vector3> validCenters = new();

        // invalid cells buffer
        private readonly HashSet<Vector2Int> invalidCells = new();




        // nodes to analyze
        private readonly List<Vector2Int> open = new();

        // nodes analyzed
        private readonly HashSet<Vector2Int> closed = new();

        private readonly Dictionary<Vector2Int, Vector2Int> cameFrom = new();

        private readonly Dictionary<Vector2Int, int> gScore = new();


        /// navigation system
        public EnemyGridNavigation(NavMeshAgent agent, int resolveSearchRadius, int maxExpandedNodes, float sampleMaxDistance = 3f, float maxHorizontalSampleOffset = 0.2f)
        {
            this.agent = agent;
            this.resolveSearchRadius = Mathf.Max(0, resolveSearchRadius);
            this.maxExpandedNodes = Mathf.Max(128, maxExpandedNodes);
            this.sampleMaxDistance = Mathf.Max(0.1f, sampleMaxDistance);
            this.maxHorizontalSampleOffset = Mathf.Max(0.01f, maxHorizontalSampleOffset);
        }



        // real path to grid path conversion
        public Vector2Int WorldToCell(Vector3 position)
        {
            return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        }


        public bool TryGetCellCenter(Vector2Int cell, out Vector3 center)
        {

            if (validCenters.TryGetValue(cell, out center))
                return true;

            if (invalidCells.Contains(cell))
            {
                center = Vector3.zero;
                return false;
            }

            if (agent == null)
            {
                center = Vector3.zero;
                return false;
            }

        
            Vector3 requestedPosition = new Vector3(cell.x, agent.nextPosition.y, cell.y);

            if (!NavMesh.SamplePosition(requestedPosition, out NavMeshHit hit, sampleMaxDistance, agent.areaMask))
            {
                invalidCells.Add(cell);
                center = Vector3.zero;
                return false;
            }

            Vector2 horizontalDifference = new Vector2(hit.position.x - cell.x, hit.position.z - cell.y);

            if (horizontalDifference.sqrMagnitude > maxHorizontalSampleOffset * maxHorizontalSampleOffset)
            {
                invalidCells.Add(cell);
                center = Vector3.zero;
                return false;
            }

            center = hit.position;
            validCenters[cell] = center;

            return true;
        }

        private bool IsCellWalkable(Vector2Int cell)
        {
            return TryGetCellCenter(cell, out _);
        }


        // pathfinding increasing manhattan distance
        public bool TryFindNearestWalkableCell(Vector2Int requestedCell, out Vector2Int result)
        {
            if (IsCellWalkable(requestedCell))
            {
                result = requestedCell;
                return true;
            }
            //radial research for a walkable path
            for (int radius = 1; radius <= resolveSearchRadius; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int z = radius - Mathf.Abs(x);

                    Vector2Int first = requestedCell + new Vector2Int(x, z);

                    if (IsCellWalkable(first))
                    {
                        result = first;
                        return true;
                    }

                    if (z == 0)
                        continue;

                    Vector2Int second = requestedCell + new Vector2Int(x, -z);

                    if (IsCellWalkable(second))
                    {
                        result = second;
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        //try to move avoiding diagonal movements
        private bool CanMoveBetween(Vector2Int from, Vector2Int to)
        {
            Vector2Int difference = to - from;

            if (Mathf.Abs(difference.x) + Mathf.Abs(difference.y) != 1)
                return false;
            if (!TryGetCellCenter(from, out Vector3 fromCenter) || !TryGetCellCenter(to, out Vector3 toCenter))
                return false;

            // controlling if there is an obstacle between 2 cells
            bool blocked = NavMesh.Raycast(fromCenter, toCenter, out _, agent.areaMask);

            return !blocked;
        }



        // pathfinding using A* to allow enemies to avoid obstacles if possible
        public bool TryFindPath(Vector2Int start, Vector2Int requestedTarget, List<Vector2Int> result)
        {
            result.Clear();

            validCenters.Clear();
            invalidCells.Clear();

            if (!TryFindNearestWalkableCell(start, out Vector2Int resolvedStart) ||
                !TryFindNearestWalkableCell(requestedTarget, out Vector2Int target))
            {
                return false;
            }

            if (resolvedStart == target)
            {
                result.Add(resolvedStart);
                return true;
            }

            // reset A*.
            open.Clear();
            closed.Clear();
            cameFrom.Clear();
            gScore.Clear();

            open.Add(resolvedStart);
            gScore[resolvedStart] = 0;

            int expandedNodes = 0;

            while (open.Count > 0 && expandedNodes < maxExpandedNodes)
            {
                int bestIndex = GetBestNodeIndex(target);
                Vector2Int current = open[bestIndex];

                open.RemoveAt(bestIndex);

                if (current == target)
                {
                    ReconstructPath(current, result);
                    return true;
                }

                closed.Add(current);
                expandedNodes++;

                foreach (Vector2Int direction in Neighbours)
                {
                    Vector2Int neighbour = current + direction;

                    if (closed.Contains(neighbour) || !CanMoveBetween(current, neighbour))
                        continue;

                    // calculating movement cost
                    int tentativeG = gScore[current] + 1;

                    if (gScore.TryGetValue(neighbour, out int previousG) && tentativeG >= previousG)
                        continue;

                    // add best path 
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeG;

                    if (!open.Contains(neighbour))
                        open.Add(neighbour);
                }
            }

            // no path available
            return false;
        }


        //choosing best node to walk in
        private int GetBestNodeIndex(Vector2Int target)
        {
            int bestIndex = 0;
            int bestF = int.MaxValue;
            int bestH = int.MaxValue;

            for (int i = 0; i < open.Count; i++)
            {
                Vector2Int cell = open[i];

                int g = gScore.TryGetValue(cell, out int score) ? score : int.MaxValue;

                //using manhattan distance to avoid diagonals
                int h = Mathf.Abs(cell.x - target.x) + Mathf.Abs(cell.y - target.y);

                int f = g + h;

                if (f >= bestF && (f != bestF || h >= bestH))
                    continue;

                bestIndex = i;
                bestF = f;
                bestH = h;
            }

            return bestIndex;
        }


        // build the path with the saved nodes, from target to start
        /// </summary>
        private void ReconstructPath(Vector2Int current, List<Vector2Int> result)
        {
            result.Clear();

            result.Add(current);

            while (cameFrom.TryGetValue(current, out Vector2Int previous))
            {
                current = previous;
                result.Add(current);
            }

            result.Reverse();
        }
    }
}