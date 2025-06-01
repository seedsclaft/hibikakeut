//////////////////////////////////////////////////////
// MK Glow Lite 	    	    	                //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MK.Glow.Legacy
{
	#if UNITY_2018_3_OR_NEWER
        [ExecuteAlways]
    #else
        [ExecuteInEditMode]
    #endif
    [DisallowMultipleComponent]
    [ImageEffectAllowedInSceneView]
    [RequireComponent(typeof(UnityEngine.Camera))]
	public class MKGlowLite : MonoBehaviour, MK.Glow.ICameraData, MK.Glow.ISettings
	{
        #if UNITY_EDITOR
        public bool showEditorMainBehavior = true;
		public bool showEditorBloomBehavior;
		public bool showEditorLensSurfaceBehavior;
        #endif

        //Main
        public DebugView debugView = MK.Glow.DebugView.None;
        public Workflow workflow = MK.Glow.Workflow.Threshold;
        public LayerMask selectiveRenderLayerMask = -1;
        [Range(-1f, 1f)]
        public float anamorphicRatio = 0f;

        //Bloom
        [MK.Glow.MinMaxRange(0, 10)]
        public MinMaxRange bloomThreshold = new MinMaxRange(1.25f, 10f);
        [Range(1f, 10f)]
		public float bloomScattering = 7f;
		public float bloomIntensity = 1f;

        //LensSurface
        public bool allowLensSurface = false;
		public Texture2D lensSurfaceDirtTexture;
		public float lensSurfaceDirtIntensity = 2.5f;
		public Texture2D lensSurfaceDiffractionTexture;
		public float lensSurfaceDiffractionIntensity = 2.0f;

        private Effect _effect;

        private RenderTarget _source, _destination;

		private UnityEngine.Camera renderingCamera
		{
			get { return GetComponent<UnityEngine.Camera>(); }
		}

		public void OnEnable()
		{
            _effect = new Effect();
			_effect.Enable(RenderPipeline.Legacy);

            enabled = Compatibility.IsSupported;
		}

		public void OnDisable()
		{
			_effect.Disable();
		}

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if(workflow == Workflow.Selective && (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline || PipelineProperties.xrEnabled))
            {
                Graphics.Blit(source, destination);
                return;
            }

            _source.renderTexture = source;
            _destination.renderTexture = destination;
			_effect.Build(_source, _destination, this, null, this, renderingCamera);

            Graphics.Blit(source, destination, _effect.renderMaterialNoGeometry, _effect.currentRenderIndex);
            _effect.AfterCompositeCleanup();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Settings
        /////////////////////////////////////////////////////////////////////////////////////////////
        public bool GetAllowGeometryShaders()
        { 
            return false;
        }
        public bool GetAllowComputeShaders()
        { 
            return false;
        }
        public MK.Glow.DebugView GetDebugView()
        { 
			return debugView;
		}
        public MK.Glow.Workflow GetWorkflow()
        { 
			return workflow;
		}
        public LayerMask GetSelectiveRenderLayerMask()
        { 
			return selectiveRenderLayerMask;
		}
        public float GetAnamorphicRatio()
        { 
			return anamorphicRatio;
		}

        //Bloom
		public MK.Glow.MinMaxRange GetBloomThreshold()
		{ 
			return bloomThreshold;
		}
		public float GetBloomScattering()
		{ 
			return bloomScattering;
		}
		public float GetBloomIntensity()
		{ 
			return bloomIntensity;
		}

        //LensSurface
		public bool GetAllowLensSurface()
		{ 
			return allowLensSurface;
		}
		public Texture2D GetLensSurfaceDirtTexture()
		{ 
			return lensSurfaceDirtTexture;
		}
		public float GetLensSurfaceDirtIntensity()
		{ 
			return lensSurfaceDirtIntensity;
		}
		public Texture2D GetLensSurfaceDiffractionTexture()
		{ 
			return lensSurfaceDiffractionTexture;
		}
		public float GetLensSurfaceDiffractionIntensity()
		{ 
			return lensSurfaceDiffractionIntensity;
		}

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Camera Data
        /////////////////////////////////////////////////////////////////////////////////////////////
        public int GetCameraWidth()
        {
            return renderingCamera.allowDynamicResolution ? Mathf.RoundToInt(renderingCamera.pixelWidth * ScalableBufferManager.widthScaleFactor) : renderingCamera.pixelWidth;
        }
        public int GetCameraHeight()
        {
            return renderingCamera.allowDynamicResolution ? Mathf.RoundToInt(renderingCamera.pixelHeight * ScalableBufferManager.heightScaleFactor) : renderingCamera.pixelHeight;
        }
        public bool GetStereoEnabled()
        {
            return renderingCamera.stereoEnabled;
        }
        public float GetAspect()
        {
            return renderingCamera.aspect;
        }
        public Matrix4x4 GetWorldToCameraMatrix()
        {
            return renderingCamera.worldToCameraMatrix;
        }
        public bool GetOverwriteDescriptor()
        {
            return false;
        }
        public UnityEngine.Rendering.TextureDimension GetOverwriteDimension()
        {
            return UnityEngine.Rendering.TextureDimension.Tex2D;
        }
        public int GetOverwriteVolumeDepth()
        {
            return 1;
        }
        public bool GetTargetTexture()
        {
            return renderingCamera.targetTexture != null ? true : false;
        }
	}
}