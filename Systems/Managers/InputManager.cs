using System;
using Godot;
using Vikare.Entities;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> Manages the input during game runtime. Translates raw Godot input events into <c>IInputIntent</c> instances and dispatches them to the registered player's state machine each frame. </summary>
    public partial class InputManager : SingletonNode<InputManager>
    {
        /// <summary> Analogue deadzone. Filters out stick drift below this magnitude. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _analogueDeadzone = 0.2f;

        /// <summary> The intent submitted the previous frame. A null indicates that there wasn't one. </summary>
        private ActionIntent? _previousInput;

        /// <summary> The player actor currently receiving input; null when no player has registered. </summary>
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
            // Get the current direction this frame.
            Vector2 direction = Input.GetVector(
                "action_left",
                "action_right",
                "action_up",
                "action_down",
                _analogueDeadzone);

            // If there is a player, we'll go through categories of actions. Each category overwrites the previous.
            if (_player != null)
            {
                ActionIntent? intent = null;

                if (Input.IsActionPressed("action_sprint"))
                {
                    intent = new SprintIntent(direction);
                }

                if (Input.IsActionJustPressed("action_attack_light"))
                {
                    intent = new AbilityIntent(direction, AttackIntent.AttackKind.Light);
                }
                else if (Input.IsActionJustPressed("action_attack_heavy"))
                {
                    intent = new AbilityIntent(direction, AttackIntent.AttackKind.Heavy);
                }

                // Dodge / block should always interrupt.
                if (Input.IsActionJustPressed("action_dodge"))
                {
                    intent = new DodgeIntent(direction);
                }
                else if (Input.IsActionJustPressed("action_block"))
                {
                    intent = new BlockIntent(direction);
                }

                // If there has been no other inputs, default to walking.
                if (intent == null)
                {
                    intent = new WalkIntent(direction);
                }

                _player!.Machine.HandleIntent(intent);
                _previousInput = intent;

                //DispatchCastPower("action_power_00", _player!.Power1);
                //DispatchCastPower("action_power_01", _player!.Power2);
                //DispatchCastPower("action_power_02", _player!.Power3);
                //DispatchCastPower("action_power_03", _player!.Power4);
            }
        }
    }
}
