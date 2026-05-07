using Godot;
using System;
using System.Collections.Generic;

namespace Vikare.Types
{
    [GlobalClass]
    public partial class SteeringAgent2D : NavigationAgent2D
    {
        public static readonly List<SteeringAgent2D> AllAgents
            = new();

        [ExportGroup("Movement")]
        [Export] public float MoveSpeed = 100f;

        [ExportGroup("Detection")]
        [Export] public float DetectionRadius = 80f;
        [Export] public int DirectionSamples = 16;

        [ExportGroup("Steering Weights")]
        [Export] public float DangerWeight = 1.5f;
        [Export] public float SideBiasStrength = 0.15f;
        [Export] public float VelocityPredictionWeight = 1.2f;

        [ExportGroup("Debug")]
        [Export] public bool DebugDraw = false;

        private float[] interest;
        private float[] danger;
        private Vector2[] directions;

        private CharacterBody2D body;

        public override void _Ready()
        {
            AllAgents.Add(this);

            body = GetParent<CharacterBody2D>();

            interest = new float[DirectionSamples];
            danger = new float[DirectionSamples];
            directions = new Vector2[DirectionSamples];

            BuildDirections();
        }

        public override void _ExitTree()
        {
            AllAgents.Remove(this);
        }

        private void BuildDirections()
        {
            for (int i = 0; i < DirectionSamples; i++)
            {
                float angle =
                    Mathf.Tau * i / DirectionSamples;

                directions[i] =
                    Vector2.Right.Rotated(angle);
            }
        }

        public Vector2 GetSteeringDirection()
        {
            Vector2 nextPathPoint =
                GetNextPathPosition();

            Vector2 desiredDirection =
                body.GlobalPosition.DirectionTo(
                    nextPathPoint
                );

            CalculateInterest(desiredDirection);
            CalculateDanger(desiredDirection);

            Vector2 finalDirection =
                ChooseDirection();

            return finalDirection;
        }

        private void CalculateInterest(
            Vector2 desiredDirection
        )
        {
            for (int i = 0; i < DirectionSamples; i++)
            {
                float alignment =
                    directions[i].Dot(desiredDirection);

                interest[i] =
                    Mathf.Max(0, alignment);
            }
        }

        private void CalculateDanger(
            Vector2 desiredDirection
        )
        {
            Array.Fill(danger, 0f);

            foreach (var other in AllAgents)
            {
                if (other == this)
                    continue;

                Vector2 offset =
                    other.body.GlobalPosition -
                    body.GlobalPosition;

                float distance = offset.Length();

                if (distance > DetectionRadius)
                    continue;

                Vector2 toAgent =
                    offset.Normalized();

                /*
                 * Distance weighting
                 * Closer agents = more dangerous
                 */
                float distanceWeight =
                    1.0f - (distance / DetectionRadius);

                /*
                 * Relative velocity prediction
                 */
                Vector2 relativeVelocity =
                    other.body.Velocity -
                    body.Velocity;

                float closingFactor =
                    Mathf.Max(
                        0,
                        relativeVelocity.Dot(toAgent)
                    );

                float velocityWeight =
                    1.0f +
                    (closingFactor *
                     VelocityPredictionWeight);

                /*
                 * Side bias
                 * Encourages all agents
                 * to pass on the same side
                 */
                float side =
                    Mathf.Sign(
                        desiredDirection.Cross(toAgent)
                    );

                for (int i = 0; i < DirectionSamples; i++)
                {
                    float alignment =
                        directions[i].Dot(toAgent);

                    alignment =
                        Mathf.Max(0, alignment);

                    float sideBias =
                        directions[i]
                            .Rotated(side *
                                     SideBiasStrength)
                            .Dot(toAgent);

                    sideBias =
                        Mathf.Max(0, sideBias);

                    danger[i] +=
                        alignment *
                        distanceWeight *
                        velocityWeight *
                        DangerWeight;

                    danger[i] +=
                        sideBias * 0.2f;
                }
            }
        }

        private Vector2 ChooseDirection()
        {
            Vector2 result = Vector2.Zero;

            for (int i = 0; i < DirectionSamples; i++)
            {
                float score =
                    Mathf.Max(
                        0,
                        interest[i] - danger[i]
                    );

                result += directions[i] * score;
            }

            if (result == Vector2.Zero)
                return Vector2.Zero;

            return result.Normalized();
        }
    }
}
