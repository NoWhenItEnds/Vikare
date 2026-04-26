using System;
using Godot;
using Vikare.Entities;
using Vikare.Entities.Intents;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary>
    /// Singleton service that translates raw Godot input into <see cref="ActionIntent"/> instances
    /// and dispatches them to the registered player actor's state machine each frame.
    /// </summary>
    /// <remarks>
    /// Priority order (highest overrides lower within a frame):
    ///   Block held → Dodge just-pressed → Heavy attack → Light attack → Power casts 1–4 → Sprint held → Walk (default).
    /// </remarks>
    public partial class InputManager : SingletonNode<InputManager>
    {
        /// <summary>Leftward movement action; defined in Project &gt; Input Map.</summary>
        private const string InputLeft = "action_left";

        /// <summary>Rightward movement action; defined in Project &gt; Input Map.</summary>
        private const string InputRight = "action_right";

        /// <summary>Upward movement action; defined in Project &gt; Input Map.</summary>
        private const string InputUp = "action_up";

        /// <summary>Downward movement action; defined in Project &gt; Input Map.</summary>
        private const string InputDown = "action_down";

        /// <summary>Sprint modifier action; defined in Project &gt; Input Map.</summary>
        private const string InputSprint = "action_sprint";

        /// <summary>Light melee attack action; defined in Project &gt; Input Map.</summary>
        private const string InputAttackLight = "action_attack_light";

        /// <summary>Heavy melee attack action; defined in Project &gt; Input Map.</summary>
        private const string InputAttackHeavy = "action_attack_heavy";

        /// <summary>Dodge action; defined in Project &gt; Input Map.</summary>
        private const string InputDodge = "action_dodge";

        /// <summary>Block action; defined in Project &gt; Input Map.</summary>
        private const string InputBlock = "action_block";

        /// <summary>Power slot 1 action; defined in Project &gt; Input Map.</summary>
        private const string InputPower1 = "action_power_00";

        /// <summary>Power slot 2 action; defined in Project &gt; Input Map.</summary>
        private const string InputPower2 = "action_power_01";

        /// <summary>Power slot 3 action; defined in Project &gt; Input Map.</summary>
        private const string InputPower3 = "action_power_02";

        /// <summary>Power slot 4 action; defined in Project &gt; Input Map.</summary>
        private const string InputPower4 = "action_power_03";

        /// <summary>
        /// Analogue stick deadzone magnitude; input below this is treated as zero to prevent stick drift.
        /// Valid range: 0.0–1.0. Typical value: 0.1–0.25.
        /// </summary>
        [ExportGroup("Settings")]
        [Export] private float _analogueDeadzone = 0.2f;

        /// <summary>The actor currently receiving input; null when none has registered.</summary>
        private Actor? _player;

        /// <summary>
        /// Registers <paramref name="player"/> as the sole input recipient. Logs a warning and
        /// replaces the prior registration if one exists.
        /// Called automatically by <see cref="Vikare.Entities.Actor._Ready"/> when
        /// <see cref="Vikare.Entities.Actor.IsPlayerControlled"/> is true.
        /// </summary>
        /// <param name="player">The actor that should receive player input.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
        public void RegisterPlayer(Actor player)
        {
            ArgumentNullException.ThrowIfNull(player);

            bool alreadyRegistered = _player != null;
            if (alreadyRegistered)
            {
                // Guard against accessing .Name on a freed Godot object.
                string previousName = IsInstanceValid(_player)
                    ? _player!.Name
                    : "<freed actor>";
                GD.PushWarning(
                    $"[InputManager] RegisterPlayer called whilst '{previousName}' is already " +
                    $"registered. Overwriting with '{player.Name}'. Check that only one actor has " +
                    "IsPlayerControlled = true in the scene.");
                DeregisterPlayer();
            }

            _player = player;
        }

        /// <summary>
        /// Clears the current player registration. No-op when no player is registered.
        /// Called automatically by <see cref="Vikare.Entities.Actor._ExitTree"/>.
        /// </summary>
        public void DeregisterPlayer()
        {
            _player = null;
        }

        /// <summary>
        /// Reads input, builds the highest-priority intent for this frame, and dispatches it to
        /// the registered player's state machine. No-op when no player is registered.
        /// </summary>
        /// <param name="delta">Unused; declared to satisfy the Godot override signature.</param>
        public override void _Process(double delta)
        {
            if (_player is not null && IsInstanceValid(_player))
            {
                Vector2 direction = Input.GetVector(
                    InputLeft, InputRight, InputUp, InputDown, _analogueDeadzone);

                ActionIntent intent = BuildIntent(direction);
                _player.Machine.HandleIntent(intent);
            }
        }

        /// <summary>
        /// Constructs the highest-priority <see cref="ActionIntent"/> for the current frame.
        /// Reads from lowest to highest priority so the last assignment wins.
        /// </summary>
        /// <param name="direction">Current stick/WASD direction, pre-filtered by <see cref="_analogueDeadzone"/>.</param>
        /// <returns>The single highest-priority intent for this frame; never null.</returns>
        /// <remarks>
        /// Block-release gating: <see cref="BlockIntent"/> with <c>IsBlocking = false</c> is only
        /// written when the current result is movement-tier (<see cref="WalkIntent"/> or
        /// <see cref="SprintIntent"/>), preventing a block-release from cancelling a same-frame
        /// higher-priority action such as a dodge or ability.
        /// </remarks>
        private ActionIntent BuildIntent(Vector2 direction)
        {
            ActionIntent result = new WalkIntent(direction);

            bool sprintHeld = Input.IsActionPressed(InputSprint);
            if (sprintHeld)
            {
                result = new SprintIntent(direction);
            }

            bool lightAttack = Input.IsActionJustPressed(InputAttackLight);
            if (lightAttack)
            {
                result = new AbilityIntent(direction, AbilityKey.LightAttack);
            }

            bool heavyAttack = Input.IsActionJustPressed(InputAttackHeavy);
            if (heavyAttack)
            {
                result = new AbilityIntent(direction, AbilityKey.HeavyAttack);
            }

            bool power1 = Input.IsActionJustPressed(InputPower1);
            if (power1)
            {
                result = new AbilityIntent(direction, AbilityKey.CastPower1);
            }

            bool power2 = Input.IsActionJustPressed(InputPower2);
            if (power2)
            {
                result = new AbilityIntent(direction, AbilityKey.CastPower2);
            }

            bool power3 = Input.IsActionJustPressed(InputPower3);
            if (power3)
            {
                result = new AbilityIntent(direction, AbilityKey.CastPower3);
            }

            bool power4 = Input.IsActionJustPressed(InputPower4);
            if (power4)
            {
                result = new AbilityIntent(direction, AbilityKey.CastPower4);
            }

            bool dodge = Input.IsActionJustPressed(InputDodge);
            if (dodge)
            {
                result = new DodgeIntent(direction);
            }

            bool blockHeld = Input.IsActionPressed(InputBlock);
            if (blockHeld)
            {
                result = new BlockIntent(direction, isBlocking: true);
            }

            bool blockReleased = Input.IsActionJustReleased(InputBlock);
            bool resultIsMovementTier = result is WalkIntent or SprintIntent;
            if (blockReleased && resultIsMovementTier)
            {
                result = new BlockIntent(direction, isBlocking: false);
            }

            return result;
        }
    }
}
