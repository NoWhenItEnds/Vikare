using System;
using Godot;
using Vikare.Entities;
using Vikare.Entities.Combat;
using Vikare.Entities.Intents;
using Vikare.Entities.Interfaces;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> Manages the input during game runtime. Translates raw Godot input events into <c>IInputIntent</c> instances and dispatches them to the registered player's state machine each frame. </summary>
    public partial class InputManager : SingletonNode<InputManager>
    {
        /// <summary> Analogue deadzone. Filters out stick drift below this magnitude. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _analogueDeadzone = 0.2f;


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
            _player = null;
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            if (_player != null)
            {
                DispatchMovement();
                DispatchSprint();
                DispatchBlock();
                DispatchIntent<DodgeIntent>("action_dodge");
                DispatchIntent<LightAttackIntent>("action_attack_light");
                DispatchIntent<HeavyAttackIntent>("action_attack_heavy");
                DispatchCastPower("action_power_00", _player!.Power1);
                DispatchCastPower("action_power_01", _player!.Power2);
                DispatchCastPower("action_power_02", _player!.Power3);
                DispatchCastPower("action_power_03", _player!.Power4);
            }
        }

        /// <summary> Reads the analogue movement vector and dispatches a <see cref="MoveIntent"/> every frame. </summary>
        private void DispatchMovement()
        {
            //A zero vector is dispatched intentionally so that walking and sprinting states observe the stick-release and transition back to idle.
            Vector2 direction = Input.GetVector(
                "action_move_left",
                "action_move_right",
                "action_move_up",
                "action_move_down",
                _analogueDeadzone);

            _player!.Machine.HandleIntent(new MoveIntent(direction));
        }

        /// <summary> Dispatches a <see cref="SprintIntent"/>s. </summary>
        private void DispatchSprint()
        {
            String sprintAction = "action_sprint";
            Boolean pressed = Input.IsActionJustPressed(sprintAction);
            Boolean released = Input.IsActionJustReleased(sprintAction);

            if (pressed)
            {
                _player!.Machine.HandleIntent(new SprintIntent(true));
            }
            else if (released)
            {
                _player!.Machine.HandleIntent(new SprintIntent(false));
            }
        }

        /// <summary> Dispatches a <see cref="BlockIntent"/>s. </summary>
        private void DispatchBlock()
        {
            String blockAction = "action_block";
            Boolean pressed = Input.IsActionJustPressed(blockAction);
            Boolean released = Input.IsActionJustReleased(blockAction);

            if (pressed)
            {
                _player!.Machine.HandleIntent(new BlockIntent(true));
            }
            else if (released)
            {
                _player!.Machine.HandleIntent(new BlockIntent(false));
            }
        }


        /// <summary>
        /// Dispatches a <see cref="CastPowerIntent"/> for <paramref name="actionName"/> on the press edge,
        /// but only when <paramref name="power"/> is non-null. A null power slot silently drops the input,
        /// consistent with the intent of an unbound hotbar slot.
        /// </summary>
        /// <param name="actionName">The InputMap action to test (one of the <c>cast_power_N</c> constants).</param>
        /// <param name="power">The power bound to this slot; null means the slot is empty.</param>
        private void DispatchCastPower(String actionName, PowerDefinition? power)
        {
            if (Input.IsActionJustPressed(actionName) && power != null)
            {
                _player!.Machine.HandleIntent(new CastPowerIntent(power));
            }
        }


        /// <summary> A generic intent dispatch. Binds an action to the intent directly. </summary>
        /// <typeparam name="T"> The type of intent to create upon the action. </typeparam>
        /// <param name="actionName"> The action's name. </param>
        private void DispatchIntent<T>(String actionName) where T : IInputIntent, new ()
        {
            if (Input.IsActionJustPressed(actionName))
            {
                _player!.Machine.HandleIntent(new T());
            }
        }
    }
}
