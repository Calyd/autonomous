﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autonomous.Public;
using MonoGameTry.GameObjects;

namespace MonoGameTry.Strategies
{
    class BusStrategy : OvertakingStrategy
    {
        private bool stopped = false;
        private Stopwatch stopwatch = new Stopwatch();
        private GameObject _lastStopped;
        public BusStrategy(float desiredSpeed, int lane, IGameStateProvider gameStateProvider) : base(desiredSpeed, lane, gameStateProvider)
        {
            
        }

        public override ControlState Calculate()
        {
            if (stopped)
            {
                if (stopwatch.ElapsedMilliseconds > 5000)
                {             
                    stopped = false;
                }
                else
                {
                    return new ControlState() {Acceleration = 0, HorizontalSpeed = 0};
                }
            }
            var busStop = GetClosestBusStop(_gameStateProvider.GameStateInternal.GameObjects.Where(go => go!=_lastStopped));
            var distanceToStop = CalculateDistanceToStop(GameObject.VY, GameConstants.PlayerDeceleration);

            if (busStop == null || Math.Abs(busStop.Y - GameObject.Y) > distanceToStop+0.2f)
                return base.Calculate();

            float vx = 0;
            float targetX = GameObject.OppositeDirection  ? -GameConstants.RoadWidth / 2 + GameObject.Width / 2
                : GameConstants.RoadWidth / 2 - GameObject.Width / 2;

            if (targetX - GameObject.X < 0.1f)
                vx = 0;
            else if (targetX < GameObject.X)
                vx = -1;
            else
                vx = 1;

            if (GameObject.VY <= 0.1)
            {
                GameObject.VY = 0;
                stopped = true;
                stopwatch = Stopwatch.StartNew();
                _lastStopped = busStop;
                return new ControlState {Acceleration = 0, HorizontalSpeed = 0};
            }

            return new ControlState {Acceleration = -1, HorizontalSpeed = vx};
        }

        private GameObject GetClosestBusStop(IEnumerable<GameObject> objects)
        {
            if (!GameObject.OppositeDirection)
                return objects.Where(o => o != GameObject && o.Y > GameObject.Y).OrderBy(o => o.Y).FirstOrDefault(o => o.Type==GameObjectType.BusStop && o.X > 0);
            return objects.Where(o => o != GameObject && o.Y < GameObject.Y).OrderByDescending(o => o.Y).FirstOrDefault(o => o.Type == GameObjectType.BusStop && o.X < 0);
        }

    }
}