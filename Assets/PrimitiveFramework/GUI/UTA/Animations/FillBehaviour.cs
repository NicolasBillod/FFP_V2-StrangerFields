using UnityEngine.UI;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class FillBehaviour : UTACurvePlayableBehaviour
    {
        public Image.FillMethod m_FillMethod;
        public int m_FillOrigin;

        private Image m_TargetImage;
        private float m_FillMultiplier;

        internal override void PrepareAnimation()
        {
            m_TargetImage = m_AffectedTransform.GetComponent<Image>();
        }

        protected override void Start()
        {
            m_TargetImage.fillMethod = m_FillMethod;
            m_TargetImage.fillOrigin = m_FillOrigin;

            UTAFloatGetter floatGetter = m_AffectedTransform.GetComponent<UTAFloatGetter>();
            m_FillMultiplier = floatGetter != null ? floatGetter.GetFloat() : 1;
        }

        internal override void Progress(float progress)
        {
            m_TargetImage.fillAmount = progress * m_FillMultiplier;
        }
    }
}