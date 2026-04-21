namespace Vikare.Entities.Capabilities
{
    /// <summary>
    /// Capability contract for any entity whose visual representation is driven by named animation clips.
    /// </summary>
    public interface IAnimated
    {
        /// <summary>
        /// Requests playback of the named clip; implementations may ignore the call if that clip is already playing.
        /// </summary>
        /// <param name="animationName">Name of the clip as defined in the entity's animation library.</param>
        void PlayAnimation(string animationName);
    }
}
