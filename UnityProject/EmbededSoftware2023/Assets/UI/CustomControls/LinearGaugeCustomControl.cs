using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Assets.UI.CustomControls
{
    /* ref from
    * https://www.youtube.com/watch?v=yivtEeoHBC8&ab_channel=OkayRoman
    * https://gist.github.com/Okay-Roman
    * https://www.youtube.com/watch?v=c-K5KmQW-Zk&ab_channel=MadCatTutorials
    */
    public class LinearGaugeCustomControl : VisualElement
    {
        protected float m_value = float.NaN;
        public void SetValueWithoutNotify(float newValue)
        {
            m_value = newValue;
            RadialFill.MarkDirtyRepaint();
        }

        public float Value
        {
            get
            {
                m_value = Mathf.Clamp(m_value, 0, 1);
                return m_value;
            }
            set
            {
                if (EqualityComparer<float>.Default.Equals(this.m_value, value))
                    return;
                if (this.panel != null)
                {
                    using (ChangeEvent<float> pooled = ChangeEvent<float>.GetPooled(this.m_value, value))
                    {
                        pooled.target = (IEventHandler)this;
                        this.SetValueWithoutNotify(value);
                        this.SendEvent((EventBase)pooled);
                    }
                }
                else
                {
                    this.SetValueWithoutNotify(value);
                }
            }
        }
        public float Width { get; set; }
        public float Height { get; set; }
        public Color StartColor { get; set; }
        public Color EndColor { get; set; }
        public Color HeadTipColor { get; set; }
        public bool IsColorPositionFixed { get; set; }//no use(always act like true)
        public bool IsBlink { get; set; }
        public Color BlinkColor { get; set; }
        public float BlinkOnDurationSec { get; set; }
        public float BlinkOffDurationSec { get; set; }
        public string OverlayImagePath { get; set; }



        public enum GaugeShape
        {
            Circle,
            Rectangle,
            Polygon
        }
        public GaugeShape GaugeShape_ { get; set; }

        public float SegmentUnit { get; set; }
        public int FillCount { get; set; }
        public int SpaceCount { get; set; }
        public float WidthHeightRatio { get; set; }
        public int PolygonN { get; set; }



        private float m_overlayImageScale;
        public float OverlayImageScale
        {
            get
            {
                m_overlayImageScale = Mathf.Clamp(m_overlayImageScale, 0, 1);
                return m_overlayImageScale;
            }
            set => m_overlayImageScale = value;
        }


        public VisualElement RadialFill;
        public VisualElement OverlayImage;

        [UnityEngine.Scripting.Preserve]

        public new class UxmlFactory : UxmlFactory<LinearGaugeCustomControl, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_value = new UxmlFloatAttributeDescription() { name = "value", defaultValue = 1f };
            UxmlFloatAttributeDescription m_width = new UxmlFloatAttributeDescription() { name = "width", defaultValue = 20f };
            UxmlFloatAttributeDescription m_height = new UxmlFloatAttributeDescription() { name = "height", defaultValue = 20f };
            UxmlColorAttributeDescription m_startColor = new UxmlColorAttributeDescription() { name = "start-color", defaultValue = Color.white };
            UxmlColorAttributeDescription m_endColor = new UxmlColorAttributeDescription() { name = "end-color", defaultValue = Color.white };
            UxmlFloatAttributeDescription m_segmentUnit = new UxmlFloatAttributeDescription() { name = "segment-unit", defaultValue = 10f };

            UxmlBoolAttributeDescription m_isBlink = new UxmlBoolAttributeDescription() { name = "is-blink", defaultValue = false };
            UxmlColorAttributeDescription m_blinkColor = new UxmlColorAttributeDescription() { name = "blink-color", defaultValue = new Color(0, 0, 0, 0) };
            UxmlFloatAttributeDescription m_blinkOnDurationSec = new UxmlFloatAttributeDescription() { name = "blink-on-duration-sec", defaultValue = 0.1f };
            UxmlFloatAttributeDescription m_blinkOffDurationSec = new UxmlFloatAttributeDescription() { name = "blink-off-duration-sec", defaultValue = 0.1f };

            UxmlColorAttributeDescription m_headTipColor = new UxmlColorAttributeDescription() { name = "head-tip-color", defaultValue = Color.white };

            UxmlIntAttributeDescription m_fillCount = new UxmlIntAttributeDescription() { name = "fill-count", defaultValue = 3 };
            UxmlIntAttributeDescription m_spaceCount = new UxmlIntAttributeDescription() { name = "space-count", defaultValue = 1 };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }

            }
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as LinearGaugeCustomControl;

                // Assigning uxml attributes to c# properties
                ate.Value = m_value.GetValueFromBag(bag, cc);
                ate.Width = m_width.GetValueFromBag(bag, cc);
                ate.Height = m_height.GetValueFromBag(bag, cc);
                ate.StartColor = m_startColor.GetValueFromBag(bag, cc);
                ate.EndColor = m_endColor.GetValueFromBag(bag, cc);
                ate.SegmentUnit = m_segmentUnit.GetValueFromBag(bag, cc);

                ate.IsBlink = m_isBlink.GetValueFromBag(bag, cc);
                ate.BlinkColor = m_blinkColor.GetValueFromBag(bag, cc);
                ate.BlinkOnDurationSec = m_blinkOnDurationSec.GetValueFromBag(bag, cc);
                ate.BlinkOffDurationSec = m_blinkOffDurationSec.GetValueFromBag(bag, cc);


                ate.HeadTipColor = m_headTipColor.GetValueFromBag(bag, cc);
                ate.FillCount = m_fillCount.GetValueFromBag(bag, cc);
                ate.SpaceCount = m_spaceCount.GetValueFromBag(bag, cc);

                // Creating the hierarchy for the radial fill element
                //ate.name = "circular-gauge";
                ate.Clear();
                VisualElement radialBoundary = new VisualElement() { name = "radial-boundary" };
                radialBoundary.Add(ate.RadialFill);
                ate.RadialFill.Add(ate.OverlayImage);


                ate.RadialFill.style.flexGrow = 1;
                ate.OverlayImage.style.flexGrow = 1;
                ate.OverlayImage.style.scale = new Scale(new Vector2(ate.OverlayImageScale, ate.OverlayImageScale));

                ate.style.height = ate.Height;
                ate.style.width = ate.Width;
                radialBoundary.style.height = ate.Height;
                radialBoundary.style.width = ate.Width;
                radialBoundary.style.overflow = Overflow.Hidden;
                ate.OverlayImage.style.backgroundImage = null;

