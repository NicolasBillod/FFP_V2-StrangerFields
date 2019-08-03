using System;

namespace PrimitiveFactory.Framework.EditorTools
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Field)]
    public class LoadOnDemand : Attribute
    {
        private string m_FieldName;

        public LoadOnDemand(string fieldName)
        {
            m_FieldName = fieldName;
        }

        public string FieldName { get { return m_FieldName; } }
    }
}