using System;
using Godot;
using Vikare.Entities;
using Vikare.Entities.Abilities;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> Translates raw Godot input events into <see cref="ActionIntent"/> instances and dispatches them to the registered player's state machine each frame. </summary>
    public partial class InputManager : SingletonNode<InputManager>
    {
        /// <summary> Analogue stick deadzone; vectors below this magnitude are treated as zero to suppress hardware drift. Valid range: 0.0–1.0. </summary>
        [ExportGroup("Settings")]
        [Export(PropertyHint.Range, "0.0,1.0,")] private Single _analogueDeadzone = 0.2f;

        /// <summary> Designer-editable list that maps Godot input actions to ability categories. The order resolves importance (0 = Highest priority). </summary>
        [Export] public Godot.Collections.Array<AbilityBinding> AbilityBindings { get; set; } = new();


        /// <summary> Player actor currently receiving input; null when no player has registered. </summary>
        private Actor? _player;


        /// <summary> Registers an actor as the recipient of all translated input intents. </summary>
        /// <param name="player"> The actor that should receive player input. </param>
        public void RegisterPlayer(Actor player)
        {
            if (_player != null)
            {
                // TODO - Make a proper logging system.
                GD.PushWarning(
                    $"[InputManager] SetPlayer called whilst '{_player.Name}' is already registered. " +
                    $"Overwriting with '{player.Name}'.");
                DeregisterPlayer();
            }

            _player = player;
        }


        /// <summary> Unregisters the current player, stopping all input dispatch. Safe to call when no player is registered. </summary>
        public void DeregisterPlayer()
        {
            // TODO - Ensure that, if a player is removed, it deregisters itself on ExitTree();
            _player = null;
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            Vector2 direction = Input.GetVector("action_left", "action_right", "action_up", "action_down", _analogueDeadzone);

            if (_player != null)
            {
                ActionIntent intent = BuildIntent(direction, _player);
                _player.Machine.HandleIntent(intent);
            }
        }


        /// <summary>
        /// Resolves all input actions for this frame into a single intent according to a fixed
        /// priority. Higher entries in the list below always win a same-frame tie.
        ///
        /// Priority (highest first):
        ///   1. Dodge
        ///   2. Block
        ///   3. Movement ability
        ///   4. Defensive ability
        ///   5. Heavy attack
        ///   6. Projectile ability
        ///   7. Utility ability
        ///   8. Light attack
        ///   9. Sprint
        ///  10. Walk (default)
        /// </summary>
        /// <param name="direction"> Movement direction vector for the current frame. </param>
        /// <param name="player"> The actor whose ability slots are queried during intent resolution. </param>
        private ActionIntent BuildIntent(Vector2 direction, Actor player)
        {
            ActionIntent result = new WalkIntent(direction);

            if (Input.IsActionPressed("action_sprint"))
            {
                result = new SprintIntent(direction);
            }

            AbilityEffect? ability = TryGetAbility(player);
            if (ability != null)
            {
                result = new AbilityIntent(direction, ability);
            }

            if (Input.IsActionJustPressed("action_block"))
            {
                result = new BlockIntent(direction);
            }

            if (Input.IsActionJustPressed("action_dodge"))
            {
                result = new DodgeIntent(direction);
            }

            return result;
        }


        /// <summary> Check to see if an ability was pressed. The first binding matching the input will be chosen. </summary>
        /// <param name="actor"> The actor whose <see cref="Actor.GetAbility"/> is queried on a match. </param>
        private AbilityEffect? TryGetAbility(Actor actor)
        {
            AbilityEffect? result = null;

            foreach (AbilityBinding binding in AbilityBindings)
            {
                if(result == null)
                {
                    if (Input.IsActionJustPressed(binding.Action))
                    {
                        result = actor.GetAbility(binding.Category);
                    }
                }
            }

            return result;
        }
    }


    /// <summary> Maps one Godot input action to one ability category. </summary>
    [GlobalClass]
    public partial class AbilityBinding : Resource
    {
        /// <summary>Godot input action name (e.g. "action_attack_light"), as registered in Project &gt; Input Map.</summary>
        [Export] public StringName Action { get; set; } = "";

        /// <summary>The ability slot this action should trigger a lookup for.</summary>
        [Export] public AbilityCategory Category { get; set; }
    }
}
