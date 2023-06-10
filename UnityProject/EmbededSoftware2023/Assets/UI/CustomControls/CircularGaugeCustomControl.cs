using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

/* ref from
* https://www.youtube.com/watch?v=yivtEeoHBC8&ab_channel=OkayRoman
* https://gist.github.com/Okay-Roman
* https://www.youtube.com/watch?v=c-K5KmQW-Zk&ab_channel=MadCatTutorials
*/
public class CircularGaugeCustomControl : VisualElement
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
    public float AngleOffset { get; set; }
    public float Thickness { get; set; }
    public bool UniformThickness { get; set; }//no use(always act like true)
    public bool IsBlink { get; set; }
    public Color BlinkColor { get; set; }
    public float BlinkOnDurationSec { get; set; }
    public float BlinkOffDurationSec { get; set; }
    public string OverlayImagePath { get; set; }


    public enum FillDirection
    {
        Clockwise,
        AntiClockwise
    }

    public FillDirection FillDirection_ { get; set; }

    public enum GaugeShape
    {
        Circle,
        Rectangle,
        Polygon
    }
    public GaugeShape GaugeShape_ { get; set; }
    
    public float AngleStrokeDegree { get; set; }
    public float AngleSegmentUnitDegree { get; set; }
    public int FillCount { get; set; }
    public int SpaceCount { get; set; }
    public float WidthHeightRatio { get; set; }


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

    private float Radius => (Width > Height) ? Width / 2 : Height / 2;

    public VisualElement RadialFill;
    public VisualElement OverlayImage;

    [UnityEngine.Scripting.Preserve]

    public new class UxmlFactory : UxmlFactory<CircularGaugeCustomControl, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlFloatAttributeDescription m_value = new UxmlFloatAttributeDescription() { name = "value", defaultValue = 1f };
        UxmlFloatAttributeDescription m_width = new UxmlFloatAttributeDescription() { name = "width", defaultValue = 20f };
        UxmlFloatAttributeDescription m_height = new UxmlFloatAttributeDescription() { name = "height", defaultValue = 20f };
        UxmlFloatAttributeDescription m_angleOffset = new UxmlFloatAttributeDescription() { name = "angle-offset", defaultValue = 0 };
        UxmlFloatAttributeDescription m_thickness = new UxmlFloatAttributeDescription() { name = "thickness", defaultValue = 30f };
        UxmlBoolAttributeDescription m_uniformThickness = new UxmlBoolAttributeDescription() { name = "uniform-thickness", defaultValue = true };
        UxmlColorAttributeDescription m_startColor = new UxmlColorAttributeDescription() { name = "start-color", defaultValue = Color.white };
        UxmlColorAttributeDescription m_endColor = new UxmlColorAttributeDescription() { name = "end-color", defaultValue = Color.white };

        UxmlBoolAttributeDescription m_isBlink = new UxmlBoolAttributeDescription() { name = "is-blink", defaultValue = false };
        UxmlColorAttributeDescription m_blinkColor = new UxmlColorAttributeDescription() { name = "blink-color", defaultValue = new Color(0,0,0,0) };
        UxmlFloatAttributeDescription m_blinkOnDurationSec = new UxmlFloatAttributeDescription() { name = "blink-on-duration-sec", defaultValue = 0.1f };
        UxmlFloatAttributeDescription m_blinkOffDurationSec = new UxmlFloatAttributeDescription() { name = "blink-off-duration-sec", defaultValue = 0.1f };

        UxmlColorAttributeDescription m_headTipColor = new UxmlColorAttributeDescription() { name = "head-tip-color", defaultValue = Color.white };
        UxmlBoolAttributeDescription m_isColorPositionFixed = new UxmlBoolAttributeDescription() { name = "is-color-position-fixed", defaultValue = true };
        UxmlFloatAttributeDescription m_overlayImageScale = new UxmlFloatAttributeDescription() { name = "overlay-image-scale", defaultValue = 1f };
        UxmlEnumAttributeDescription<FillDirection> m_fillDirection = new UxmlEnumAttributeDescription<FillDirection>() { name = "fill-direction", defaultValue = 0 };
        UxmlEnumAttributeDescription<GaugeShape> m_gaugeShape = new UxmlEnumAttributeDescription<GaugeShape>() { name = "gauge-shape", defaultValue = GaugeShape.Circle };

        UxmlFloatAttributeDescription m_angleStrokeDegree = new UxmlFloatAttributeDescription() { name = "angle-stroke-degree", defaultValue = 360f };
        UxmlFloatAttributeDescription m_angleSegmentUnitDegree = new UxmlFloatAttributeDescription() { name = "angle-segment-unit-degree", defaultValue = 10f };
        UxmlIntAttributeDescription m_fillCount = new UxmlIntAttributeDescription() { name = "fill-count", defaultValue = 3 };
        UxmlIntAttributeDescription m_spaceCount = new UxmlIntAttributeDescription() { name = "space-count", defaultValue = 1 };
        UxmlFloatAttributeDescription m_widthHeightRatio = new UxmlFloatAttributeDescription() { name = "width-height-ratio", defaultValue = 1f };

        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }

        }
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var ate = ve as CircularGaugeCustomControl;

            // Assigning uxml attributes to c# properties
            ate.Value = m_value.GetValueFromBag(bag, cc);
            ate.Width = m_width.GetValueFromBag(bag, cc);
            ate.Height = m_height.GetValueFromBag(bag, cc);
            ate.StartColor = m_startColor.GetValueFromBag(bag, cc);
            ate.EndColor = m_endColor.GetValueFromBag(bag, cc);

            ate.IsBlink = m_isBlink.GetValueFromBag(bag, cc);
            ate.BlinkColor = m_blinkColor.GetValueFromBag(bag, cc);
            ate.BlinkOnDurationSec = m_blinkOnDurationSec.GetValueFromBag(bag, cc);
            ate.BlinkOffDurationSec = m_blinkOffDurationSec.GetValueFromBag(bag, cc);


            ate.HeadTipColor = m_headTipColor.GetValueFromBag(bag, cc);
            ate.IsColorPositionFixed = m_isColorPositionFixed.GetValueFromBag(bag, cc);
            ate.OverlayImageScale = m_overlayImageScale.GetValueFromBag(bag, cc);
            ate.AngleOffset = m_angleOffset.GetValueFromBag(bag, cc);
            ate.Thickness = m_thickness.GetValueFromBag(bag, cc);
            ate.UniformThickness = m_uniformThickness.GetValueFromBag(bag, cc);
            ate.FillDirection_ = m_fillDirection.GetValueFromBag(bag, cc);
            ate.GaugeShape_ = m_gaugeShape.GetValueFromBag(bag, cc);
            ate.AngleStrokeDegree = m_angleStrokeDegree.GetValueFromBag(bag, cc);
            ate.AngleSegmentUnitDegree = m_angleSegmentUnitDegree.GetValueFromBag(bag, cc);
            ate.FillCount = m_fillCount.GetValueFromBag(bag, cc);
            ate.SpaceCount = m_spaceCount.GetValueFromBag(bag, cc);
            ate.WidthHeightRatio = m_widthHeightRatio.GetValueFromBag(bag, cc);

            // Creating the hierarchy for the radial fill element
            ate.name = "circular-gauge";
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

    public CircularGaugeCustomControl() : base()
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
        float angleStrokeRad = AngleStrokeDegree * Mathf.Deg2Rad;
        float currentAngleRad = Value * angleStrokeRad;
        float angleRadUnit = AngleSegmentUnitDegree * Mathf.Deg2Rad;
        ushort squadNum = (ushort)(Mathf.Ceil(currentAngleRad / angleRadUnit));
        int vertCount = (squadNum + 1) * 2;
        int indiceCount = 0;
        for (int idx = 0; idx < squadNum; idx++)
        {
            if ((idx) % (FillCount + SpaceCount) < FillCount) indiceCount += 6;
        }
        Debug.Log($"squadNum:{squadNum}, vertCount:{vertCount}, indiceCount:{indiceCount}");
        //int indiceCount = (int)(Mathf.Ceil(squadNum / 2f) * 2 * 3);//squadNum 2 -> 1개만 씀 3 -> 2개 4 -> 2개 5->3개   squadNum/2

        float angleOffsetRad = AngleOffset * Mathf.Deg2Rad;

        // Create our MeshWriteData object, allocate the least amount of vertices and triangle indices required
        MeshWriteData mwd = mgc.Allocate(vertCount, indiceCount);
        Vector3 origin = new Vector3((float)Width / 2, (float)Height / 2, 0);


        //Rect
        switch (GaugeShape_)
        {
            case GaugeShape.Circle:
                {
                    for (int idx = 0; idx < squadNum; idx++)
                    {
                        float theta = angleRadUnit * idx + angleOffsetRad;
                        float outerRadiusByTheta = Radius;
                        float cosSqr = Mathf.Cos(theta) * Mathf.Cos(theta);
                        float sinSqr = Mathf.Sin(theta) * Mathf.Sin(theta);
                        float innerRadiusByTheta = Radius - Thickness;
                        Vector3 innerCoord = new Vector3(Mathf.Cos(theta) * innerRadiusByTheta,
                                                            Mathf.Sin(theta) * innerRadiusByTheta, Vertex.nearZ);
                        Vector3 outerCoord = new Vector3(Mathf.Cos(theta) * outerRadiusByTheta,
                                                            Mathf.Sin(theta) * outerRadiusByTheta, Vertex.nearZ);
                        if (WidthHeightRatio > 1)
                        {
                            outerCoord.y /= WidthHeightRatio;
                            innerCoord.y /= ((WidthHeightRatio *Radius-Thickness)/(Radius-Thickness));//inner ratio = (R*ratio) / r; r = R - t
                        }
                        else
                        {
                            outerCoord.x *= WidthHeightRatio;
                            innerCoord.x *= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                        }
                        var color = Color.Lerp(StartColor, EndColor, (float)angleRadUnit * idx / angleStrokeRad);
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

                    float theta2 = currentAngleRad + angleOffsetRad;
                    float outerRadiusByTheta2 = Radius;
                    float cosSqr2 = Mathf.Cos(theta2) * Mathf.Cos(theta2);
                    float sinSqr2 = Mathf.Sin(theta2) * Mathf.Sin(theta2);
                    float innerRadiusByTheta2 = Radius - Thickness;
                    Vector3 innerCoord2 = new Vector3(Mathf.Cos(theta2) * innerRadiusByTheta2,
                                                            Mathf.Sin(theta2) * innerRadiusByTheta2, Vertex.nearZ);
                    Vector3 outerCoord2 = new Vector3(Mathf.Cos(theta2) * outerRadiusByTheta2,
                                                        Mathf.Sin(theta2) * outerRadiusByTheta2, Vertex.nearZ);
                    if (WidthHeightRatio > 1)
                    {
                        outerCoord2.y /= WidthHeightRatio;
                        innerCoord2.y /= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                    }
                    else
                    {
                        outerCoord2.x *= WidthHeightRatio;
                        innerCoord2.x *= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                    }
                    mwd.SetNextVertex(new Vertex()
                    {
                        position = origin + innerCoord2,
                        //tint = Color.Lerp(startColor, endColor, (float)totalAngleRad / angleStrokeRad)
                        tint = HeadTipColor

                    });
                    mwd.SetNextVertex(new Vertex()
                    {
                        position = origin + outerCoord2,
                        tint = HeadTipColor

                    });
                    break;
                }
            case GaugeShape.Rectangle:
                {
                    for (int idx = 0; idx < squadNum; idx++)
                    {
                        float theta = angleRadUnit * idx + angleOffsetRad;
                        float outerRadiusByTheta;
                        float innerRadiusByTheta;
                        if (Mathf.Abs(Mathf.Sin(theta)) < Mathf.Abs(Mathf.Cos(theta)))
                        {
                            outerRadiusByTheta = Mathf.Abs(Radius / Mathf.Cos(theta) / Mathf.Sqrt(2));
                            innerRadiusByTheta = Mathf.Abs((Radius - Thickness) / Mathf.Cos(theta) / Mathf.Sqrt(2));
                        }
                        else
                        {
                            outerRadiusByTheta = Mathf.Abs(Radius / Mathf.Sin(theta) / Mathf.Sqrt(2));
                            innerRadiusByTheta = Mathf.Abs((Radius - Thickness) / Mathf.Sin(theta) / Mathf.Sqrt(2));

                        }
                        Vector3 innerCoord = new Vector3(Mathf.Cos(theta) * innerRadiusByTheta,
                                                            Mathf.Sin(theta) * innerRadiusByTheta, Vertex.nearZ);
                        Vector3 outerCoord = new Vector3(Mathf.Cos(theta) * outerRadiusByTheta,
                                                            Mathf.Sin(theta) * outerRadiusByTheta, Vertex.nearZ);
                        if (WidthHeightRatio > 1)
                        {
                            outerCoord.y /= WidthHeightRatio;
                            innerCoord.y /= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                        }
                        else
                        {
                            outerCoord.x *= WidthHeightRatio;
                            innerCoord.x *= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                        }


                        var color = Color.Lerp(StartColor, EndColor, (float)angleRadUnit * idx / angleStrokeRad);
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

                    float theta2 = currentAngleRad + angleOffsetRad;
                    float outerRadiusByTheta2;
                    float innerRadiusByTheta2;
                    if (Mathf.Abs(Mathf.Sin(theta2)) < Mathf.Abs(Mathf.Cos(theta2)))

                    {
                        outerRadiusByTheta2 = Mathf.Abs(Radius / Mathf.Cos(theta2) / Mathf.Sqrt(2));
                        innerRadiusByTheta2 = Mathf.Abs((Radius - Thickness) / Mathf.Cos(theta2) / Mathf.Sqrt(2));
                    }
                    else
                    {
                        outerRadiusByTheta2 = Mathf.Abs(Radius / Mathf.Sin(theta2) / Mathf.Sqrt(2));
                        innerRadiusByTheta2 = Mathf.Abs((Radius - Thickness) / Mathf.Sin(theta2) / Mathf.Sqrt(2));

                    }
                    Vector3 innerCoord2 = new Vector3(Mathf.Cos(theta2) * innerRadiusByTheta2,
                                                            Mathf.Sin(theta2) * innerRadiusByTheta2, Vertex.nearZ);
                    Vector3 outerCoord2 = new Vector3(Mathf.Cos(theta2) * outerRadiusByTheta2,
                                                        Mathf.Sin(theta2) * outerRadiusByTheta2, Vertex.nearZ);
                    if (WidthHeightRatio > 1)
                    {
                        outerCoord2.y /= WidthHeightRatio;
                        innerCoord2.y /= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                    }
                    else
                    {
                        outerCoord2.x *= WidthHeightRatio;
                        innerCoord2.x *= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                    }
                    mwd.SetNextVertex(new Vertex()
                    {
                        position = origin + innerCoord2,
                        //tint = Color.Lerp(startColor, endColor, (float)totalAngleRad / angleStrokeRad)
                        tint = HeadTipColor

                    });
                    mwd.SetNextVertex(new Vertex()
                    {
                        position = origin + outerCoord2,
                        tint = HeadTipColor

                    });
                    break;
                }
            case GaugeShape.Polygon:
                {

                    for (int idx = 0; idx < squadNum; idx++)
                    {
                        float theta = angleRadUnit * idx + angleOffsetRad;
                        float localTheta = theta%(Mathf.PI / 5);
                        float outerRadiusByTheta = Radius * Mathf.Cos(localTheta);
                        float cosSqr = Mathf.Cos(theta) * Mathf.Cos(theta);
                        float sinSqr = Mathf.Sin(theta) * Mathf.Sin(theta);
                        float innerRadiusByTheta = Radius * Mathf.Cos(localTheta) - Thickness;
                        Vector3 innerCoord = new Vector3(Mathf.Cos(theta) * innerRadiusByTheta,
                                                            Mathf.Sin(theta) * innerRadiusByTheta, Vertex.nearZ);
                        Vector3 outerCoord = new Vector3(Mathf.Cos(theta) * outerRadiusByTheta,
                                                            Mathf.Sin(theta) * outerRadiusByTheta, Vertex.nearZ);
                        if (WidthHeightRatio > 1)
                        {
                            outerCoord.y /= WidthHeightRatio;
                            innerCoord.y /= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));//inner ratio = (R*ratio) / r; r = R - t
                        }
                        else
                        {
                            outerCoord.x *= WidthHeightRatio;
                            innerCoord.x *= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                        }
                        var color = Color.Lerp(StartColor, EndColor, (float)angleRadUnit * idx / angleStrokeRad);
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

                    float theta2 = currentAngleRad + angleOffsetRad;
                    float localTheta2 = theta2 % (Mathf.PI / 5);

                    float outerRadiusByTheta2 = Radius * Mathf.Cos(localTheta2);
                    float cosSqr2 = Mathf.Cos(theta2) * Mathf.Cos(theta2);
                    float sinSqr2 = Mathf.Sin(theta2) * Mathf.Sin(theta2);
                    float innerRadiusByTheta2 = Radius * Mathf.Cos(localTheta2) - Thickness;
                    Vector3 innerCoord2 = new Vector3(Mathf.Cos(theta2) * innerRadiusByTheta2,
                                                            Mathf.Sin(theta2) * innerRadiusByTheta2, Vertex.nearZ);
                    Vector3 outerCoord2 = new Vector3(Mathf.Cos(theta2) * outerRadiusByTheta2,
                                                        Mathf.Sin(theta2) * outerRadiusByTheta2, Vertex.nearZ);
                    if (WidthHeightRatio > 1)
                    {
                        outerCoord2.y /= WidthHeightRatio;
                        innerCoord2.y /= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                    }
                    else
                    {
                        outerCoord2.x *= WidthHeightRatio;
                        innerCoord2.x *= ((WidthHeightRatio * Radius - Thickness) / (Radius - Thickness));
                    }
                    mwd.SetNextVertex(new Vertex()
                    {
                        position = origin + innerCoord2,
                        //tint = Color.Lerp(startColor, endColor, (float)totalAngleRad / angleStrokeRad)
                        tint = HeadTipColor

                    });
                    mwd.SetNextVertex(new Vertex()
                    {
                        position = origin + outerCoord2,
                        tint = HeadTipColor

                    });
                    break;
                }
        }

        //0  1
        //2  3
        //4  5
        //6  7
        for (int idx = 0; idx < squadNum; idx++)
        {
            if ((idx) % (FillCount + SpaceCount) < FillCount)
            {
                mwd.SetNextIndex((ushort)(idx * 2 + 0));
                mwd.SetNextIndex((ushort)(idx * 2 + 1));
                mwd.SetNextIndex((ushort)(idx * 2 + 3));

                mwd.SetNextIndex((ushort)(idx * 2 + 0));
                mwd.SetNextIndex((ushort)(idx * 2 + 3));
                mwd.SetNextIndex((ushort)(idx * 2 + 2));
            }
        }
    }

}