#if UNITY_EDITOR
                Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(ate.OverlayImagePath);
                if (tex != null)
                {
                    ate.OverlayImage.style.backgroundImage = tex;
                }
#endif

                ate.RadialFill.transform.rotation = Quaternion.Euler(0, 0, 0);
                ate.OverlayImage.transform.rotation = Quaternion.Euler(0, 0, 0);

                ate.Add(radialBoundary);
            }
        }

        public LinearGaugeCustomControl() : base()
        {
            RadialFill = new VisualElement() { name = "radial-fill" };
            OverlayImage = new VisualElement() { name = "overlay-image" };
            RadialFill.generateVisualContent += OnGenerateVisualContent;
        }

        public void AngleUpdate(ChangeEvent<float> evt)
        {
            RadialFill?.MarkDirtyRepaint();
        }
        public void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            /*
             * 
             * 33도
             * squadNum = ceil(33/5) == 7
             * vertCount = (7+1) * 2 == 16
             * indiceCount = ceil(7/2) * 2 * 3 == 24
             * 
             * 
             * for문 7*2 + 2 == 16개 vert
             * 
             * for문
             */
            if (m_value <= 0) return;
            if (SpaceCount < 0 || FillCount < 0 || FillCount + SpaceCount <= 0) return;
            float currentHeight = Value * Height;
            ushort squadNum = (ushort)(Mathf.Ceil(currentHeight / SegmentUnit));
            int vertCount = (squadNum + 1) * 2;
            int indiceCount = 0;
            for (int idx = 0; idx < squadNum; idx++)
            {
                if ((idx) % (FillCount + SpaceCount) < FillCount) indiceCount += 6;
            }
            Debug.Log($"squadNum:{squadNum}, vertCount:{vertCount}, indiceCount:{indiceCount}");
            //int indiceCount = (int)(Mathf.Ceil(squadNum / 2f) * 2 * 3);//squadNum 2 -> 1개만 씀 3 -> 2개 4 -> 2개 5->3개   squadNum/2
            

            // Create our MeshWriteData object, allocate the least amount of vertices and triangle indices required
            MeshWriteData mwd = mgc.Allocate(vertCount, indiceCount);
            Vector3 origin = new Vector3(0, Height, 0);
            //Vector3 origin = new Vector3((float)Width / 2, (float)Height / 2, 0);

            for (int idx = 0; idx < squadNum; idx++)
            {
                Vector3 innerCoord = new Vector3(0, -idx * SegmentUnit, Vertex.nearZ);
                Vector3 outerCoord = new Vector3(Width, -idx * SegmentUnit, Vertex.nearZ);
                var color = Color.Lerp(StartColor, EndColor, (float)SegmentUnit * idx / Height);
                if (IsBlink && TimeManager.RectangularPulse(BlinkOnDurationSec, BlinkOffDurationSec))
                {
                    color = BlinkColor;
                }
                mwd.SetNextVertex(new Vertex()
                {
                    position = origin + innerCoord,
                    tint = color
                }); ;
                mwd.SetNextVertex(new Vertex()
                {
                    position = origin + outerCoord,
                    tint = color


                });
            }

            var color2 = Color.Lerp(StartColor, EndColor, (float)currentHeight / Height);
            if (IsBlink && TimeManager.RectangularPulse(BlinkOnDurationSec, BlinkOffDurationSec))
            {
                color2 = BlinkColor;
            }
            mwd.SetNextVertex(new Vertex()
            {
                position = origin + new Vector3(0, -currentHeight, Vertex.nearZ),
                tint = color2
            });
            mwd.SetNextVertex(new Vertex()
            {
                position = origin + new Vector3(Width, -currentHeight, Vertex.nearZ),
                tint = color2
            });


            for (int idx = 0; idx < squadNum; idx++)
            {
                if ((idx) % (FillCount + SpaceCount) < FillCount)
                {
                    //mwd.SetNextIndex((ushort)(idx * 2 + 0));
                    //mwd.SetNextIndex((ushort)(idx * 2 + 1));
                    //mwd.SetNextIndex((ushort)(idx * 2 + 3));

                    //mwd.SetNextIndex((ushort)(idx * 2 + 0));
                    //mwd.SetNextIndex((ushort)(idx * 2 + 3));
                    //mwd.SetNextIndex((ushort)(idx * 2 + 2));

                    mwd.SetNextIndex((ushort)(idx * 2 + 3));
                    mwd.SetNextIndex((ushort)(idx * 2 + 1));
                    mwd.SetNextIndex((ushort)(idx * 2 + 0));

                    mwd.SetNextIndex((ushort)(idx * 2 + 2));
                    mwd.SetNextIndex((ushort)(idx * 2 + 3));
                    mwd.SetNextIndex((ushort)(idx * 2 + 0));
                }
            }
        }

    }

}
