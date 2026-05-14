public static class DirectionHelper
{
    public static Direction TurnLeft(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                return Direction.Down;
            case Direction.Up:
                return Direction.Left;
            case Direction.Right:
                return Direction.Up;
            case Direction.Down:
                return Direction.Right;
            default:
                throw new System.Exception("Invalid direction");
        }
    }

    public static Direction TurnRight(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                return Direction.Up;
            case Direction.Up:
                return Direction.Right;
            case Direction.Right:
                return Direction.Down;
            case Direction.Down:
                return Direction.Left;
            default:
                throw new System.Exception("Invalid direction");
        }
    }

    public static Direction RandomDirection()
    {
        int randomInt = UnityEngine.Random.Range(0, 4);
        switch (randomInt)
        {
            case 0:
                return Direction.Left;
            case 1:
                return Direction.Up;
            case 2:
                return Direction.Right;
            case 3:
                return Direction.Down;
            default:
                throw new System.Exception("Invalid random direction");
        }
    }
}