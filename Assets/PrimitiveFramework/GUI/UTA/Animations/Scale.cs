namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class Scale : UTACurveClip<ScaleBehaviour>
    {
#if UNITY_EDITOR
        public override string DisplayName { get { return string.Concat("Scale"); } }
#endif
    }
}