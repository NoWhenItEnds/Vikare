using Godot;
using Vikare.Entities;
using Vikare.Entities.Combat;
using Vikare.Entities.Controllers;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary>
    /// Translates raw Godot input events into <c>IInputIntent</c> instances and dispatches them
    /// to the registered player's state machine each frame.
    /// <para>
    /// This node is placed in the scene tree at <c>Managers/Input</c> inside <c>Game.tscn</c>;
    /// it is not an autoload. Placement in the scene ensures deterministic teardown order.
    /// </para>
    /// <para>
    /// Only one player may be registered at a time. A second <see cref="SetPlayer"/> call overwrites
    /// the first and logs a warning. AI-controlled actors drive their own state machines directly
    /// and are never registered here.
    /// </para>
    /// </summary>
    public partial class InputManager : SingletonNode<InputManager>
    {
        /// <summary>
        /// Name of the input action that moves the player upward (keyboard: W; gamepad: left stick up).
        /// </summary>
        private const string ActionMoveUp = "move_up";

        /// <summary>
        /// Name of the input action that moves the player downward (keyboard: S; gamepad: left stick down).
        /// </summary>
        private const string ActionMoveDown = "move_down";

        /// <summary>
        /// Name of the input action that moves the player leftward (keyboard: A; gamepad: left stick left).
        /// </summary>
        private const string ActionMoveLeft = "move_left";

        /// <summary>
        /// Name of the input action that moves the player rightward (keyboard: D; gamepad: left stick right).
        /// </summary>
        private const string ActionMoveRight = "move_right";

        /// <summary>
        /// Name of the input action that begins or ends sprinting (keyboard: Shift; gamepad: L3).
        /// </summary>
        private const string ActionSprint = "sprint";

        /// <summary>
        /// Name of the input action that triggers a dodge (keyboard: Space; gamepad: East face button).
        /// </summary>
        private const string ActionDodge = "dodge";

        /// <summary>
        /// Name of the input action that triggers a light attack (keyboard: left mouse button; gamepad: West face button).
        /// </summary>
        private const string ActionAttackLight = "attack_light";

        /// <summary>
        /// Name of the input action that triggers a heavy attack (keyboard: right mouse button; gamepad: North face button).
        /// </summary>
        private const string ActionAttackHeavy = "attack_heavy";

        /// <summary>
        /// Name of the input action that begins or ends blocking (keyboard: E; gamepad: right shoulder button).
        /// </summary>
        private const string ActionBlock = "block";

        /// <summary>
        /// Name of the input action that casts the power bound to slot 1 (keyboard: 1; gamepad: D-pad up).
        /// </summary>
        private const string ActionCastPower1 = "cast_power_1";

        /// <summary>
        /// Name of the input action that casts the power bound to slot 2 (keyboard: 2; gamepad: D-pad right).
        /// </summary>
        private const string ActionCastPower2 = "cast_power_2";

        /// <summary>
        /// Name of the input action that casts the power bound to slot 3 (keyboard: 3; gamepad: D-pad down).
        /// </summary>
        private const string ActionCastPower3 = "cast_power_3";

        /// <summary>
        /// Name of the input action that casts the power bound to slot 4 (keyboard: 4; gamepad: D-pad left).
        /// </summary>
        private const string ActionCastPower4 = "cast_power_4";

        /// <summary>
        /// Analogue deadzone applied to <see cref="Input.GetVector"/> for movement; filters out stick drift below this magnitude.
        /// </summary>
        private const float MovementDeadzone = 0.2f;

        /// <summary>
        /// The player actor currently receiving input; null when no player has registered.
        /// Assigned by <see cref="SetPlayer"/> and cleared by <see cref="ClearPlayer"/>.
        /// </summary>
        private Actor? _player;

        /// <summary>
        /// Registers <paramref name="player"/> as the recipient of all translated input intents.
        /// If a player is already registered, a warning is pushed to the Godot output and the new actor
        /// overwrites the old one. A future multiplayer refactor will need to extend this to a collection.
        /// </summary>
        /// <param name="player">The actor that should receive player input.</param>
        public void SetPlayer(Actor player)
        {
            if (_player != null)
            {
                GD.PushWarning(
                    $"[InputManager] SetPlayer called whilst '{_player.Name}' is already registered. " +
                    $"Overwriting with '{player.Name}'. If this is unintentional, ensure ClearPlayer is called on _ExitTree.");
            }

            _player = player;
        }

        /// <summary>
        /// Unregisters the current player, stopping all input dispatch. Safe to call when no player is registered.
        /// </summary>
        public void ClearPlayer()
        {
            _player = null;
        }

        /// <summary>
        /// Polls Godot's <see cref="Input"/> singleton each frame and dispatches zero or more
        /// <c>IInputIntent</c> instances to the registered player's state machine.
        /// <para>
        /// Dispatch order per frame:
        /// <list type="number">
        ///   <item><see cref="MoveIntent"/> — always dispatched, even when the vector is zero, so walking/sprinting
        ///   states observe the release and transition to idle.</item>
        ///   <item><see cref="SprintIntent"/> — dispatched on edge (press or release) only.</item>
        ///   <item><see cref="BlockIntent"/> — dispatched on edge (press or release) only.</item>
        ///   <item><see cref="DodgeIntent"/> — dispatched on press only.</item>
        ///   <item><see cref="LightAttackIntent"/> — dispatched on press only.</item>
        ///   <item><see cref="HeavyAttackIntent"/> — dispatched on press only.</item>
        ///   <item><see cref="CastPowerIntent"/> ×4 — dispatched on press only; silently dropped if the corresponding power slot is null.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="delta">Elapsed time since the last process tick, in seconds. Not used directly; Godot requires the signature.</param>
        public override void _Process(double delta)
        {
            bool playerActive = _player != null;

            if (playerActive)
            {
                DispatchMovement();
                DispatchSprint();
                DispatchBlock();
                DispatchDodge();
                DispatchLightAttack();
                DispatchHeavyAttack();
                DispatchCastPower(ActionCastPower1, _player!.Power1);
                DispatchCastPower(ActionCastPower2, _player!.Power2);
                DispatchCastPower(ActionCastPower3, _player!.Power3);
                DispatchCastPower(ActionCastPower4, _player!.Power4);
            }
        }

        /// <summary>
        /// Reads the analogue movement vector and dispatches a <see cref="MoveIntent"/> every frame.
        /// A zero vector is dispatched intentionally so that walking and sprinting states observe the
        /// stick-release and transition back to idle.
        /// </summary>
        private void DispatchMovement()
        {
            Vector2 direction = Input.GetVector(
                ActionMoveLeft,
                ActionMoveRight,
                ActionMoveUp,
                ActionMoveDown,
                MovementDeadzone);

            _player!.Machine.HandleIntent(new MoveIntent(direction));
        }

        /// <summary>
        /// Dispatches a <see cref="SprintIntent"/> on the press edge (<c>IsSprinting = true</c>)
        /// and on the release edge (<c>IsSprinting = false</c>). No intent is sent on held frames.
        /// </summary>
        private void DispatchSprint()
        {
            bool pressed = Input.IsActionJustPressed(ActionSprint);
            bool released = Input.IsActionJustReleased(ActionSprint);

            if (pressed)
            {
                _player!.Machine.HandleIntent(new SprintIntent(isSprinting: true));
            }
            else if (released)
            {
                _player!.Machine.HandleIntent(new SprintIntent(isSprinting: false));
            }
        }

        /// <summary>
        /// Dispatches a <see cref="BlockIntent"/> on the press edge (<c>IsBlocking = true</c>)
        /// and on the release edge (<c>IsBlocking = false</c>). No intent is sent on held frames.
        /// </summary>
        private void DispatchBlock()
        {
            bool pressed = Input.IsActionJustPressed(ActionBlock);
            bool released = Input.IsActionJustReleased(ActionBlock);

            if (pressed)
            {
                _player!.Machine.HandleIntent(new BlockIntent(isBlocking: true));
            }
            else if (released)
            {
                _player!.Machine.HandleIntent(new BlockIntent(isBlocking: false));
            }
        }

        /// <summary>
        /// Dispatches a <see cref="DodgeIntent"/> on the press edge only. No release semantics — dodge is a one-shot trigger.
        /// </summary>
        private void DispatchDodge()
        {
            if (Input.IsActionJustPressed(ActionDodge))
            {
                _player!.Machine.HandleIntent(new DodgeIntent());
            }
        }

        /// <summary>
        /// Dispatches a <see cref="LightAttackIntent"/> on the press edge only.
        /// </summary>
        private void DispatchLightAttack()
        {
            if (Input.IsActionJustPressed(ActionAttackLight))
            {
                _player!.Machine.HandleIntent(new LightAttackIntent());
            }
        }

        /// <summary>
        /// Dispatches a <see cref="HeavyAttackIntent"/> on the press edge only.
        /// </summary>
        private void DispatchHeavyAttack()
        {
            if (Input.IsActionJustPressed(ActionAttackHeavy))
            {
                _player!.Machine.HandleIntent(new HeavyAttackIntent());
            }
        }

        /// <summary>
        /// Dispatches a <see cref="CastPowerIntent"/> for <paramref name="actionName"/> on the press edge,
        /// but only when <paramref name="power"/> is non-null. A null power slot silently drops the input,
        /// consistent with the intent of an unbound hotbar slot.
        /// </summary>
        /// <param name="actionName">The InputMap action to test (one of the <c>cast_power_N</c> constants).</param>
        /// <param name="power">The power bound to this slot; null means the slot is empty.</param>
        private void DispatchCastPower(string actionName, PowerDefinition? power)
        {
            if (Input.IsActionJustPressed(actionName) && power != null)
            {
                _player!.Machine.HandleIntent(new CastPowerIntent(power));
            }
        }
    }
}
