using System;
using Godot;
using Vikare.Entities;
using Vikare.Entities.Abilities;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary>
    /// Manages input during game runtime. Translates raw Godot input events into
    /// <see cref="ActionIntent"/> instances and dispatches them to the registered player's state
    /// machine each frame. Ability slots are resolved via <see cref="Actor.GetAbility"/> using
    /// <see cref="AbilityCategory"/> — no editor-assigned exemplars are required.
    /// </summary>
    public partial class InputManager : SingletonNode<InputManager>
    {
        /// <summary>
        /// Analogue stick deadzone. Input vectors with a magnitude below this threshold are treated
        /// as zero to filter out hardware drift. Valid range: 0.0–1.0.
        /// </summary>
        [ExportGroup("Settings")]
        [Export] private Single _analogueDeadzone = 0.2f;

        /// <summary>
        /// The intent submitted the previous frame. Null indicates that no intent was produced
        /// last frame, which is a normal condition (e.g. when no player is registered).
        /// </summary>
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

                AbilityEffect? lightAbility = _player.GetAbility(AbilityCategory.LightAttack);
                if (Input.IsActionJustPressed("action_attack_light") && lightAbility is not null)
                {
                    intent = new AbilityIntent(direction, lightAbility);
                }
                else
                {
                    AbilityEffect? heavyAbility = _player.GetAbility(AbilityCategory.HeavyAttack);
                    if (Input.IsActionJustPressed("action_attack_heavy") && heavyAbility is not null)
                    {
                        intent = new AbilityIntent(direction, heavyAbility);
                    }
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

                _player.Machine.HandleIntent(intent);
                _previousInput = intent;

                //DispatchCastPower("action_power_00", _player!.Power1);
                //DispatchCastPower("action_power_01", _player!.Power2);
                //DispatchCastPower("action_power_02", _player!.Power3);
                //DispatchCastPower("action_power_03", _player!.Power4);
            }
        }
    }
}
