using UnityEngine;

namespace Farmer
{
    // Short lived world feedback. No global timeScale changes: menus and saving remain responsive.
    public sealed class CombatFeedback : MonoBehaviour
    {
        private float age;
        private bool slash;
        private TextMesh label;
        private Material material;
        private LineRenderer[] rays;
        public static void Burst(Vector3 position,string text,Color color)
        {
            var root=new GameObject("Combat impact");root.transform.position=position;
            var effect=root.AddComponent<CombatFeedback>();
            var words=new GameObject("Damage or reward");words.transform.SetParent(root.transform,false);
            effect.label=words.AddComponent<TextMesh>();effect.label.text=text;effect.label.fontSize=64;
            effect.label.characterSize=.045f;effect.label.anchor=TextAnchor.MiddleCenter;effect.label.color=color;
            effect.material=new Material(Shader.Find("Universal Render Pipeline/Lit"));effect.material.color=color;
            effect.rays=new LineRenderer[8];
            for(int i=0;i<8;i++)
            {
                var go=new GameObject("Impact streak");go.transform.SetParent(root.transform,false);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=effect.material;line.useWorldSpace=false;
                line.positionCount=2;line.startWidth=.055f;line.endWidth=.008f;effect.rays[i]=line;
            }
        }
        public static void Slash(Transform actor,Vector3 target)
        {
            var root=new GameObject("Axe sweep");root.transform.position=actor.position+Vector3.up*.85f;
            var direction=target-actor.position;direction.y=0;if(direction.sqrMagnitude>.01f)root.transform.rotation=Quaternion.LookRotation(direction);
            var effect=root.AddComponent<CombatFeedback>();effect.slash=true;
            effect.material=new Material(Shader.Find("Universal Render Pipeline/Lit"));effect.material.color=new Color(1,.9f,.6f);
            var line=root.AddComponent<LineRenderer>();line.sharedMaterial=effect.material;line.useWorldSpace=false;
            line.positionCount=16;line.startWidth=.1f;line.endWidth=.015f;effect.rays=new[]{line};
        }
        private void Update()
        {
            age+=Time.deltaTime;
            if(slash)
            {
                var line=rays[0];line.enabled=age>=.1f&&age<=.28f;
                float progress=Mathf.Clamp01((age-.1f)/.18f);
                for(int i=0;i<16;i++){float a=Mathf.Lerp(-75,75,progress)-(15-i)*3;float rad=a*Mathf.Deg2Rad;line.SetPosition(i,new Vector3(Mathf.Sin(rad)*1.45f,0,Mathf.Cos(rad)*1.45f));}
                if(age>.3f)Destroy(gameObject);return;
            }
            if(Camera.main!=null)label.transform.rotation=Camera.main.transform.rotation;
            label.transform.localPosition=Vector3.up*(.35f+age*.7f);
            var c=label.color;c.a=Mathf.Clamp01((1.1f-age)*3);label.color=c;
            for(int i=0;i<rays.Length;i++)
            {
                rays[i].enabled=age<.22f;
                float a=i*Mathf.PI/4;var dir=new Vector3(Mathf.Cos(a),Mathf.Sin(a),.2f);
                rays[i].SetPosition(0,dir*(.12f+age));rays[i].SetPosition(1,dir*(.45f+age*2));
            }
            if(age>1.1f)Destroy(gameObject);
        }
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
