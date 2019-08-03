// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	public sealed class TemplateDepthModule : TemplateModuleParent
	{
		public TemplateDepthModule() : base("Depth"){}

		[SerializeField]
		private bool m_validZTest = false;

		[SerializeField]
		private int m_zTestMode = 0;
		
		[SerializeField]
		private bool m_validZWrite = false;

		[SerializeField]
		private int m_zWriteMode = 0;

		[SerializeField]
		private float m_offsetFactor = 0;

		[SerializeField]
		private float m_offsetUnits = 0;

		[SerializeField]
		private bool m_offsetEnabled = false;

		[SerializeField]
		private bool m_validOffset = false;

		public override void ShowUnreadableDataMessage( ParentNode owner )
		{
			bool foldoutValue = owner.ContainerGraph.ParentWindow.ExpandedDepth;
			NodeUtils.DrawPropertyGroup( ref foldoutValue, ZBufferOpHelper.DepthParametersStr,  base.ShowUnreadableDataMessage );
			owner.ContainerGraph.ParentWindow.ExpandedDepth = foldoutValue;
		}

		public override void Draw( ParentNode owner )
		{
			Color cachedColor = GUI.color;
			GUI.color = new Color( cachedColor.r, cachedColor.g, cachedColor.b, 0.5f );
			EditorGUILayout.BeginHorizontal( UIUtils.MenuItemToolbarStyle );
			GUI.color = cachedColor;
			EditorGUI.BeginChangeCheck();
			owner.ContainerGraph.ParentWindow.ExpandedDepth = owner.GUILayoutToggle( owner.ContainerGraph.ParentWindow.ExpandedDepth, ZBufferOpHelper.DepthParametersStr, UIUtils.MenuItemToggleStyle );
			if( EditorGUI.EndChangeCheck() )
			{
				EditorPrefs.SetBool( "ExpandedDepth", owner.ContainerGraph.ParentWindow.ExpandedDepth );
			}
			EditorGUILayout.EndHorizontal();

			if( owner.ContainerGraph.ParentWindow.ExpandedDepth )
			{
				cachedColor = GUI.color;
				GUI.color = new Color( cachedColor.r, cachedColor.g, cachedColor.b, ( EditorGUIUtility.isProSkin ? 0.5f : 0.25f ) );
				EditorGUILayout.BeginVertical( UIUtils.MenuItemBackgroundStyle );
				GUI.color = cachedColor;

				EditorGUI.indentLevel++;
				
				EditorGUILayout.Separator();

				if( m_validZWrite)
					m_zWriteMode = owner.EditorGUILayoutPopup( ZBufferOpHelper.ZWriteModeStr, m_zWriteMode, ZBufferOpHelper.ZWriteModeValues );

				if( m_validZTest) 
					m_zTestMode = owner.EditorGUILayoutPopup( ZBufferOpHelper.ZTestModeStr, m_zTestMode, ZBufferOpHelper.ZTestModeLabels );


				if( m_validOffset )
				{
					m_offsetEnabled = owner.EditorGUILayoutToggle( ZBufferOpHelper.OffsetStr, m_offsetEnabled );
					if( m_offsetEnabled )
					{
						EditorGUI.indentLevel++;
						m_offsetFactor = owner.EditorGUILayoutFloatField( ZBufferOpHelper.OffsetFactorStr, m_offsetFactor );
						m_offsetUnits = owner.EditorGUILayoutFloatField( ZBufferOpHelper.OffsetUnitsStr, m_offsetUnits );
						EditorGUI.indentLevel--;
					}
				}
				EditorGUILayout.Separator();
				EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
			}

			EditorGUI.EndDisabledGroup();
		}
		
		public void ConfigureFromTemplateData( TemplateDepthData depthData )
		{
			m_validZTest = depthData.ValidZTest;
			if( depthData.ValidZTest )
			{
				m_zTestMode = ZBufferOpHelper.ZTestModeDict[ depthData.ZTestModeValue ];
			}

			m_validZWrite = depthData.ValidZWrite;
			if( depthData.ValidZWrite )
			{
				m_zWriteMode = ZBufferOpHelper.ZWriteModeDict[ depthData.ZWriteModeValue ]; 
			}

			m_offsetEnabled = depthData.ValidOffset;
			m_validOffset = depthData.ValidOffset;
			if( depthData.ValidOffset )
			{
				m_offsetFactor = depthData.OffsetFactor;
				m_offsetUnits = depthData.OffsetUnits;
			}
		}

		public void ReadZWriteFromString( ref uint index, ref string[] nodeParams )
		{	
			m_zWriteMode = Convert.ToInt32( nodeParams[ index++ ] );
		}

		public void ReadZTestFromString( ref uint index, ref string[] nodeParams )
		{
			m_zTestMode = Convert.ToInt32( nodeParams[ index++ ] );
		}

		public void ReadOffsetFromString( ref uint index, ref string[] nodeParams )
		{
			m_offsetEnabled = Convert.ToBoolean( nodeParams[ index++ ] );
			m_offsetFactor = Convert.ToSingle( nodeParams[ index++ ] );
			m_offsetUnits = Convert.ToSingle( nodeParams[ index++ ] );
		}

		public void WriteZWriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_zWriteMode );
		}

		public void WriteZTestToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_zTestMode );
		}

		public void WriteOffsetToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_offsetEnabled );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_offsetFactor );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_offsetUnits );
		}
		public bool IsActive { get { return m_zTestMode != 0 || m_zWriteMode != 0 || m_offsetEnabled; } }
		public string CurrentZWriteMode { get { return m_zWriteMode == 0?string.Empty:( "ZWrite " + ZBufferOpHelper.ZWriteModeValues[ m_zWriteMode ]); } }
		public string CurrentZTestMode { get { return m_zTestMode == 0 ? string.Empty : "ZTest " + ZBufferOpHelper.ZTestModeValues[ m_zTestMode ]; } }
		public string CurrentOffset { get { return "Offset " + m_offsetFactor + " , " + m_offsetUnits; } }
	}
}
