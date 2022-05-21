using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

#if VRC_SDK_VRCSDK3
using VRC.Core;
using VRC.SDKBase;
#endif

#if VRC_SDK_VRCSDK3 && !UDON
using VRC_AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using VRC_SpatialAudioSource = VRC.SDK3.Avatars.Components.VRCSpatialAudioSource;
#endif

namespace Shadster.AvatarTools
{

    public class ShadstersAvatarTools : EditorWindow
    {



	}
}
