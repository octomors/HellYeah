using System;

[Serializable]
public class Walker : IIterativeGenerator
{
    public float ChanceOfTurningLeft { get; set; }
    public float ChanceOfWalkingStraight { get; set; }
    public float ChanceOfTurningRight { get; set; }
    private Direction walkDirection;
    private (int y, int x) coordinates;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startCoords"></param>
    /// <param name="straightness"> 0 to 1, The chance of continuing in the designated direction</param>
    /// <param name="walkDirection">The Direction to walk</param>
    public Walker((int y, int x) startCoords, Direction walkDirection,
    float chanceOfWalkingStraight = 0.6f, float chanceOfTurningRight = 0.2f, float chanceOfTurningLeft = 0.2f)
    {
        this.coordinates = startCoords;
        this.walkDirection = walkDirection;
        this.ChanceOfWalkingStraight = chanceOfWalkingStraight;
        this.ChanceOfTurningRight = chanceOfTurningRight;
        this.ChanceOfTurningLeft = chanceOfTurningLeft;
    }
    public (int y, int x) GetNextPoint()
    {
        float randomValue = UnityEngine.Random.value;
        Direction newWalkDirection = walkDirection;
        if (randomValue < ChanceOfTurningLeft)
        {
            newWalkDirection = DirectionHelper.TurnLeft(walkDirection);
        }
        else if (randomValue < ChanceOfTurningLeft + ChanceOfWalkingStraight)
        {
            // keep walking straight
        }
        else
        {
            newWalkDirection = DirectionHelper.TurnRight(walkDirection);
        }

        Walk(newWalkDirection);
        return coordinates;
    }
    
    private void Walk(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                coordinates.y++;
                break;
            case Direction.Down:
                coordinates.y--;
                break;
            case Direction.Left:
                coordinates.x--;
                break;
            case Direction.Right:
                coordinates.x++;
                break;
        }
    }
}