using System;

namespace Frontenac.Blueprints.Contracts
{
    public static class EdgeContract
    {
        public static void ValidateGetVertex(Direction direction)
        {
            if (direction == Direction.Both)
                throw new ArgumentException("direction cannot equal Direction.Both.");
        }
    }
}