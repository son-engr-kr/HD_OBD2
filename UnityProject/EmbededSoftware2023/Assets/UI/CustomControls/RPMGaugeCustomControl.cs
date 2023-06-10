using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
public class RPMGaugeCustomControl : VisualElement
{
    protected float m_value = float.NaN;
    public void SetValueWithoutNotify(float newValue)
    {
        m_value = newValue;
        radialFill.MarkDirtyRepaint();
    }

    public float _value
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
    public float width { get; set; }
    public float height { get; set; }
    public Color startColor { get; set; }
    public Color endColor { get; set; }
    public float angleOffset { get; set; }
    public float thickness { get; set; }
    public string overlayImagePath { get; set; }

    public enum FillDirection
    {
        Clockwise,
        AntiClockwise
    }

    public FillDirection fillDirection { get; set; }
    private float m_overlayImageScale;
    public float overlayImageScale
    {
        get
        {
            m_overlayImageScale = Mathf.Clamp(m_overlayImageScale, 0, 1);
            return m_overlayImageScale;
        }
        set => m_overlayImageScale = value;
    }

    private float radius => (width > height) ? width / 2 : height / 2;

    public VisualElement radialFill;
    public VisualElement overlayImage;
    [UnityEngine.Scripting.Preserve]

    public new class UxmlFactory : UxmlFactory<RPMGaugeCustomControl, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlFloatAttributeDescription m_value = new UxmlFloatAttributeDescription() { name = "value", defaultValue = 1f };
        UxmlFloatAttributeDescription m_width = new UxmlFloatAttributeDescription() { name = "width", defaultValue = 20f };
        UxmlFloatAttributeDescription m_height = new UxmlFloatAttributeDescription() { name = "height", defaultValue = 20f };
        UxmlFloatAttributeDescription m_angleOffset = new UxmlFloatAttributeDescription() { name = "angle-offset", defaultValue = 0 };
        UxmlFloatAttributeDescription m_thickness = new UxmlFloatAttributeDescription() { name = "thickness", defaultValue = 30f };
        UxmlColorAttributeDescription m_startColor = new UxmlColorAttributeDescription() { name = "start-color", defaultValue = Color.white };
        UxmlColorAttributeDescription m_endColor = new UxmlColorAttributeDescription() { name = "end-color", defaultValue = Color.white };
        UxmlStringAttributeDescription m_overlayImagePath = new UxmlStringAttributeDescription() { name = "overlay-image-path", defaultValue = "" };
        UxmlFloatAttributeDescription m_overlayImageScale = new UxmlFloatAttributeDescription() { name = "overlay-image-scale", defaultValue = 1f };
        UxmlEnumAttributeDescription<FillDirection> m_fillDirection = new UxmlEnumAttributeDescription<FillDirection>() { name = "fill-direction", defaultValue = 0 };

        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }

        }
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var ate = ve as RPMGaugeCustomControl;

            // Assigning uxml attributes to c# properties
            ate._value = m_value.GetValueFromBag(bag, cc);
            ate.width = m_width.GetValueFromBag(bag, cc);
            ate.height = m_height.GetValueFromBag(bag, cc);
            ate.startColor = m_startColor.GetValueFromBag(bag, cc);
            ate.endColor = m_endColor.GetValueFromBag(bag, cc);
            ate.overlayImagePath = m_overlayImagePath.GetValueFromBag(bag, cc);
            ate.overlayImageScale = m_overlayImageScale.GetValueFromBag(bag, cc);
            ate.angleOffset = m_angleOffset.GetValueFromBag(bag, cc);
            ate.thickness = m_thickness.GetValueFromBag(bag, cc);
            ate.fillDirection = m_fillDirection.GetValueFromBag(bag, cc);

            // Creating the hierarchy for the radial fill element
            ate.name = "rpm-gauge";
            ate.Clear();
            VisualElement radialBoundary = new VisualElement() { name = "radial-boundary" };
            radialBoundary.Add(ate.radialFill);
            ate.radialFill.Add(ate.overlayImage);

            ate.radialFill.style.flexGrow = 1;
            ate.overlayImage.style.flexGrow = 1;
            ate.overlayImage.style.scale = new Scale(new Vector2(ate.overlayImageScale, ate.overlayImageScale));
            ate.style.height = ate.height;
            ate.style.width = ate.width;
            radialBoundary.style.height = ate.height;
            radialBoundary.style.width = ate.width;
            radialBoundary.style.overflow = Overflow.Hidden;
            ate.overlayImage.style.backgroundImage = null;
