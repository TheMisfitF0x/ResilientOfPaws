/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Oculus.Interaction.Samples
{
    public class Poseuse : MonoBehaviour
    {
        [SerializeField, Interface(typeof(IHmd))]
        private UnityEngine.Object _hmd;
        private IHmd Hmd { get; set; }

        [SerializeField]
        private ActiveStateSelector[] _poses;

        public GameObject cubeGameObject; // Reference to the cube GameObject with Animator attached
        private Animator cubeAnimator; // Reference to the Animator component of the cube

        protected virtual void Awake()
        {
            Hmd = _hmd as IHmd;
        }

        protected virtual void Start()
        {
            this.AssertField(Hmd, nameof(Hmd));

            for (int i = 0; i < _poses.Length; i++)
            {
                int poseNumber = i;
                _poses[i].WhenSelected += () => HandleSelectedPose(poseNumber);
                _poses[i].WhenUnselected += () => HandleUnselectedPose(poseNumber);
            }

            // Get the Animator component of the cubeGameObject
            cubeAnimator = cubeGameObject.GetComponent<Animator>();
        }

        private void HandleSelectedPose(int poseNumber)
        {
            // Trigger the corresponding animation based on the pose number
            switch (poseNumber)
            {
                case 0:
                    cubeAnimator.SetTrigger("Ismove");
                    break;
                case 1:
                    cubeAnimator.SetTrigger("Isrotate");
                    break;
                // Add more cases for other poses and their respective animations
                // ...
            }
        }

        private void HandleUnselectedPose(int poseNumber)
        {
            // Add code to handle when a pose is unselected (if needed for your scenario)
            // ...
        }
    }
}
