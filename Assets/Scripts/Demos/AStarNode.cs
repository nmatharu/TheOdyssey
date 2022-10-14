using UnityEngine;

public class AStarNode
{
    public int X;
    public int Y;
    public int GCost;
    public int HCost;
    public AStarNode Parent;

    public AStarNode( int x, int y )
    {
        X = x;
        Y = y;
        // Parent = parent;
    }

    public int FCost() => GCost + HCost;
}