#if UNITY_EDITOR
            Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(ate.overlayImagePath);
            if (tex != null)
            {
                ate.overlayImage.style.backgroundImage = tex;
            }
#endif
            // Angle Offset determines the rotation of the radialFill VE, overlayImage will use the inverse of this
            // rotation so the image remains upright
            ate.radialFill.transform.rotation = Quaternion.Euler(0, 0, ate.angleOffset);
            ate.overlayImage.transform.rotation = Quaternion.Euler(0, 0, -ate.angleOffset);
            ate.Add(radialBoundary);
        }
    }

    public RPMGaugeCustomControl() : base()
    {
        radialFill = new VisualElement() { name = "radial-fill" };
        overlayImage = new VisualElement() { name = "overlay-image" };
        radialFill.generateVisualContent += OnGenerateVisualContent;
    }

    public void AngleUpdate(ChangeEvent<float> evt)
    {
        radialFill?.MarkDirtyRepaint();
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
        float angleStrokeDeg = 270f;
        float angleStrokeRad = angleStrokeDeg * Mathf.Deg2Rad;
        float totalAngleRad = ((m_value * angleStrokeDeg)) * Mathf.Deg2Rad;
        float angleRadUnit = 10f * Mathf.Deg2Rad;
        int fillCount = 3;//space한번 띄우고 몇번 채우기 할거냐
        ushort squadNum = (ushort)(Mathf.Ceil(totalAngleRad / angleRadUnit));
        int vertCount = (squadNum + 1) * 2;
        int indiceCount = 0;
        for(int idx = 0; idx < squadNum; idx++)
        {
            if ((idx+1) % (fillCount+1) != 0) indiceCount += 6;
        }
        Debug.Log($"squadNum:{squadNum}, vertCount:{vertCount}, indiceCount:{indiceCount}");
        //int indiceCount = (int)(Mathf.Ceil(squadNum / 2f) * 2 * 3);//squadNum 2 -> 1개만 씀 3 -> 2개 4 -> 2개 5->3개   squadNum/2

        float startAngle = 120 * Mathf.Deg2Rad;

        // Create our MeshWriteData object, allocate the least amount of vertices and triangle indices required
        MeshWriteData mwd = mgc.Allocate(vertCount, indiceCount);
        Vector3 origin = new Vector3((float)width / 2, (float)height / 2, 0);

        float innerRadius = radius - thickness;

        //Rect

        for (int idx = 0; idx < squadNum; idx++)
        {
            float theta = angleRadUnit * idx + startAngle;
            float outerRadiusByTheta = 0;
            float innerRadiusByTheta = 0;
            int valueForSegment = (int)Mathf.Floor(theta / (45 * Mathf.Deg2Rad)) % 8;
            if (Mathf.Abs(Mathf.Sin(theta)) < Mathf.Abs(Mathf.Cos(theta)))
            {
                outerRadiusByTheta = Mathf.Abs(radius/Mathf.Cos(theta)/Mathf.Sqrt(2));
                innerRadiusByTheta = Mathf.Abs((radius - thickness) / Mathf.Cos(theta) / Mathf.Sqrt(2));
            }
            else
            {
                outerRadiusByTheta = Mathf.Abs(radius / Mathf.Sin(theta) / Mathf.Sqrt(2));
                innerRadiusByTheta = Mathf.Abs((radius - thickness) / Mathf.Sin(theta) / Mathf.Sqrt(2));

            }
            var color = Color.Lerp(startColor, endColor, (float)angleRadUnit * idx / angleStrokeRad);
            if(_value > 0.5f && TimeManager.RectangularPulse(0.1f,0.1f))
            {
                color = Color.red;
            }
            mwd.SetNextVertex(new Vertex()
            {
                position = origin + new Vector3(Mathf.Cos(theta) * innerRadiusByTheta,
                                                Mathf.Sin(theta) * innerRadiusByTheta/2f, Vertex.nearZ),
                tint = color
            }); ;
            mwd.SetNextVertex(new Vertex()
            {
                position = origin + new Vector3(Mathf.Cos(theta) * outerRadiusByTheta,
                                                Mathf.Sin(theta) * outerRadiusByTheta/2f, Vertex.nearZ),
                tint = color


            });
        }
        float outerRadiusByTheta2 = 0;
        float innerRadiusByTheta2 = 0;
        float theta2 = totalAngleRad + startAngle;
        if (Mathf.Abs(Mathf.Sin(theta2)) < Mathf.Abs(Mathf.Cos(theta2)))

        {
            outerRadiusByTheta2 = Mathf.Abs(radius / Mathf.Cos(theta2) / Mathf.Sqrt(2));
            innerRadiusByTheta2 = Mathf.Abs((radius - thickness) / Mathf.Cos(theta2) / Mathf.Sqrt(2));
        }
        else
        {
            outerRadiusByTheta2 = Mathf.Abs(radius / Mathf.Sin(theta2) / Mathf.Sqrt(2));
            innerRadiusByTheta2 = Mathf.Abs((radius - thickness) / Mathf.Sin(theta2) / Mathf.Sqrt(2));

        }
        mwd.SetNextVertex(new Vertex()
        {
            position = origin + new Vector3(Mathf.Cos(theta2) * innerRadiusByTheta2,
                                            Mathf.Sin(theta2) * innerRadiusByTheta2/2f, Vertex.nearZ),
            //tint = Color.Lerp(startColor, endColor, (float)totalAngleRad / angleStrokeRad)
            tint = Color.white

        });
        mwd.SetNextVertex(new Vertex()
        {
            position = origin + new Vector3(Mathf.Cos(theta2) * outerRadiusByTheta2,
                                            Mathf.Sin(theta2) * outerRadiusByTheta2/2f, Vertex.nearZ),
            tint = Color.white

        });

        //just circle

        //for (int idx = 0; idx < squadNum; idx++)
        //{
        //    var color = Color.Lerp(startColor, endColor, (float)angleRadUnit * idx / angleStrokeRad);
        //    mwd.SetNextVertex(new Vertex()
        //    {
        //        position = origin + new Vector3(Mathf.Cos(angleRadUnit * idx+ startAngle) * innerRadius,
        //                                        Mathf.Sin(angleRadUnit * idx+ startAngle) * innerRadius, Vertex.nearZ),
        //        tint = color
        //    }); ;
        //    mwd.SetNextVertex(new Vertex()
        //    {
        //        position = origin + new Vector3(Mathf.Cos(angleRadUnit * idx+ startAngle) * radius,
        //                                        Mathf.Sin(angleRadUnit * idx+ startAngle) * radius, Vertex.nearZ),
        //        tint = color


        //    });
        //}
        //mwd.SetNextVertex(new Vertex()
        //{
        //    position = origin + new Vector3(Mathf.Cos(totalAngleRad+ startAngle) * innerRadius,
        //                                    Mathf.Sin(totalAngleRad+ startAngle) * innerRadius, Vertex.nearZ),
        //    tint = Color.Lerp(startColor, endColor, (float)totalAngleRad / angleStrokeRad)

        //});
        //mwd.SetNextVertex(new Vertex()
        //{
        //    position = origin + new Vector3(Mathf.Cos(totalAngleRad+ startAngle) * radius,
        //                                    Mathf.Sin(totalAngleRad+ startAngle) * radius, Vertex.nearZ),
        //    tint = Color.Lerp(startColor, endColor, (float)totalAngleRad / angleStrokeRad)

        //});


        //0  1
        //2  3
        //4  5
        //6  7
        for (int idx = 0; idx < squadNum; idx++)
        {
            if((idx + 1) % (fillCount + 1) != 0)
            {
                mwd.SetNextIndex((ushort)(idx * 2 + 0));
                mwd.SetNextIndex((ushort)(idx * 2 + 1));
                mwd.SetNextIndex((ushort)(idx * 2 + 3));

                mwd.SetNextIndex((ushort)(idx * 2 + 0));
                mwd.SetNextIndex((ushort)(idx * 2 + 3));
                mwd.SetNextIndex((ushort)(idx * 2 + 2));
            }
        }
        //for (ushort idx = 0; idx < indiceCount / 6; idx++)
        //{
        //    //if (idx * 2 + 0 >= vertCount-1 || (idx+1)*6>=indiceCount-1) continue;

        //    //if (idx * 2 + 3 < vertCount) continue;
        //    mwd.SetNextIndex((ushort)(idx * 4 + 0));
        //    mwd.SetNextIndex((ushort)(idx * 4 + 1));
        //    mwd.SetNextIndex((ushort)(idx * 4 + 3));

        //    mwd.SetNextIndex((ushort)(idx * 4 + 0));
        //    mwd.SetNextIndex((ushort)(idx * 4 + 3));
        //    mwd.SetNextIndex((ushort)(idx * 4 + 2));

        //}
    }
}
