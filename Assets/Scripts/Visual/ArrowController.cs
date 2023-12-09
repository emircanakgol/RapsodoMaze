using System;
using Controllers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Visual
{
    [RequireComponent(typeof(RawImage))]
    public class ArrowController : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        
        private Transform _follow;
        private RawImage _img;
        private bool _startFollowing;
        
        private void OnEnable() {
            MazeGenerator.OnMazeGeneratedCallback += Init;
        }

        private void OnDisable() {
            MazeGenerator.OnMazeGeneratedCallback -= Init;
        }

        void Init(MazeGenerator mazeGenerator) {
            _follow = mazeGenerator.endCellTransform;
            _img = GetComponent<RawImage>();
            _img.enabled = false;
            _startFollowing = true;
        }
        void LateUpdate(){
            if(!_startFollowing) return;
            Vector3 screenPos = cam.WorldToViewportPoint(_follow.position);
            Vector3 screenPosPla = cam.WorldToViewportPoint(cam.transform.position);
            if(screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1){
                _img.enabled = false;
                return;
            }
            _img.enabled = true;

            float width = cam.pixelWidth;
            float height = cam.pixelHeight;
            float arrowX = Mathf.Clamp(screenPos.x * width, 0, width);
            float arrowY = Mathf.Clamp(screenPos.y * height, 0, height);

            float plaX = Mathf.Clamp(screenPosPla.x * width, 0, width);
            float plaY = Mathf.Clamp(screenPosPla.y * height, 0, height);

            Vector2 arrowPos = new Vector2(arrowX, arrowY);
            Vector3 arrowRot = new Vector3(0,0,Mathf.Atan2(arrowY - plaY, arrowX - plaX)*180/Mathf.PI + 225);
            transform.position = arrowPos;
            transform.rotation = Quaternion.Euler(arrowRot);
        }
    }
}
