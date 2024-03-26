// -----------------------------------------------------------------------
// <copyright file="VoiceCodec.cs" company="Exit Games GmbH">
//   Photon Voice API Framework for Photon - Copyright (C) 2017 Exit Games GmbH
// </copyright>
// <summary>
//   Photon data streaming support.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Photon.Voice
{
    public enum FrameFlags : byte
    {
        Config = 1,
        KeyFrame = 2,
        PartialFrame = 4,
        EndOfStream = 8
    }
}
