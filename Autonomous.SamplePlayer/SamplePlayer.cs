﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Autonomous.Public;

namespace Autonomous.SamplePlayer
{
    [Export(typeof(IPlayer))]
    [ExportMetadata("PlayerName", "Example")]
    public class SamplePlayer : IPlayer
    {
        private string _playerId;

        public void Finish()
        {
        }

        public void Initialize(string playerId)
        {
            _playerId = playerId;
        }

        public PlayerAction Update(GameState gameState)
        {
            var self = gameState.GameObjectStates.First(o => o.Id == _playerId);
            var gameObjects = gameState.GameObjectStates.Where(g => g.GameObjectType != GameObjectType.Player).ToList();

            var objectInFront = GetClosestObjectInFront(gameObjects, self);

            float accelerationY = 1;
            bool left = false, right = false;

            float desiredX = GameConstants.LaneWidth / 2;

            if (objectInFront != null)
            {
                float otherDistanceToStop = objectInFront.DistanceToStop;
                float selfDistancveToStop = self.DistanceToStop;

                float distanceBetweenCars = CalculateDistance(self, objectInFront);
                float plusSafeDistance = 5;
                if (distanceBetweenCars < selfDistancveToStop - otherDistanceToStop + plusSafeDistance && self.VY > 0)
                    accelerationY = -1;
            }

            float centerX = (self.BoundingBox.Left + self.BoundingBox.Right) / 2;

            if (Math.Abs(desiredX - centerX) > 0.2)
            {
                if (desiredX < centerX)
                    left = true;
                else
                    right = true;
            }
                
            return new PlayerAction() { MoveLeft = left, MoveRight = right, Acceleration = accelerationY };
        }

        private static float CalculateDistance(GameObjectState self, GameObjectState objectInFront)
        {
            float selfCenterY = self.BoundingBox.CenterY;
            float otherCenterY = objectInFront.BoundingBox.CenterY;

            float distanceBetweenCars = Math.Abs(selfCenterY - otherCenterY) - self.BoundingBox.Height / 2 - objectInFront.BoundingBox.Height / 2;
            return distanceBetweenCars;
        }

        private GameObjectState GetClosestObjectInFront(IEnumerable<GameObjectState> objects, GameObjectState self)
        {
            return objects
                          .Where(o => o.BoundingBox.Y > self.BoundingBox.Y)
                          .FirstOrDefault(o => IsOverlappingHorizontally(self, o));
        }

        private bool IsOverlappingHorizontally(GameObjectState self, GameObjectState other)
        {
            var r1 = self.BoundingBox;
            var r2 = other.BoundingBox;
            return Between(r1.Left, r1.Right, r2.Left) ||
                   Between(r1.Left, r1.Right, r2.Right) ||
                   Between(r2.Left, r2.Right, r1.Right) ||
                   Between(r2.Left, r2.Right, r1.Left);
        }

        private static bool Between(float limit1, float limit2, float value)
        {
            return (value >= limit1 && value <= limit2) || (value >= limit2 && value <= limit1);
        }

    }
}
