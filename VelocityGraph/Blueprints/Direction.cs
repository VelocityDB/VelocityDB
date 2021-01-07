namespace Frontenac.Blueprints
{
    public enum Direction
    {
        Out,
        In,
        Both
    }

    public static class Directions
    {
        public static readonly Direction[] Proper = new[]
            {
                Direction.Out,
                Direction.In
            };

        public static Direction Opposite(this Direction direction)
        {
            if (direction == Direction.Out)
                return Direction.In;
            if (direction == Direction.In)
                return Direction.Out;
            return Direction.Both;
        }
    }
}