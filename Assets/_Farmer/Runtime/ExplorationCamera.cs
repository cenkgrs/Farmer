using UnityEngine;
namespace Farmer
{
    // Keeps the established farm framing, follows outside it without changing the isometric angle.
    public sealed class ExplorationCamera : MonoBehaviour
    {
        [SerializeField] private Transform player;
        private Vector3 home,velocity;
        public void Configure(Transform target)=>player=target;
        private void Awake()=>home=transform.position;
        private void LateUpdate()
        {
            if(player==null)return;
            Vector3 p=player.position;float distance=Mathf.Max(Mathf.Abs(p.x),Mathf.Abs(p.z));
            float weight=Mathf.SmoothStep(0,1,Mathf.InverseLerp(7,12,distance));
            var desired=home+new Vector3(p.x,0,p.z)*weight;
            transform.position=Vector3.SmoothDamp(transform.position,desired,ref velocity,.25f);
        }
    }
}
