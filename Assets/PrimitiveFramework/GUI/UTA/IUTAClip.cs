namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    internal interface IUTAClip
    {
#if UNITY_EDITOR
        string DisplayName { get; }
#endif
    }
}