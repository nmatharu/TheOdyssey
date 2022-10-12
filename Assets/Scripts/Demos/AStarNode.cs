public class AStarNode
{
    public int X;
    public int Y;
    public AStarNode Parent;

    public AStarNode( int x, int y, AStarNode parent )
    {
        X = x;
        Y = y;
        Parent = parent;
    }
}