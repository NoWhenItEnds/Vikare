using System;
using Godot;
using Microsoft.Extensions.Logging;
using Vikare.Entities.GOAP.Advertisers;
using Vikare.Utilities.Logging;

namespace Vikare.Entities
{
    /// <summary> An entity that does not move under its own power and does not participate in GOAP planning as an actor. </summary>
    /// <remarks> On <c>_Ready</c>, each advertiser blueprint in is deep-duplicated. </remarks>
    public partial class Prop : Entity
    {
        /// <summary> Authoring-time advertiser blueprints registered with the entity in <c>_Ready</c>. </summary>
        [ExportGroup("Resources")]
        [Export] public Godot.Collections.Array<ActionAdvertiser> AdvertiserBlueprints { get; set; } = new Godot.Collections.Array<ActionAdvertiser>();


        /// <summary> Resolved logger for props. </summary>
        private static readonly ILogger Logger = Log.For<Prop>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            base._Ready();

            // Each blueprint is deep-duplicated before use because references are shared by default.
            foreach (ActionAdvertiser blueprint in AdvertiserBlueprints)
            {
                if (blueprint == null)
                {
                    Logger.LogError($"{Name}: null entry in AdvertiserBlueprints; skipping.");
                }
                else
                {
                    ActionAdvertiser? duplicate = blueprint.Duplicate(true) as ActionAdvertiser;

                    if (duplicate == null)
                    {
                        Logger.LogError($"{Name}: duplicated advertiser blueprint '{blueprint.GetType().Name}' could not be cast back to ActionAdvertiser; skipping.");
                    }
                    else
                    {
                        duplicate.Initialise(this);
                        TryAddAdvertiser(duplicate);
                    }
                }
            }
        }


        /// <inheritdoc/>
        public override void PlayAnimation(String animationName, Vector2 direction)
        {
            throw new InvalidOperationException(
                $"{nameof(Prop)}.{nameof(PlayAnimation)} is not yet implemented. Props do not have a Race concept and the sprite-frames key path for non-Actor entities has not been designed. Wire this up before requesting animations on Props.");
        }
    }
}
