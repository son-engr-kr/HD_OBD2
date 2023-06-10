using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class CustomControlTest : VisualElement
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

    public new class UxmlFactory : UxmlFactory<CustomControlTest, UxmlTraits> { }

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
            var ate = ve as CustomControlTest;

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
            ate.name = "custom-control-test";
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

    public CustomControlTest() : base()
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
        float totalAngleRad = ((m_value * 360)) * Mathf.Deg2Rad;
        float angleRadUnit = 5f * Mathf.Deg2Rad;
        ushort squadNum = (ushort)(Mathf.Ceil(totalAngleRad / angleRadUnit));
        int vertCount = (squadNum + 1) * 2;
        int indiceCount = (int)(Mathf.Ceil(squadNum/2f) * 2 * 3);//squadNum 2 -> 1개만 씀 3 -> 2개 4 -> 2개 5->3개   squadNum/2

        // Create our MeshWriteData object, allocate the least amount of vertices and triangle indices required
        MeshWriteData mwd = mgc.Allocate(vertCount, indiceCount);
        Vector3 origin = new Vector3((float)width / 2, (float)height / 2, 0);

        float innerRadius = radius - thickness;

        for (int idx = 0; idx < squadNum; idx++)
        {
            mwd.SetNextVertex(new Vertex()
            {
                position = origin + new Vector3(Mathf.Cos(angleRadUnit * idx) * innerRadius,
                                                                             Mathf.Sin(angleRadUnit * idx) * innerRadius, Vertex.nearZ),
                tint = Color.Lerp(startColor, endColor, (float)idx / squadNum)
            }); ;
            mwd.SetNextVertex(new Vertex()
            {
                position = origin + new Vector3(Mathf.Cos(angleRadUnit * idx) * radius,
                                                                             Mathf.Sin(angleRadUnit * idx) * radius, Vertex.nearZ),
                tint = Color.Lerp(startColor, endColor, (float)idx / squadNum)

            });
        }
        mwd.SetNextVertex(new Vertex()
        {
            position = origin + new Vector3(Mathf.Cos(totalAngleRad) * innerRadius,
                                                                             Mathf.Sin(totalAngleRad) * innerRadius, Vertex.nearZ),
            tint = endColor
        });
        mwd.SetNextVertex(new Vertex()
        {
            position = origin + new Vector3(Mathf.Cos(totalAngleRad) * radius,
                                                                         Mathf.Sin(totalAngleRad) * radius, Vertex.nearZ),
            tint = endColor
        });
        //0  1
        //2  3
        //4  5
        //6  7
        Debug.Log($"squadNum:{squadNum}, vertCount:{vertCount}, indiceCount:{indiceCount}");
        for (ushort idx = 0; idx < indiceCount/6; idx++)
        {
            //if (idx * 2 + 0 >= vertCount-1 || (idx+1)*6>=indiceCount-1) continue;

            //if (idx * 2 + 3 < vertCount) continue;
            mwd.SetNextIndex((ushort)(idx * 4 + 0));
            mwd.SetNextIndex((ushort)(idx * 4 + 1));
            mwd.SetNextIndex((ushort)(idx * 4 + 3));

            mwd.SetNextIndex((ushort)(idx * 4 + 0));
            mwd.SetNextIndex((ushort)(idx * 4 + 3));
            mwd.SetNextIndex((ushort)(idx * 4 + 2));

        }
    }
    public void OnGenerateVisualContent_unit(MeshGenerationContext mgc)
    {
        if(m_value < 0 ) return;
        float totalAngleRad = ((m_value * 360)) * Mathf.Deg2Rad;
        float angleRadUnit = 1f * Mathf.Deg2Rad;
        ushort squadNum = (ushort)(Mathf.Ceil(totalAngleRad / angleRadUnit)+1);
        int vertCount = (squadNum+1) * 2;
        int indiceCount = squadNum*2*3;

        // Create our MeshWriteData object, allocate the least amount of vertices and triangle indices required
        MeshWriteData mwd = mgc.Allocate(vertCount, indiceCount);
        Vector3 origin = new Vector3((float)width / 2, (float)height / 2, 0);



        for (int idx = 0; idx < squadNum; idx++)
        {
            mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(angleRadUnit * idx) * radius / 2f,
                                                                             Mathf.Sin(angleRadUnit * idx) * radius / 2f, Vertex.nearZ), tint = startColor });
            mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(angleRadUnit * idx) * radius,
                                                                             Mathf.Sin(angleRadUnit * idx) * radius, Vertex.nearZ), tint = startColor });
        }
        mwd.SetNextVertex(new Vertex()
        {
            position = origin + new Vector3(Mathf.Cos(totalAngleRad) * radius / 2f,
                                                                             Mathf.Sin(totalAngleRad) * radius / 2f, Vertex.nearZ),
            tint = startColor
        });
        mwd.SetNextVertex(new Vertex()
        {
            position = origin + new Vector3(Mathf.Cos(totalAngleRad) * radius,
                                                                         Mathf.Sin(totalAngleRad) * radius, Vertex.nearZ),
            tint = startColor
        });
        //0  1
        //2  3
        //4  5
        //6  7
        for (ushort idx = 0; idx < squadNum; idx++)
        {
            mwd.SetNextIndex((ushort)(idx * 2 + 0));
            mwd.SetNextIndex((ushort)(idx * 2 + 1));
            mwd.SetNextIndex((ushort)(idx * 2 + 3));

            mwd.SetNextIndex((ushort)(idx * 2 + 0));
            mwd.SetNextIndex((ushort)(idx * 2 + 3));
            mwd.SetNextIndex((ushort)(idx * 2 + 2));

        }
    }
    public void OnGenerateVisualContent_fixed(MeshGenerationContext mgc)
    {
        int vertCount = 200;
        int indiceCount = 300;

        // Create our MeshWriteData object, allocate the least amount of vertices and triangle indices required
        MeshWriteData mwd = mgc.Allocate(vertCount, indiceCount);
        Vector3 origin = new Vector3((float)width / 2, (float)height / 2, 0);

        float degrees = ((m_value * 360)) / Mathf.Rad2Deg;


        for (int idx = 0; idx < 100; idx ++)
        {
            mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(degrees/100f* idx) * radius/2f, Mathf.Sin(degrees / 100f * idx) * radius/2f, Vertex.nearZ), tint = startColor });
            mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(degrees / 100f * idx) * radius, Mathf.Sin(degrees / 100f * idx) * radius, Vertex.nearZ), tint = startColor });
        }

        //0  1
        //2  3
        //4  5
        //6  7
        for (ushort idx = 0; idx < 100/2; idx++)
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
