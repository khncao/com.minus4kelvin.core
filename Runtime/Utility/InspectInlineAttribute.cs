// https://github.com/garettbass/UnityExtensions.InspectInline

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k
{

    public class InspectInlineAttribute : PropertyAttribute
    {
        // public bool canEditRemoteTarget;

        public bool canCreateSubasset = true;
    }

}
